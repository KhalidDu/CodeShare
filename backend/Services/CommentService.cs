using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.DTOs;
using CodeSnippetManager.Api.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeSnippetManager.Api.Services;

/// <summary>
/// 评论服务实现 - 遵循单一职责原则和依赖倒置原则
/// 提供评论相关的业务逻辑操作，包括CRUD、点赞、举报、审核等功能
/// </summary>
public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly ICommentLikeRepository _commentLikeRepository;
    private readonly ICommentReportRepository _commentReportRepository;
    private readonly ICommentValidationService _commentValidationService;
    private readonly ICommentAnalyticsService _commentAnalyticsService;
    private readonly ICommentModerationService _commentModerationService;
    private readonly ICommentReportManagementService _commentReportManagementService;
    private readonly IInputValidationService _inputValidationService;
    private readonly IPermissionService _permissionService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CommentService> _logger;

    // 缓存键前缀
    private const string COMMENT_CACHE_PREFIX = "comment_";
    private const string SNIPPET_COMMENTS_CACHE_PREFIX = "snippet_comments_";
    private const string USER_COMMENTS_CACHE_PREFIX = "user_comments_";
    private const string COMMENT_STATS_CACHE_PREFIX = "comment_stats_";

    public CommentService(
        ICommentRepository commentRepository,
        ICommentLikeRepository commentLikeRepository,
        ICommentReportRepository commentReportRepository,
        ICommentValidationService commentValidationService,
        ICommentAnalyticsService commentAnalyticsService,
        ICommentModerationService commentModerationService,
        ICommentReportManagementService commentReportManagementService,
        IInputValidationService inputValidationService,
        IPermissionService permissionService,
        ICacheService cacheService,
        ILogger<CommentService> logger)
    {
        _commentRepository = commentRepository ?? throw new ArgumentNullException(nameof(commentRepository));
        _commentLikeRepository = commentLikeRepository ?? throw new ArgumentNullException(nameof(commentLikeRepository));
        _commentReportRepository = commentReportRepository ?? throw new ArgumentNullException(nameof(commentReportRepository));
        _commentValidationService = commentValidationService ?? throw new ArgumentNullException(nameof(commentValidationService));
        _commentAnalyticsService = commentAnalyticsService ?? throw new ArgumentNullException(nameof(commentAnalyticsService));
        _commentModerationService = commentModerationService ?? throw new ArgumentNullException(nameof(commentModerationService));
        _commentReportManagementService = commentReportManagementService ?? throw new ArgumentNullException(nameof(commentReportManagementService));
        _inputValidationService = inputValidationService ?? throw new ArgumentNullException(nameof(inputValidationService));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region 基础评论管理操作

    /// <summary>
    /// 创建新评论
    /// </summary>
    public async Task<CommentDto> CreateCommentAsync(CreateCommentDto createCommentDto, Guid userId)
    {
        try
        {
            _logger.LogInformation("用户 {UserId} 尝试创建评论", userId);

            // 验证输入数据
            await ValidateCommentInputAsync(createCommentDto);

            // 检查用户权限
            var canComment = await _permissionService.CanAccessSnippetAsync(userId, createCommentDto.SnippetId, PermissionOperation.Comment);
            if (!canComment)
            {
                _logger.LogWarning("用户 {UserId} 无权限对代码片段 {SnippetId} 发表评论", userId, createCommentDto.SnippetId);
                throw new UnauthorizedAccessException("您没有权限对此代码片段发表评论");
            }

            // 如果是回复评论，检查父评论是否存在
            if (createCommentDto.ParentId.HasValue)
            {
                var parentComment = await _commentRepository.GetByIdAsync(createCommentDto.ParentId.Value);
                if (parentComment == null)
                {
                    _logger.LogWarning("父评论 {ParentId} 不存在", createCommentDto.ParentId.Value);
                    throw new ArgumentException("父评论不存在");
                }
            }

            // 创建评论实体
            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                Content = createCommentDto.Content,
                SnippetId = createCommentDto.SnippetId,
                UserId = userId,
                ParentId = createCommentDto.ParentId,
                Depth = createCommentDto.ParentId.HasValue ? await GetCommentDepthAsync(createCommentDto.ParentId.Value) + 1 : 0,
                LikeCount = 0,
                ReplyCount = 0,
                Status = CommentStatus.Normal,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // 设置父路径
            if (createCommentDto.ParentId.HasValue)
            {
                comment.ParentPath = await GetParentPathAsync(createCommentDto.ParentId.Value);
            }

            // 保存评论
            var createdComment = await _commentRepository.CreateAsync(comment);

            // 如果是回复评论，更新父评论的回复数
            if (createCommentDto.ParentId.HasValue)
            {
                await _commentRepository.IncrementReplyCountAsync(createCommentDto.ParentId.Value);
            }

            // 清除相关缓存
            await ClearSnippetCommentCacheAsync(createCommentDto.SnippetId);
            await ClearUserCommentCacheAsync(userId);

            _logger.LogInformation("用户 {UserId} 成功创建评论 {CommentId}", userId, createdComment.Id);

            return await MapToCommentDtoAsync(createdComment, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建评论时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 更新评论内容
    /// </summary>
    public async Task<CommentDto> UpdateCommentAsync(Guid commentId, UpdateCommentDto updateCommentDto, Guid userId)
    {
        try
        {
            _logger.LogInformation("用户 {UserId} 尝试更新评论 {CommentId}", userId, commentId);

            // 验证输入数据
            if (string.IsNullOrWhiteSpace(updateCommentDto.Content))
            {
                throw new ArgumentException("评论内容不能为空");
            }

            // 获取现有评论
            var existingComment = await _commentRepository.GetByIdAsync(commentId);
            if (existingComment == null)
            {
                _logger.LogWarning("评论 {CommentId} 不存在", commentId);
                throw new ArgumentException("评论不存在");
            }

            // 检查用户权限
            var canEdit = await _commentRepository.CanUserEditCommentAsync(commentId, userId);
            if (!canEdit)
            {
                _logger.LogWarning("用户 {UserId} 无权限更新评论 {CommentId}", userId, commentId);
                throw new UnauthorizedAccessException("您没有权限更新此评论");
            }

            // 验证评论内容
            await _commentValidationService.ValidateCommentContentAsync(updateCommentDto.Content);

            // 更新评论内容
            existingComment.Content = updateCommentDto.Content;
            existingComment.UpdatedAt = DateTime.UtcNow;

            // 保存更新
            var updatedComment = await _commentRepository.UpdateAsync(existingComment);

            // 清除相关缓存
            await ClearCommentCacheAsync(commentId);
            await ClearSnippetCommentCacheAsync(existingComment.SnippetId);

            _logger.LogInformation("用户 {UserId} 成功更新评论 {CommentId}", userId, commentId);

            return await MapToCommentDtoAsync(updatedComment, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新评论时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 删除评论（软删除）
    /// </summary>
    public async Task<bool> DeleteCommentAsync(Guid commentId, Guid userId)
    {
        try
        {
            _logger.LogInformation("用户 {UserId} 尝试删除评论 {CommentId}", userId, commentId);

            // 获取现有评论
            var existingComment = await _commentRepository.GetByIdAsync(commentId);
            if (existingComment == null)
            {
                _logger.LogWarning("评论 {CommentId} 不存在", commentId);
                return false;
            }

            // 检查用户权限
            var canDelete = await _commentRepository.CanUserDeleteCommentAsync(commentId, userId);
            if (!canDelete)
            {
                _logger.LogWarning("用户 {UserId} 无权限删除评论 {CommentId}", userId, commentId);
                throw new UnauthorizedAccessException("您没有权限删除此评论");
            }

            // 执行软删除
            var result = await _commentRepository.SoftDeleteAsync(commentId);

            if (result)
            {
                // 如果是回复评论，更新父评论的回复数
                if (existingComment.ParentId.HasValue)
                {
                    await _commentRepository.DecrementReplyCountAsync(existingComment.ParentId.Value);
                }

                // 清除相关缓存
                await ClearCommentCacheAsync(commentId);
                await ClearSnippetCommentCacheAsync(existingComment.SnippetId);
                await ClearUserCommentCacheAsync(userId);

                _logger.LogInformation("用户 {UserId} 成功删除评论 {CommentId}", userId, commentId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除评论时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 获取评论详情
    /// </summary>
    public async Task<CommentDto?> GetCommentAsync(Guid commentId, Guid? currentUserId = null)
    {
        try
        {
            _logger.LogDebug("获取评论详情: {CommentId}", commentId);

            // 尝试从缓存获取
            var cacheKey = $"{COMMENT_CACHE_PREFIX}{commentId}";
            var cachedComment = await _cacheService.GetAsync<CommentDto>(cacheKey);
            if (cachedComment != null)
            {
                // 如果当前用户ID不同，更新点赞状态
                if (currentUserId.HasValue)
                {
                    cachedComment.IsLikedByCurrentUser = await IsCommentLikedByUserAsync(commentId, currentUserId.Value);
                    cachedComment.CanEdit = await _commentRepository.CanUserEditCommentAsync(commentId, currentUserId.Value);
                    cachedComment.CanDelete = await _commentRepository.CanUserDeleteCommentAsync(commentId, currentUserId.Value);
                    cachedComment.CanReport = !await HasUserReportedCommentAsync(commentId, currentUserId.Value);
                }
                return cachedComment;
            }

            // 从数据库获取
            var comment = await _commentRepository.GetByIdWithUserAsync(commentId);
            if (comment == null)
            {
                _logger.LogWarning("评论 {CommentId} 不存在", commentId);
                return null;
            }

            // 映射为DTO
            var commentDto = await MapToCommentDtoAsync(comment, currentUserId);

            // 缓存结果
            await _cacheService.SetAsync(cacheKey, commentDto, TimeSpan.FromMinutes(10));

            return commentDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取评论详情时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 分页获取评论列表
    /// </summary>
    public async Task<PaginatedResult<CommentDto>> GetCommentsAsync(CommentFilterDto filter, Guid? currentUserId = null)
    {
        try
        {
            _logger.LogDebug("获取评论列表: SnippetId={SnippetId}, Page={Page}, PageSize={PageSize}", 
                filter.SnippetId, filter.Page, filter.PageSize);

            // 获取评论列表
            var result = await _commentRepository.GetPagedAsync(filter);

            // 映射为DTO
            var commentDtos = new List<CommentDto>();
            foreach (var comment in result.Items)
            {
                commentDtos.Add(await MapToCommentDtoAsync(comment, currentUserId));
            }

            return new PaginatedResult<CommentDto>
            {
                Items = commentDtos,
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取评论列表时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 根据ID获取评论详情（别名方法）
    /// </summary>
    public async Task<CommentDto?> GetCommentByIdAsync(Guid commentId, Guid? currentUserId = null)
    {
        return await GetCommentAsync(commentId, currentUserId);
    }

    /// <summary>
    /// 分页获取评论列表（别名方法）
    /// </summary>
    public async Task<PaginatedResult<CommentDto>> GetCommentsPaginatedAsync(CommentFilterDto filter, Guid? currentUserId = null)
    {
        return await GetCommentsAsync(filter, currentUserId);
    }

    /// <summary>
    /// 获取代码片段的评论树结构
    /// </summary>
    public async Task<IEnumerable<CommentDto>> GetCommentTreeAsync(Guid snippetId, Guid? currentUserId = null)
    {
        try
        {
            _logger.LogDebug("获取代码片段 {SnippetId} 的评论树", snippetId);

            // 尝试从缓存获取
            var cacheKey = $"{SNIPPET_COMMENTS_CACHE_PREFIX}{snippetId}_tree";
            var cachedTree = await _cacheService.GetAsync<IEnumerable<CommentDto>>(cacheKey);
            if (cachedTree != null)
            {
                return cachedTree;
            }

            // 获取根评论
            var rootComments = await _commentRepository.GetRootCommentsBySnippetIdAsync(snippetId);
            
            // 构建评论树
            var commentTree = new List<CommentDto>();
            foreach (var rootComment in rootComments)
            {
                var commentDto = await MapToCommentDtoAsync(rootComment, currentUserId);
                commentDto.Replies = await GetCommentRepliesRecursiveAsync(rootComment.Id, currentUserId);
                commentTree.Add(commentDto);
            }

            // 缓存结果
            await _cacheService.SetAsync(cacheKey, commentTree, TimeSpan.FromMinutes(5));

            return commentTree;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取评论树时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 获取评论的回复列表
    /// </summary>
    public async Task<IEnumerable<CommentDto>> GetRepliesAsync(Guid parentId, Guid? currentUserId = null)
    {
        try
        {
            _logger.LogDebug("获取评论 {ParentId} 的回复列表", parentId);

            // 获取直接回复
            var replies = await _commentRepository.GetRepliesByParentIdAsync(parentId);
            
            // 映射为DTO
            var replyDtos = new List<CommentDto>();
            foreach (var reply in replies)
            {
                replyDtos.Add(await MapToCommentDtoAsync(reply, currentUserId));
            }

            return replyDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取回复列表时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 获取用户的评论列表
    /// </summary>
    public async Task<PaginatedResult<CommentDto>> GetUserCommentsAsync(Guid userId, CommentFilterDto filter, Guid? currentUserId = null)
    {
        try
        {
            _logger.LogDebug("获取用户 {UserId} 的评论列表", userId);

            // 获取用户评论
            var result = await _commentRepository.GetByUserIdAsync(userId, filter);

            // 映射为DTO
            var commentDtos = new List<CommentDto>();
            foreach (var comment in result.Items)
            {
                commentDtos.Add(await MapToCommentDtoAsync(comment, currentUserId));
            }

            return new PaginatedResult<CommentDto>
            {
                Items = commentDtos,
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户评论列表时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    #endregion

    #region 评论点赞管理

    /// <summary>
    /// 点赞评论
    /// </summary>
    public async Task<bool> LikeCommentAsync(Guid commentId, Guid userId)
    {
        try
        {
            _logger.LogInformation("用户 {UserId} 尝试点赞评论 {CommentId}", userId, commentId);

            // 检查评论是否存在
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null)
            {
                _logger.LogWarning("评论 {CommentId} 不存在", commentId);
                return false;
            }

            // 检查用户是否已点赞
            var hasLiked = await IsCommentLikedByUserAsync(commentId, userId);
            if (hasLiked)
            {
                _logger.LogWarning("用户 {UserId} 已经点赞过评论 {CommentId}", userId, commentId);
                return false;
            }

            // 创建点赞记录
            var commentLike = new CommentLike
            {
                Id = Guid.NewGuid(),
                CommentId = commentId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            // 保存点赞记录
            await _commentLikeRepository.CreateAsync(commentLike);

            // 更新评论点赞数
            await _commentRepository.IncrementLikeCountAsync(commentId);

            // 清除相关缓存
            await ClearCommentCacheAsync(commentId);
            await ClearSnippetCommentCacheAsync(comment.SnippetId);

            _logger.LogInformation("用户 {UserId} 成功点赞评论 {CommentId}", userId, commentId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "点赞评论时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 取消点赞评论
    /// </summary>
    public async Task<bool> UnlikeCommentAsync(Guid commentId, Guid userId)
    {
        try
        {
            _logger.LogInformation("用户 {UserId} 尝试取消点赞评论 {CommentId}", userId, commentId);

            // 检查用户是否已点赞
            var hasLiked = await IsCommentLikedByUserAsync(commentId, userId);
            if (!hasLiked)
            {
                _logger.LogWarning("用户 {UserId} 未点赞过评论 {CommentId}", userId, commentId);
                return false;
            }

            // 删除点赞记录
            var result = await _commentLikeRepository.DeleteAsync(commentId, userId);
            if (result)
            {
                // 更新评论点赞数
                await _commentRepository.DecrementLikeCountAsync(commentId);

                // 获取评论信息用于清除缓存
                var comment = await _commentRepository.GetByIdAsync(commentId);
                if (comment != null)
                {
                    await ClearCommentCacheAsync(commentId);
                    await ClearSnippetCommentCacheAsync(comment.SnippetId);
                }

                _logger.LogInformation("用户 {UserId} 成功取消点赞评论 {CommentId}", userId, commentId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取消点赞评论时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 获取评论的点赞列表
    /// </summary>
    public async Task<PaginatedResult<CommentLikeDto>> GetCommentLikesAsync(Guid commentId, int page = 1, int pageSize = 20)
    {
        try
        {
            _logger.LogDebug("获取评论 {CommentId} 的点赞列表", commentId);

            // 检查评论是否存在
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null)
            {
                _logger.LogWarning("评论 {CommentId} 不存在", commentId);
                return new PaginatedResult<CommentLikeDto>
                {
                    Items = new List<CommentLikeDto>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize
                };
            }

            // 获取点赞列表
            var result = await _commentLikeRepository.GetCommentLikesAsync(commentId, page, pageSize);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取评论点赞列表时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 获取用户的点赞历史
    /// </summary>
    public async Task<PaginatedResult<CommentLikeDto>> GetUserLikedCommentsAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        try
        {
            _logger.LogDebug("获取用户 {UserId} 的点赞历史", userId);

            var result = await _commentLikeRepository.GetUserLikedCommentsAsync(userId, page, pageSize);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户点赞历史时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 检查用户是否已点赞评论
    /// </summary>
    public async Task<bool> IsCommentLikedByUserAsync(Guid commentId, Guid userId)
    {
        try
        {
            return await _commentLikeRepository.IsLikedByUserAsync(commentId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查用户是否已点赞评论时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    #endregion

    #region 评论举报管理

    /// <summary>
    /// 举报评论
    /// </summary>
    public async Task<Guid> ReportCommentAsync(CreateCommentReportDto createReportDto, Guid userId)
    {
        try
        {
            _logger.LogInformation("用户 {UserId} 尝试举报评论 {CommentId}", userId, createReportDto.CommentId);

            // 检查评论是否存在
            var comment = await _commentRepository.GetByIdAsync(createReportDto.CommentId);
            if (comment == null)
            {
                _logger.LogWarning("评论 {CommentId} 不存在", createReportDto.CommentId);
                throw new ArgumentException("评论不存在");
            }

            // 检查用户是否已举报过该评论
            var hasReported = await HasUserReportedCommentAsync(createReportDto.CommentId, userId);
            if (hasReported)
            {
                _logger.LogWarning("用户 {UserId} 已经举报过评论 {CommentId}", userId, createReportDto.CommentId);
                throw new InvalidOperationException("您已经举报过此评论");
            }

            // 验证举报原因
            if (!Enum.IsDefined(typeof(ReportReason), createReportDto.Reason))
            {
                throw new ArgumentException("无效的举报原因");
            }

            // 验证描述长度
            if (createReportDto.Description != null && createReportDto.Description.Length > 1000)
            {
                throw new ArgumentException("举报描述长度不能超过1000个字符");
            }

            // 使用输入验证服务验证举报内容
            await _inputValidationService.ValidateReportContentAsync(createReportDto.Description);

            // 创建举报记录
            var report = new CommentReport
            {
                Id = Guid.NewGuid(),
                CommentId = createReportDto.CommentId,
                UserId = userId,
                Reason = createReportDto.Reason,
                Description = createReportDto.Description,
                Status = ReportStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            // 保存举报记录
            await _commentReportRepository.CreateAsync(report);

            _logger.LogInformation("用户 {UserId} 成功举报评论 {CommentId}, 举报ID: {ReportId}", userId, createReportDto.CommentId, report.Id);

            return report.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "举报评论时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 获取评论举报详情
    /// </summary>
    public async Task<CommentReportDto?> GetReportAsync(Guid reportId)
    {
        try
        {
            _logger.LogDebug("获取评论举报详情: {ReportId}", reportId);

            var report = await _commentReportRepository.GetByIdAsync(reportId);
            if (report == null)
            {
                _logger.LogWarning("举报 {ReportId} 不存在", reportId);
                return null;
            }

            return await MapToCommentReportDtoAsync(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取评论举报详情时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 分页获取评论举报列表
    /// </summary>
    public async Task<PaginatedResult<CommentReportDto>> GetReportsAsync(CommentReportFilterDto filter)
    {
        try
        {
            _logger.LogDebug("获取评论举报列表: Status={Status}, Page={Page}, PageSize={PageSize}", 
                filter.Status, filter.Page, filter.PageSize);

            var result = await _commentReportRepository.GetPagedAsync(filter);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取评论举报列表时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 处理评论举报
    /// </summary>
    public async Task<bool> HandleReportAsync(Guid reportId, HandleCommentReportDto handleReportDto, Guid handlerId)
    {
        try
        {
            _logger.LogInformation("用户 {HandlerId} 尝试处理举报 {ReportId}", handlerId, reportId);

            // 检查举报是否存在
            var report = await _commentReportRepository.GetByIdAsync(reportId);
            if (report == null)
            {
                _logger.LogWarning("举报 {ReportId} 不存在", reportId);
                return false;
            }

            // 检查举报状态
            if (report.Status != ReportStatus.Pending)
            {
                _logger.LogWarning("举报 {ReportId} 已经被处理过，当前状态: {Status}", reportId, report.Status);
                return false;
            }

            // 验证处理状态
            if (!Enum.IsDefined(typeof(ReportStatus), handleReportDto.Status) || 
                handleReportDto.Status == ReportStatus.Pending)
            {
                throw new ArgumentException("无效的处理状态");
            }

            // 验证处理说明长度
            if (handleReportDto.Resolution != null && handleReportDto.Resolution.Length > 500)
            {
                throw new ArgumentException("处理说明长度不能超过500个字符");
            }

            // 更新举报记录
            report.Status = handleReportDto.Status;
            report.Resolution = handleReportDto.Resolution;
            report.HandledAt = DateTime.UtcNow;
            report.HandledBy = handlerId;

            var result = await _commentReportRepository.UpdateAsync(report);

            if (result)
            {
                // 根据处理结果执行相应操作
                if (handleReportDto.Status == ReportStatus.Resolved)
                {
                    // 举报被确认，可能需要隐藏评论
                    await SetCommentVisibilityAsync(report.CommentId, true, handlerId, handleReportDto.Resolution);
                }

                _logger.LogInformation("用户 {HandlerId} 成功处理举报 {ReportId}, 状态: {Status}", handlerId, reportId, handleReportDto.Status);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理评论举报时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 批量处理评论举报
    /// </summary>
    public async Task<bool> BulkHandleReportsAsync(IEnumerable<Guid> reportIds, ReportStatus status, string? resolution, Guid handlerId)
    {
        try
        {
            _logger.LogInformation("用户 {HandlerId} 尝试批量处理 {Count} 个举报", handlerId, reportIds.Count());

            if (!reportIds.Any())
            {
                _logger.LogWarning("举报ID列表为空");
                return false;
            }

            // 验证处理状态
            if (!Enum.IsDefined(typeof(ReportStatus), status) || status == ReportStatus.Pending)
            {
                throw new ArgumentException("无效的处理状态");
            }

            // 验证处理说明长度
            if (resolution != null && resolution.Length > 500)
            {
                throw new ArgumentException("处理说明长度不能超过500个字符");
            }

            // 批量处理举报
            var result = await _commentReportRepository.BulkHandleReportsAsync(reportIds, status, resolution, handlerId);

            if (result)
            {
                _logger.LogInformation("用户 {HandlerId} 成功批量处理 {Count} 个举报", handlerId, reportIds.Count());
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量处理评论举报时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 获取用户的举报历史
    /// </summary>
    public async Task<PaginatedResult<CommentReportDto>> GetUserReportsAsync(Guid userId, CommentReportFilterDto filter)
    {
        try
        {
            _logger.LogDebug("获取用户 {UserId} 的举报历史", userId);

            filter.UserId = userId;
            var result = await _commentReportRepository.GetPagedAsync(filter);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户举报历史时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 检查用户是否已举报评论
    /// </summary>
    public async Task<bool> HasUserReportedCommentAsync(Guid commentId, Guid userId)
    {
        try
        {
            return await _commentReportRepository.HasUserReportedCommentAsync(commentId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查用户是否已举报评论时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    #endregion

    #region 评论审核管理

    /// <summary>
    /// 审核评论（通过/拒绝）
    /// </summary>
    public async Task<bool> ModerateCommentAsync(Guid commentId, bool approved, string? reason, Guid moderatorId)
    {
        try
        {
            _logger.LogInformation("审核人 {ModeratorId} 尝试审核评论 {CommentId}, 结果: {Approved}", moderatorId, commentId, approved);

            // 检查评论是否存在
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null)
            {
                _logger.LogWarning("评论 {CommentId} 不存在", commentId);
                return false;
            }

            // 检查评论状态是否为待审核
            if (comment.Status != CommentStatus.Pending)
            {
                _logger.LogWarning("评论 {CommentId} 状态不是待审核，当前状态: {Status}", commentId, comment.Status);
                return false;
            }

            // 验证审核原因长度
            if (reason != null && reason.Length > 500)
            {
                throw new ArgumentException("审核原因长度不能超过500个字符");
            }

            // 执行审核
            var result = await _commentModerationService.ModerateCommentAsync(commentId, approved, reason, moderatorId);

            if (result)
            {
                // 清除相关缓存
                await ClearCommentCacheAsync(commentId);
                await ClearSnippetCommentCacheAsync(comment.SnippetId);

                _logger.LogInformation("审核人 {ModeratorId} 成功审核评论 {CommentId}, 结果: {Approved}", moderatorId, commentId, approved);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "审核评论时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 批量审核评论
    /// </summary>
    public async Task<bool> BulkModerateCommentsAsync(BatchCommentOperationDto batchOperationDto, Guid moderatorId)
    {
        try
        {
            _logger.LogInformation("审核人 {ModeratorId} 尝试批量审核 {Count} 个评论", moderatorId, batchOperationDto.CommentIds.Count);

            if (!batchOperationDto.CommentIds.Any())
            {
                _logger.LogWarning("评论ID列表为空");
                return false;
            }

            // 验证操作类型
            if (!Enum.IsDefined(typeof(CommentOperation), batchOperationDto.Operation))
            {
                throw new ArgumentException("无效的操作类型");
            }

            // 验证操作原因长度
            if (batchOperationDto.Reason != null && batchOperationDto.Reason.Length > 500)
            {
                throw new ArgumentException("操作原因长度不能超过500个字符");
            }

            // 执行批量审核
            var result = await _commentModerationService.BulkModerateCommentsAsync(batchOperationDto, moderatorId);

            if (result)
            {
                // 清除相关缓存
                foreach (var commentId in batchOperationDto.CommentIds)
                {
                    await ClearCommentCacheAsync(commentId);
                    var comment = await _commentRepository.GetByIdAsync(commentId);
                    if (comment != null)
                    {
                        await ClearSnippetCommentCacheAsync(comment.SnippetId);
                    }
                }

                _logger.LogInformation("审核人 {ModeratorId} 成功批量审核 {Count} 个评论", moderatorId, batchOperationDto.CommentIds.Count);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量审核评论时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 获取待审核评论列表
    /// </summary>
    public async Task<PaginatedResult<CommentDto>> GetPendingCommentsAsync(CommentFilterDto filter)
    {
        try
        {
            _logger.LogDebug("获取待审核评论列表: Page={Page}, PageSize={PageSize}", filter.Page, filter.PageSize);

            filter.Status = CommentStatus.Pending;
            var result = await _commentRepository.GetPagedAsync(filter);

            // 映射为DTO
            var commentDtos = new List<CommentDto>();
            foreach (var comment in result.Items)
            {
                commentDtos.Add(await MapToCommentDtoAsync(comment, null));
            }

            return new PaginatedResult<CommentDto>
            {
                Items = commentDtos,
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取待审核评论列表时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 获取被隐藏评论列表
    /// </summary>
    public async Task<PaginatedResult<CommentDto>> GetHiddenCommentsAsync(CommentFilterDto filter)
    {
        try
        {
            _logger.LogDebug("获取被隐藏评论列表: Page={Page}, PageSize={PageSize}", filter.Page, filter.PageSize);

            filter.Status = CommentStatus.Hidden;
            var result = await _commentRepository.GetPagedAsync(filter);

            // 映射为DTO
            var commentDtos = new List<CommentDto>();
            foreach (var comment in result.Items)
            {
                commentDtos.Add(await MapToCommentDtoAsync(comment, null));
            }

            return new PaginatedResult<CommentDto>
            {
                Items = commentDtos,
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取被隐藏评论列表时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 显示/隐藏评论
    /// </summary>
    public async Task<bool> SetCommentVisibilityAsync(Guid commentId, bool hide, Guid moderatorId, string? reason)
    {
        try
        {
            _logger.LogInformation("审核人 {ModeratorId} 尝试{Action}评论 {CommentId}", moderatorId, hide ? "隐藏" : "显示", commentId);

            // 检查评论是否存在
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null)
            {
                _logger.LogWarning("评论 {CommentId} 不存在", commentId);
                return false;
            }

            // 验证操作原因长度
            if (reason != null && reason.Length > 500)
            {
                throw new ArgumentException("操作原因长度不能超过500个字符");
            }

            // 执行显示/隐藏操作
            var result = await _commentModerationService.SetCommentVisibilityAsync(commentId, hide, moderatorId, reason);

            if (result)
            {
                // 清除相关缓存
                await ClearCommentCacheAsync(commentId);
                await ClearSnippetCommentCacheAsync(comment.SnippetId);

                _logger.LogInformation("审核人 {ModeratorId} 成功{Action}评论 {CommentId}", moderatorId, hide ? "隐藏" : "显示", commentId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置评论可见性时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    #endregion

    #region 评论统计分析

    /// <summary>
    /// 获取评论统计信息
    /// </summary>
    public async Task<CommentStatsDto> GetCommentStatsAsync(Guid snippetId)
    {
        try
        {
            _logger.LogDebug("获取代码片段 {SnippetId} 的评论统计信息", snippetId);

            // 尝试从缓存获取
            var cacheKey = $"{COMMENT_STATS_CACHE_PREFIX}{snippetId}";
            var cachedStats = await _cacheService.GetAsync<CommentStatsDto>(cacheKey);
            if (cachedStats != null)
            {
                return cachedStats;
            }

            // 从数据库获取统计信息
            var stats = await _commentAnalyticsService.GetCommentStatsAsync(snippetId);

            // 缓存结果
            await _cacheService.SetAsync(cacheKey, stats, TimeSpan.FromMinutes(10));

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取评论统计信息时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 获取用户评论统计信息
    /// </summary>
    public async Task<UserCommentStatsDto> GetUserCommentStatsAsync(Guid userId)
    {
        try
        {
            _logger.LogDebug("获取用户 {UserId} 的评论统计信息", userId);

            // 尝试从缓存获取
            var cacheKey = $"{USER_COMMENTS_CACHE_PREFIX}{userId}_stats";
            var cachedStats = await _cacheService.GetAsync<UserCommentStatsDto>(cacheKey);
            if (cachedStats != null)
            {
                return cachedStats;
            }

            // 从数据库获取统计信息
            var stats = await _commentAnalyticsService.GetUserCommentStatsAsync(userId);

            // 缓存结果
            await _cacheService.SetAsync(cacheKey, stats, TimeSpan.FromMinutes(30));

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户评论统计信息时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 获取全局评论统计信息
    /// </summary>
    public async Task<GlobalCommentStatsDto> GetGlobalCommentStatsAsync()
    {
        try
        {
            _logger.LogDebug("获取全局评论统计信息");

            // 尝试从缓存获取
            var cacheKey = $"{COMMENT_STATS_CACHE_PREFIX}global";
            var cachedStats = await _cacheService.GetAsync<GlobalCommentStatsDto>(cacheKey);
            if (cachedStats != null)
            {
                return cachedStats;
            }

            // 从数据库获取统计信息
            var stats = await _commentAnalyticsService.GetGlobalCommentStatsAsync();

            // 缓存结果
            await _cacheService.SetAsync(cacheKey, stats, TimeSpan.FromMinutes(5));

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取全局评论统计信息时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 获取评论趋势统计
    /// </summary>
    public async Task<IEnumerable<CommentTrendDto>> GetCommentTrendAsync(int days = 30)
    {
        try
        {
            _logger.LogDebug("获取评论趋势统计: {Days} 天", days);

            if (days <= 0 || days > 365)
            {
                throw new ArgumentException("统计天数必须在1-365天之间");
            }

            var trends = await _commentAnalyticsService.GetCommentTrendAsync(days);
            return trends;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取评论趋势统计时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 获取最热门评论
    /// </summary>
    public async Task<IEnumerable<CommentDto>> GetTopCommentsAsync(Guid snippetId, int count = 5)
    {
        try
        {
            _logger.LogDebug("获取代码片段 {SnippetId} 的最热门评论: {Count} 条", snippetId, count);

            if (count <= 0 || count > 20)
            {
                throw new ArgumentException("获取数量必须在1-20之间");
            }

            // 尝试从缓存获取
            var cacheKey = $"{SNIPPET_COMMENTS_CACHE_PREFIX}{snippetId}_top_{count}";
            var cachedTopComments = await _cacheService.GetAsync<IEnumerable<CommentDto>>(cacheKey);
            if (cachedTopComments != null)
            {
                return cachedTopComments;
            }

            // 获取最热门评论
            var topComments = await _commentRepository.GetMostLikedCommentsBySnippetIdAsync(snippetId, count);

            // 映射为DTO
            var commentDtos = new List<CommentDto>();
            foreach (var comment in topComments)
            {
                commentDtos.Add(await MapToCommentDtoAsync(comment, null));
            }

            // 缓存结果
            await _cacheService.SetAsync(cacheKey, commentDtos, TimeSpan.FromMinutes(10));

            return commentDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取最热门评论时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 获取评论活动报告
    /// </summary>
    public async Task<CommentActivityReportDto> GetCommentActivityReportAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            _logger.LogDebug("获取评论活动报告: {StartDate} - {EndDate}", startDate, endDate);

            if (startDate > endDate)
            {
                throw new ArgumentException("开始日期不能晚于结束日期");
            }

            if (endDate - startDate > TimeSpan.FromDays(365))
            {
                throw new ArgumentException("报告时间范围不能超过365天");
            }

            var report = await _commentAnalyticsService.GetCommentActivityReportAsync(startDate, endDate);
            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取评论活动报告时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    #endregion

    #region 搜索和筛选

    /// <summary>
    /// 搜索评论
    /// </summary>
    public async Task<PaginatedResult<CommentDto>> SearchCommentsAsync(string searchTerm, CommentFilterDto filter, Guid? currentUserId = null)
    {
        try
        {
            _logger.LogDebug("搜索评论: {SearchTerm}", searchTerm);

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                throw new ArgumentException("搜索词不能为空");
            }

            if (searchTerm.Length > 100)
            {
                throw new ArgumentException("搜索词长度不能超过100个字符");
            }

            // 使用输入验证服务验证搜索词
            await _inputValidationService.ValidateSearchTermAsync(searchTerm);

            filter.Search = searchTerm;
            var result = await _commentRepository.GetPagedAsync(filter);

            // 映射为DTO
            var commentDtos = new List<CommentDto>();
            foreach (var comment in result.Items)
            {
                commentDtos.Add(await MapToCommentDtoAsync(comment, currentUserId));
            }

            return new PaginatedResult<CommentDto>
            {
                Items = commentDtos,
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "搜索评论时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 获取最新评论
    /// </summary>
    public async Task<IEnumerable<CommentDto>> GetLatestCommentsAsync(Guid snippetId, int count = 5, Guid? currentUserId = null)
    {
        try
        {
            _logger.LogDebug("获取代码片段 {SnippetId} 的最新评论: {Count} 条", snippetId, count);

            if (count <= 0 || count > 20)
            {
                throw new ArgumentException("获取数量必须在1-20之间");
            }

            // 尝试从缓存获取
            var cacheKey = $"{SNIPPET_COMMENTS_CACHE_PREFIX}{snippetId}_latest_{count}";
            var cachedLatestComments = await _cacheService.GetAsync<IEnumerable<CommentDto>>(cacheKey);
            if (cachedLatestComments != null)
            {
                return cachedLatestComments;
            }

            // 获取最新评论
            var latestComments = await _commentRepository.GetLatestCommentsBySnippetIdAsync(snippetId, count);

            // 映射为DTO
            var commentDtos = new List<CommentDto>();
            foreach (var comment in latestComments)
            {
                commentDtos.Add(await MapToCommentDtoAsync(comment, currentUserId));
            }

            // 缓存结果
            await _cacheService.SetAsync(cacheKey, commentDtos, TimeSpan.FromMinutes(5));

            return commentDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取最新评论时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 获取最多点赞的评论
    /// </summary>
    public async Task<IEnumerable<CommentDto>> GetMostLikedCommentsAsync(Guid snippetId, int count = 5, Guid? currentUserId = null)
    {
        try
        {
            _logger.LogDebug("获取代码片段 {SnippetId} 的最多点赞评论: {Count} 条", snippetId, count);

            if (count <= 0 || count > 20)
            {
                throw new ArgumentException("获取数量必须在1-20之间");
            }

            // 尝试从缓存获取
            var cacheKey = $"{SNIPPET_COMMENTS_CACHE_PREFIX}{snippetId}_most_liked_{count}";
            var cachedMostLikedComments = await _cacheService.GetAsync<IEnumerable<CommentDto>>(cacheKey);
            if (cachedMostLikedComments != null)
            {
                return cachedMostLikedComments;
            }

            // 获取最多点赞评论
            var mostLikedComments = await _commentRepository.GetMostLikedCommentsBySnippetIdAsync(snippetId, count);

            // 映射为DTO
            var commentDtos = new List<CommentDto>();
            foreach (var comment in mostLikedComments)
            {
                commentDtos.Add(await MapToCommentDtoAsync(comment, currentUserId));
            }

            // 缓存结果
            await _cacheService.SetAsync(cacheKey, commentDtos, TimeSpan.FromMinutes(10));

            return commentDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取最多点赞评论时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    #endregion

    #region 缓存管理

    /// <summary>
    /// 清除评论缓存
    /// </summary>
    public async Task<bool> ClearCommentCacheAsync(Guid commentId)
    {
        try
        {
            _logger.LogDebug("清除评论缓存: {CommentId}", commentId);

            var cacheKey = $"{COMMENT_CACHE_PREFIX}{commentId}";
            var result = await _cacheService.RemoveAsync(cacheKey);
            
            if (result)
            {
                _logger.LogDebug("成功清除评论缓存: {CommentId}", commentId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清除评论缓存时发生错误: {Message}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// 清除代码片段评论缓存
    /// </summary>
    public async Task<bool> ClearSnippetCommentCacheAsync(Guid snippetId)
    {
        try
        {
            _logger.LogDebug("清除代码片段评论缓存: {SnippetId}", snippetId);

            var result = true;
            
            // 清除所有相关的缓存键
            var cacheKeys = new[]
            {
                $"{SNIPPET_COMMENTS_CACHE_PREFIX}{snippetId}_tree",
                $"{SNIPPET_COMMENTS_CACHE_PREFIX}{snippetId}_top_5",
                $"{SNIPPET_COMMENTS_CACHE_PREFIX}{snippetId}_top_10",
                $"{SNIPPET_COMMENTS_CACHE_PREFIX}{snippetId}_latest_5",
                $"{SNIPPET_COMMENTS_CACHE_PREFIX}{snippetId}_latest_10",
                $"{SNIPPET_COMMENTS_CACHE_PREFIX}{snippetId}_most_liked_5",
                $"{SNIPPET_COMMENTS_CACHE_PREFIX}{snippetId}_most_liked_10",
                $"{COMMENT_STATS_CACHE_PREFIX}{snippetId}"
            };

            foreach (var cacheKey in cacheKeys)
            {
                var cleared = await _cacheService.RemoveAsync(cacheKey);
                if (!cleared)
                {
                    result = false;
                    _logger.LogWarning("清除缓存失败: {CacheKey}", cacheKey);
                }
            }

            if (result)
            {
                _logger.LogDebug("成功清除代码片段评论缓存: {SnippetId}", snippetId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清除代码片段评论缓存时发生错误: {Message}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// 清除用户评论缓存
    /// </summary>
    public async Task<bool> ClearUserCommentCacheAsync(Guid userId)
    {
        try
        {
            _logger.LogDebug("清除用户评论缓存: {UserId}", userId);

            var result = true;
            
            // 清除所有相关的缓存键
            var cacheKeys = new[]
            {
                $"{USER_COMMENTS_CACHE_PREFIX}{userId}_stats",
                $"{USER_COMMENTS_CACHE_PREFIX}{userId}_reports"
            };

            foreach (var cacheKey in cacheKeys)
            {
                var cleared = await _cacheService.RemoveAsync(cacheKey);
                if (!cleared)
                {
                    result = false;
                    _logger.LogWarning("清除缓存失败: {CacheKey}", cacheKey);
                }
            }

            if (result)
            {
                _logger.LogDebug("成功清除用户评论缓存: {UserId}", userId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清除用户评论缓存时发生错误: {Message}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// 预热评论缓存
    /// </summary>
    public async Task<bool> WarmupCommentCacheAsync(Guid snippetId)
    {
        try
        {
            _logger.LogDebug("预热评论缓存: {SnippetId}", snippetId);

            try
            {
                // 预热评论树
                await GetCommentTreeAsync(snippetId, null);
                
                // 预热评论统计
                await GetCommentStatsAsync(snippetId);
                
                // 预热热门评论
                await GetTopCommentsAsync(snippetId, 5);
                
                // 预热最新评论
                await GetLatestCommentsAsync(snippetId, 5);
                
                // 预热最多点赞评论
                await GetMostLikedCommentsAsync(snippetId, 5);

                _logger.LogDebug("成功预热评论缓存: {SnippetId}", snippetId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "预热评论缓存时发生错误: {Message}", ex.Message);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "预热评论缓存时发生错误: {Message}", ex.Message);
            return false;
        }
    }

    #endregion

    #region 私有辅助方法

    /// <summary>
    /// 验证评论输入数据
    /// </summary>
    private async Task ValidateCommentInputAsync(CreateCommentDto createCommentDto)
    {
        if (createCommentDto == null)
        {
            throw new ArgumentNullException(nameof(createCommentDto));
        }

        if (string.IsNullOrWhiteSpace(createCommentDto.Content))
        {
            throw new ArgumentException("评论内容不能为空");
        }

        if (createCommentDto.Content.Length > 2000)
        {
            throw new ArgumentException("评论内容长度不能超过2000个字符");
        }

        if (createCommentDto.SnippetId == Guid.Empty)
        {
            throw new ArgumentException("代码片段ID不能为空");
        }

        // 使用输入验证服务验证内容
        await _inputValidationService.ValidateCommentContentAsync(createCommentDto.Content);

        // 使用评论验证服务验证内容
        await _commentValidationService.ValidateCommentContentAsync(createCommentDto.Content);
    }

    /// <summary>
    /// 获取评论深度
    /// </summary>
    private async Task<int> GetCommentDepthAsync(Guid commentId)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        return comment?.Depth ?? 0;
    }

    /// <summary>
    /// 获取父路径
    /// </summary>
    private async Task<string> GetParentPathAsync(Guid parentId)
    {
        var parentComment = await _commentRepository.GetByIdAsync(parentId);
        if (parentComment == null)
        {
            return string.Empty;
        }

        return string.IsNullOrEmpty(parentComment.ParentPath) 
            ? parentId.ToString() 
            : $"{parentComment.ParentPath}/{parentId}";
    }

    /// <summary>
    /// 递归获取评论回复
    /// </summary>
    private async Task<List<CommentDto>> GetCommentRepliesRecursiveAsync(Guid parentId, Guid? currentUserId = null)
    {
        var replies = await _commentRepository.GetRepliesByParentIdAsync(parentId);
        var replyDtos = new List<CommentDto>();

        foreach (var reply in replies)
        {
            var replyDto = await MapToCommentDtoAsync(reply, currentUserId);
            replyDto.Replies = await GetCommentRepliesRecursiveAsync(reply.Id, currentUserId);
            replyDtos.Add(replyDto);
        }

        return replyDtos;
    }

    /// <summary>
    /// 映射评论实体到DTO
    /// </summary>
    private async Task<CommentDto> MapToCommentDtoAsync(Comment comment, Guid? currentUserId = null)
    {
        var commentDto = new CommentDto
        {
            Id = comment.Id,
            Content = comment.Content,
            SnippetId = comment.SnippetId,
            UserId = comment.UserId,
            UserName = comment.User?.UserName ?? "未知用户",
            UserAvatar = comment.User?.AvatarUrl,
            ParentId = comment.ParentId,
            ParentPath = comment.ParentPath,
            Depth = comment.Depth,
            LikeCount = comment.LikeCount,
            ReplyCount = comment.ReplyCount,
            Status = comment.Status,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };

        // 设置当前用户相关状态
        if (currentUserId.HasValue)
        {
            commentDto.IsLikedByCurrentUser = await IsCommentLikedByUserAsync(comment.Id, currentUserId.Value);
            commentDto.CanEdit = await _commentRepository.CanUserEditCommentAsync(comment.Id, currentUserId.Value);
            commentDto.CanDelete = await _commentRepository.CanUserDeleteCommentAsync(comment.Id, currentUserId.Value);
            commentDto.CanReport = !await HasUserReportedCommentAsync(comment.Id, currentUserId.Value);
        }
        else
        {
            commentDto.IsLikedByCurrentUser = false;
            commentDto.CanEdit = false;
            commentDto.CanDelete = false;
            commentDto.CanReport = false;
        }

        return commentDto;
    }

    /// <summary>
    /// 映射评论举报实体到DTO
    /// </summary>
    private async Task<CommentReportDto> MapToCommentReportDtoAsync(CommentReport report)
    {
        return new CommentReportDto
        {
            Id = report.Id,
            CommentId = report.CommentId,
            UserId = report.UserId,
            UserName = report.User?.UserName ?? "未知用户",
            Reason = report.Reason,
            Description = report.Description,
            Status = report.Status,
            CreatedAt = report.CreatedAt,
            HandledAt = report.HandledAt,
            HandlerName = report.Handler?.UserName,
            Resolution = report.Resolution
        };
    }

    #endregion
}