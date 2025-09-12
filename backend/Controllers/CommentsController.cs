using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.DTOs;
using CodeSnippetManager.Api.Models;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using System.Collections.Concurrent;

namespace CodeSnippetManager.Api.Controllers;

/// <summary>
/// 评论控制器 - 提供评论功能的RESTful API接口
/// </summary>
/// <remarks>
/// 此控制器提供完整的评论功能，包括：
/// - 评论的创建、读取、更新、删除（CRUD）
/// - 评论点赞功能
/// - 评论举报功能
/// - 评论审核管理
/// - 评论统计和分析
/// - 批量操作支持
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
[ResponseCache(Duration = 30, Location = ResponseCacheLocation.Client, NoStore = false)]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;
    private readonly ICommentValidationService _commentValidationService;
    private readonly ICommentModerationService _commentModerationService;
    private readonly ICommentReportManagementService _commentReportManagementService;
    private readonly ICommentAnalyticsService _commentAnalyticsService;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<CommentsController> _logger;

    // 简单缓存，用于缓存热门评论
    private static readonly ConcurrentDictionary<string, CacheEntry> _commentCache = new();

    public CommentsController(
        ICommentService commentService,
        ICommentValidationService commentValidationService,
        ICommentModerationService commentModerationService,
        ICommentReportManagementService commentReportManagementService,
        ICommentAnalyticsService commentAnalyticsService,
        IPermissionService permissionService,
        ILogger<CommentsController> logger)
    {
        _commentService = commentService ?? throw new ArgumentNullException(nameof(commentService));
        _commentValidationService = commentValidationService ?? throw new ArgumentNullException(nameof(commentValidationService));
        _commentModerationService = commentModerationService ?? throw new ArgumentNullException(nameof(commentModerationService));
        _commentReportManagementService = commentReportManagementService ?? throw new ArgumentNullException(nameof(commentReportManagementService));
        _commentAnalyticsService = commentAnalyticsService ?? throw new ArgumentNullException(nameof(commentAnalyticsService));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region 评论CRUD操作

    /// <summary>
    /// 获取评论列表
    /// </summary>
    /// <param name="filter">评论筛选条件</param>
    /// <returns>评论列表</returns>
    /// <remarks>
    /// 根据筛选条件获取评论列表，支持分页、排序和多种筛选条件。
    /// 
    /// 示例请求：
    /// GET /api/comments?snippetId=12345678-1234-1234-1234-123456789012&amp;page=1&amp;pageSize=20&amp;sortBy=CreatedAtDesc
    /// </remarks>
    /// <response code="200">成功获取评论列表</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<CommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResult<CommentDto>>> GetComments([FromQuery] CommentFilter filter)
    {
        try
        {
            // 输入验证
            if (filter == null)
            {
                return BadRequest(new { message = "筛选参数不能为空" });
            }

            if (filter.Page < 1)
            {
                return BadRequest(new { message = "页码必须大于0" });
            }

            if (filter.PageSize < 1 || filter.PageSize > 100)
            {
                return BadRequest(new { message = "每页大小必须在1-100之间" });
            }

            // 设置当前用户ID
            filter.CurrentUserId = GetCurrentUserId();

            // 生成缓存键
            var cacheKey = GenerateCacheKey(filter);

            // 尝试从缓存获取
            if (ShouldUseCache(filter))
            {
                if (_commentCache.TryGetValue(cacheKey, out var cachedEntry))
                {
                    if (cachedEntry.ExpiresAt > DateTime.UtcNow)
                    {
                        _logger.LogDebug("从缓存获取评论列表成功，缓存键: {CacheKey}", cacheKey);
                        return Ok(cachedEntry.Data);
                    }
                    else
                    {
                        _commentCache.TryRemove(cacheKey, out _);
                    }
                }
            }

            // 从服务获取数据
            var result = await _commentService.GetCommentsPaginatedAsync(filter);

            // 缓存结果
            if (ShouldUseCache(filter))
            {
                var cacheEntry = new CacheEntry
                {
                    Data = result,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5)
                };
                _commentCache.TryAdd(cacheKey, cacheEntry);
            }

            _logger.LogInformation("获取评论列表成功，筛选条件: {Filter}, 页码: {Page}, 每页大小: {PageSize}, 总数: {TotalCount}", 
                filter, filter.Page, filter.PageSize, result.TotalCount);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限获取评论列表: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("获取评论列表参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取评论列表时发生错误");
            return StatusCode(500, new { message = "获取评论列表时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取指定评论的详细信息
    /// </summary>
    /// <param name="id">评论ID</param>
    /// <returns>评论详细信息</returns>
    /// <remarks>
    /// 获取指定评论的详细信息，包括回复和点赞信息。
    /// 
    /// 示例请求：
    /// GET /api/comments/12345678-1234-1234-1234-123456789012
    /// </remarks>
    /// <response code="200">成功获取评论信息</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="404">评论不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client, NoStore = false)]
    public async Task<ActionResult<CommentDto>> GetComment(Guid id)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "评论ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            var comment = await _commentService.GetCommentByIdAsync(id, currentUserId);

            if (comment == null)
            {
                return NotFound(new { message = "评论不存在" });
            }

            _logger.LogInformation("获取评论信息成功，评论ID: {CommentId}", id);
            return Ok(comment);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("获取评论信息参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取评论信息时发生错误，评论ID: {CommentId}", id);
            return StatusCode(500, new { message = "获取评论信息时发生内部错误" });
        }
    }

    /// <summary>
    /// 创建新评论
    /// </summary>
    /// <param name="createCommentDto">创建评论的请求参数</param>
    /// <returns>创建的评论信息</returns>
    /// <remarks>
    /// 为指定的代码片段创建新评论，支持回复功能。
    /// 
    /// 示例请求：
    /// POST /api/comments
    /// {
    ///   "content": "这是一个很好的代码片段，谢谢分享！",
    ///   "snippetId": "12345678-1234-1234-1234-123456789012",
    ///   "parentId": null
    /// }
    /// </remarks>
    /// <response code="201">评论创建成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">代码片段不存在</response>
    /// <response code="429">评论频率限制</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CommentDto>> CreateComment([FromBody] CreateCommentDto createCommentDto)
    {
        try
        {
            // 输入验证
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            // 验证用户权限
            if (!await _permissionService.CanCreateCommentAsync(currentUserId.Value))
            {
                return Forbid();
            }

            // 频率限制检查
            if (!await _commentValidationService.CanCreateCommentAsync(currentUserId.Value))
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, 
                    new { message = "评论频率过高，请稍后再试" });
            }

            // 内容验证
            var validationResult = await _commentValidationService.ValidateCommentContentAsync(
                createCommentDto.Content, currentUserId.Value);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { message = validationResult.ErrorMessage });
            }

            var comment = await _commentService.CreateCommentAsync(createCommentDto, currentUserId.Value);

            // 清除相关缓存
            ClearCommentCache(createCommentDto.SnippetId);

            _logger.LogInformation("用户 {UserId} 成功创建评论，评论ID: {CommentId}, 代码片段ID: {SnippetId}", 
                currentUserId.Value, comment.Id, createCommentDto.SnippetId);

            // 返回201 Created状态码和评论信息
            return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, comment);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限创建评论: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("创建评论参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建评论时发生错误");
            return StatusCode(500, new { message = "创建评论时发生内部错误" });
        }
    }

    /// <summary>
    /// 更新评论内容
    /// </summary>
    /// <param name="id">评论ID</param>
    /// <param name="updateCommentDto">更新评论的请求参数</param>
    /// <returns>更新后的评论信息</returns>
    /// <remarks>
    /// 更新指定评论的内容，只有评论作者可以更新自己的评论。
    /// 
    /// 示例请求：
    /// PUT /api/comments/12345678-1234-1234-1234-123456789012
    /// {
    ///   "content": "更新后的评论内容"
    /// }
    /// </remarks>
    /// <response code="200">评论更新成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">评论不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CommentDto>> UpdateComment(Guid id, [FromBody] UpdateCommentDto updateCommentDto)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "评论ID不能为空" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            // 内容验证
            var validationResult = await _commentValidationService.ValidateCommentContentAsync(
                updateCommentDto.Content, currentUserId.Value);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { message = validationResult.ErrorMessage });
            }

            var comment = await _commentService.UpdateCommentAsync(id, updateCommentDto, currentUserId.Value);

            // 清除相关缓存
            ClearCommentCache(comment.SnippetId);

            _logger.LogInformation("用户 {UserId} 成功更新评论，评论ID: {CommentId}", 
                currentUserId.Value, id);

            return Ok(comment);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限更新评论: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("更新评论参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新评论时发生错误，评论ID: {CommentId}", id);
            return StatusCode(500, new { message = "更新评论时发生内部错误" });
        }
    }

    /// <summary>
    /// 删除评论
    /// </summary>
    /// <param name="id">评论ID</param>
    /// <returns>删除结果</returns>
    /// <remarks>
    /// 删除指定评论，只有评论作者或管理员可以删除评论。
    /// 此操作会将评论标记为已删除状态，而不是物理删除。
    /// 
    /// 示例请求：
    /// DELETE /api/comments/12345678-1234-1234-1234-123456789012
    /// </remarks>
    /// <response code="204">评论删除成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">评论不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteComment(Guid id)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "评论ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var comment = await _commentService.GetCommentByIdAsync(id, currentUserId.Value);
            if (comment == null)
            {
                return NotFound(new { message = "评论不存在" });
            }

            var success = await _commentService.DeleteCommentAsync(id, currentUserId.Value);

            if (!success)
            {
                return NotFound(new { message = "评论不存在" });
            }

            // 清除相关缓存
            ClearCommentCache(comment.SnippetId);

            _logger.LogInformation("用户 {UserId} 成功删除评论，评论ID: {CommentId}", 
                currentUserId.Value, id);

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限删除评论: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("删除评论参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除评论时发生错误，评论ID: {CommentId}", id);
            return StatusCode(500, new { message = "删除评论时发生内部错误" });
        }
    }

    #endregion

    #region 评论点赞功能

    /// <summary>
    /// 点赞评论
    /// </summary>
    /// <param name="id">评论ID</param>
    /// <returns>点赞结果</returns>
    /// <remarks>
    /// 为指定评论点赞，每个用户对同一评论只能点赞一次。
    /// 重复调用会取消点赞。
    /// 
    /// 示例请求：
    /// POST /api/comments/12345678-1234-1234-1234-123456789012/like
    /// </remarks>
    /// <response code="200">点赞操作成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="404">评论不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPost("{id:guid}/like")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> LikeComment(Guid id)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "评论ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var result = await _commentService.LikeCommentAsync(id, currentUserId.Value);
            var comment = await _commentService.GetCommentByIdAsync(id, currentUserId.Value);

            // 清除相关缓存
            if (comment != null)
            {
                ClearCommentCache(comment.SnippetId);
            }

            _logger.LogInformation("用户 {UserId} {Action} 评论，评论ID: {CommentId}", 
                currentUserId.Value, result.IsLiked ? "点赞" : "取消点赞", id);

            return Ok(new { 
                isLiked = result.IsLiked, 
                likeCount = result.LikeCount,
                message = result.IsLiked ? "点赞成功" : "取消点赞成功" 
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("评论点赞参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "评论点赞时发生错误，评论ID: {CommentId}", id);
            return StatusCode(500, new { message = "评论点赞时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取评论点赞列表
    /// </summary>
    /// <param name="id">评论ID</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <returns>评论点赞列表</returns>
    /// <remarks>
    /// 获取指定评论的点赞用户列表。
    /// 
    /// 示例请求：
    /// GET /api/comments/12345678-1234-1234-1234-123456789012/likes?page=1&amp;pageSize=20
    /// </remarks>
    /// <response code="200">成功获取点赞列表</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="404">评论不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("{id:guid}/likes")]
    [ProducesResponseType(typeof(PaginatedResult<CommentLikeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Client, NoStore = false)]
    public async Task<ActionResult<PaginatedResult<CommentLikeDto>>> GetCommentLikes(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "评论ID不能为空" });
            }

            if (page < 1)
            {
                return BadRequest(new { message = "页码必须大于0" });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { message = "每页大小必须在1-100之间" });
            }

            var filter = new CommentLikeFilter
            {
                CommentId = id,
                Page = page,
                PageSize = pageSize,
                IncludeUser = true
            };

            var result = await _commentService.GetCommentLikesPaginatedAsync(filter);

            _logger.LogInformation("获取评论点赞列表成功，评论ID: {CommentId}, 页码: {Page}, 每页大小: {PageSize}", 
                id, page, pageSize);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("获取评论点赞列表参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取评论点赞列表时发生错误，评论ID: {CommentId}", id);
            return StatusCode(500, new { message = "获取评论点赞列表时发生内部错误" });
        }
    }

    /// <summary>
    /// 检查用户是否已点赞指定评论
    /// </summary>
    /// <param name="id">评论ID</param>
    /// <returns>点赞状态</returns>
    /// <remarks>
    /// 检查当前用户是否已点赞指定评论。
    /// 
    /// 示例请求：
    /// GET /api/comments/12345678-1234-1234-1234-123456789012/like-status
    /// </remarks>
    /// <response code="200">成功获取点赞状态</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="404">评论不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("{id:guid}/like-status")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetCommentLikeStatus(Guid id)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "评论ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var isLiked = await _commentService.IsCommentLikedByUserAsync(id, currentUserId.Value);

            _logger.LogDebug("获取评论点赞状态成功，评论ID: {CommentId}, 用户ID: {UserId}, 点赞状态: {IsLiked}", 
                id, currentUserId.Value, isLiked);

            return Ok(new { isLiked });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("获取评论点赞状态参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取评论点赞状态时发生错误，评论ID: {CommentId}", id);
            return StatusCode(500, new { message = "获取评论点赞状态时发生内部错误" });
        }
    }

    #endregion

    #region 评论举报功能

    /// <summary>
    /// 举报评论
    /// </summary>
    /// <param name="id">评论ID</param>
    /// <param name="reportDto">举报信息</param>
    /// <returns>举报结果</returns>
    /// <remarks>
    /// 举报指定评论的不当内容，需要提供举报原因和详细描述。
    /// 
    /// 示例请求：
    /// POST /api/comments/12345678-1234-1234-1234-123456789012/report
    /// {
    ///   "reason": 1,
    ///   "description": "该评论包含不当内容"
    /// }
    /// </remarks>
    /// <response code="201">举报创建成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">评论不存在</response>
    /// <response code="409">已存在相同举报</response>
    /// <response code="429">举报频率限制</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPost("{id:guid}/report")]
    [Authorize]
    [ProducesResponseType(typeof(CommentReportDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(object), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CommentReportDto>> ReportComment(Guid id, [FromBody] CreateCommentReportDto reportDto)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "评论ID不能为空" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            // 验证用户权限
            if (!await _permissionService.CanReportCommentAsync(currentUserId.Value))
            {
                return Forbid();
            }

            // 检查是否已存在相同举报
            var existingReport = await _commentReportManagementService.GetExistingReportAsync(id, currentUserId.Value);
            if (existingReport != null)
            {
                return Conflict(new { message = "您已经举报过此评论" });
            }

            reportDto.CommentId = id;
            var report = await _commentReportManagementService.CreateReportAsync(reportDto, currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 成功举报评论，评论ID: {CommentId}, 举报ID: {ReportId}, 原因: {Reason}", 
                currentUserId.Value, id, report.Id, report.Reason);

            return CreatedAtAction(nameof(GetCommentReport), new { commentId = id, reportId = report.Id }, report);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限举报评论: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("举报评论参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "举报评论时发生错误，评论ID: {CommentId}", id);
            return StatusCode(500, new { message = "举报评论时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取评论举报详情
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <param name="reportId">举报ID</param>
    /// <returns>举报详情</returns>
    /// <remarks>
    /// 获取指定评论举报的详细信息。
    /// </remarks>
    /// <response code="200">成功获取举报详情</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">举报不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("{commentId:guid}/reports/{reportId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(CommentReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CommentReportDto>> GetCommentReport(Guid commentId, Guid reportId)
    {
        try
        {
            // 输入验证
            if (commentId == Guid.Empty || reportId == Guid.Empty)
            {
                return BadRequest(new { message = "评论ID和举报ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var report = await _commentReportManagementService.GetReportByIdAsync(reportId, currentUserId.Value);

            if (report == null)
            {
                return NotFound(new { message = "举报不存在" });
            }

            _logger.LogInformation("获取评论举报详情成功，评论ID: {CommentId}, 举报ID: {ReportId}", 
                commentId, reportId);

            return Ok(report);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限获取举报详情: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取评论举报详情时发生错误，评论ID: {CommentId}, 举报ID: {ReportId}", 
                commentId, reportId);
            return StatusCode(500, new { message = "获取举报详情时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取用户的评论举报列表
    /// </summary>
    /// <param name="filter">举报筛选条件</param>
    /// <returns>举报列表</returns>
    /// <remarks>
    /// 获取当前用户的评论举报列表。
    /// 
    /// 示例请求：
    /// GET /api/comments/my-reports?page=1&amp;pageSize=20&amp;status=0
    /// </remarks>
    /// <response code="200">成功获取举报列表</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("my-reports")]
    [Authorize]
    [ProducesResponseType(typeof(PaginatedResult<CommentReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResult<CommentReportDto>>> GetMyCommentReports([FromQuery] CommentReportFilterDto filter)
    {
        try
        {
            // 输入验证
            if (filter == null)
            {
                return BadRequest(new { message = "筛选参数不能为空" });
            }

            if (filter.Page < 1)
            {
                return BadRequest(new { message = "页码必须大于0" });
            }

            if (filter.PageSize < 1 || filter.PageSize > 100)
            {
                return BadRequest(new { message = "每页大小必须在1-100之间" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            filter.UserId = currentUserId.Value;
            var result = await _commentReportManagementService.GetReportsPaginatedAsync(filter);

            _logger.LogInformation("获取用户评论举报列表成功，用户ID: {UserId}, 页码: {Page}, 每页大小: {PageSize}, 总数: {TotalCount}", 
                currentUserId.Value, filter.Page, filter.PageSize, result.TotalCount);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户评论举报列表时发生错误");
            return StatusCode(500, new { message = "获取举报列表时发生内部错误" });
        }
    }

    #endregion

    #region 评论审核功能

    /// <summary>
    /// 获取评论统计信息
    /// </summary>
    /// <param name="snippetId">代码片段ID</param>
    /// <returns>评论统计信息</returns>
    /// <remarks>
    /// 获取指定代码片段的评论统计信息，包括评论总数、点赞数等。
    /// 
    /// 示例请求：
    /// GET /api/comments/stats?snippetId=12345678-1234-1234-1234-123456789012
    /// </remarks>
    /// <response code="200">成功获取统计信息</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="404">代码片段不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(CommentStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client, NoStore = false)]
    public async Task<ActionResult<CommentStatsDto>> GetCommentStats([FromQuery] Guid snippetId)
    {
        try
        {
            // 输入验证
            if (snippetId == Guid.Empty)
            {
                return BadRequest(new { message = "代码片段ID不能为空" });
            }

            var stats = await _commentAnalyticsService.GetCommentStatsAsync(snippetId);

            if (stats == null)
            {
                return NotFound(new { message = "代码片段不存在" });
            }

            _logger.LogInformation("获取评论统计信息成功，代码片段ID: {SnippetId}", snippetId);
            return Ok(stats);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("获取评论统计信息参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取评论统计信息时发生错误，代码片段ID: {SnippetId}", snippetId);
            return StatusCode(500, new { message = "获取评论统计信息时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取评论审核列表（管理员）
    /// </summary>
    /// <param name="filter">审核筛选条件</param>
    /// <returns>待审核评论列表</returns>
    /// <remarks>
    /// 获取待审核的评论列表，仅管理员可访问。
    /// 
    /// 示例请求：
    /// GET /api/comments/moderation/pending?page=1&amp;pageSize=20
    /// </remarks>
    /// <response code="200">成功获取待审核评论列表</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("moderation/pending")]
    [Authorize(Roles = "Admin,Moderator")]
    [ProducesResponseType(typeof(PaginatedResult<CommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResult<CommentDto>>> GetPendingComments([FromQuery] CommentFilter filter)
    {
        try
        {
            // 输入验证
            if (filter == null)
            {
                return BadRequest(new { message = "筛选参数不能为空" });
            }

            if (filter.Page < 1)
            {
                return BadRequest(new { message = "页码必须大于0" });
            }

            if (filter.PageSize < 1 || filter.PageSize > 100)
            {
                return BadRequest(new { message = "每页大小必须在1-100之间" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            filter.Status = CommentStatus.Pending;
            filter.CurrentUserId = currentUserId.Value;

            var result = await _commentModerationService.GetPendingCommentsAsync(filter);

            _logger.LogInformation("获取待审核评论列表成功，页码: {Page}, 每页大小: {PageSize}, 总数: {TotalCount}", 
                filter.Page, filter.PageSize, result.TotalCount);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限获取待审核评论: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取待审核评论列表时发生错误");
            return StatusCode(500, new { message = "获取待审核评论列表时发生内部错误" });
        }
    }

    /// <summary>
    /// 审核评论（管理员）
    /// </summary>
    /// <param name="id">评论ID</param>
    /// <param name="moderationDto">审核结果</param>
    /// <returns>审核结果</returns>
    /// <remarks>
    /// 审核指定评论，可以批准或拒绝评论。
    /// 
    /// 示例请求：
    /// POST /api/comments/12345678-1234-1234-1234-123456789012/moderate
    /// {
    ///   "status": 0,
    ///   "reason": "评论内容合规"
    /// }
    /// </remarks>
    /// <response code="200">评论审核成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">评论不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPost("{id:guid}/moderate")]
    [Authorize(Roles = "Admin,Moderator")]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CommentDto>> ModerateComment(Guid id, [FromBody] ModerateCommentDto moderationDto)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "评论ID不能为空" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var comment = await _commentModerationService.ModerateCommentAsync(id, moderationDto, currentUserId.Value);

            // 清除相关缓存
            ClearCommentCache(comment.SnippetId);

            _logger.LogInformation("用户 {UserId} 成功审核评论，评论ID: {CommentId}, 审核结果: {Status}", 
                currentUserId.Value, id, moderationDto.Status);

            return Ok(comment);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限审核评论: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("审核评论参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "审核评论时发生错误，评论ID: {CommentId}", id);
            return StatusCode(500, new { message = "审核评论时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取评论举报管理列表（管理员）
    /// </summary>
    /// <param name="filter">举报筛选条件</param>
    /// <returns>举报列表</returns>
    /// <remarks>
    /// 获取评论举报管理列表，仅管理员可访问。
    /// 
    /// 示例请求：
    /// GET /api/comments/reports?page=1&amp;pageSize=20&amp;status=0
    /// </remarks>
    /// <response code="200">成功获取举报列表</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("reports")]
    [Authorize(Roles = "Admin,Moderator")]
    [ProducesResponseType(typeof(PaginatedResult<CommentReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResult<CommentReportDto>>> GetCommentReports([FromQuery] CommentReportFilter filter)
    {
        try
        {
            // 输入验证
            if (filter == null)
            {
                return BadRequest(new { message = "筛选参数不能为空" });
            }

            if (filter.Page < 1)
            {
                return BadRequest(new { message = "页码必须大于0" });
            }

            if (filter.PageSize < 1 || filter.PageSize > 100)
            {
                return BadRequest(new { message = "每页大小必须在1-100之间" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var result = await _commentReportManagementService.GetReportsPaginatedAsync(filter);

            _logger.LogInformation("获取评论举报管理列表成功，页码: {Page}, 每页大小: {PageSize}, 总数: {TotalCount}", 
                filter.Page, filter.PageSize, result.TotalCount);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限获取举报管理列表: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取评论举报管理列表时发生错误");
            return StatusCode(500, new { message = "获取举报管理列表时发生内部错误" });
        }
    }

    /// <summary>
    /// 处理评论举报（管理员）
    /// </summary>
    /// <param name="reportId">举报ID</param>
    /// <param name="handleDto">处理结果</param>
    /// <returns>处理结果</returns>
    /// <remarks>
    /// 处理指定的评论举报，可以批准或拒绝举报。
    /// 
    /// 示例请求：
    /// POST /api/comments/reports/12345678-1234-1234-1234-123456789012/handle
    /// {
    ///   "status": 1,
    ///   "resolution": "举报已处理，评论已被删除"
    /// }
    /// </remarks>
    /// <response code="200">举报处理成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">举报不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPost("reports/{reportId:guid}/handle")]
    [Authorize(Roles = "Admin,Moderator")]
    [ProducesResponseType(typeof(CommentReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CommentReportDto>> HandleCommentReport(Guid reportId, [FromBody] HandleCommentReportDto handleDto)
    {
        try
        {
            // 输入验证
            if (reportId == Guid.Empty)
            {
                return BadRequest(new { message = "举报ID不能为空" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var report = await _commentReportManagementService.HandleReportAsync(reportId, handleDto, currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 成功处理评论举报，举报ID: {ReportId}, 处理结果: {Status}", 
                currentUserId.Value, reportId, handleDto.Status);

            return Ok(report);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限处理举报: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("处理评论举报参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理评论举报时发生错误，举报ID: {ReportId}", reportId);
            return StatusCode(500, new { message = "处理评论举报时发生内部错误" });
        }
    }

    /// <summary>
    /// 批量操作评论（管理员）
    /// </summary>
    /// <param name="batchDto">批量操作请求</param>
    /// <returns>操作结果</returns>
    /// <remarks>
    /// 批量操作评论，支持删除、隐藏、显示、批准等操作。
    /// 
    /// 示例请求：
    /// POST /api/comments/batch
    /// {
    ///   "commentIds": ["12345678-1234-1234-1234-123456789012"],
    ///   "operation": 0,
    ///   "reason": "批量删除违规评论"
    /// }
    /// </remarks>
    /// <response code="200">批量操作成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPost("batch")]
    [Authorize(Roles = "Admin,Moderator")]
    [ProducesResponseType(typeof(BatchOperationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BatchOperationResultDto>> BatchOperationComments([FromBody] BatchCommentOperationDto batchDto)
    {
        try
        {
            // 输入验证
            if (batchDto == null || batchDto.CommentIds == null || !batchDto.CommentIds.Any())
            {
                return BadRequest(new { message = "评论ID列表不能为空" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            batchDto.OperatorId = currentUserId.Value;
            var result = await _commentModerationService.BatchOperationCommentsAsync(batchDto);

            // 清除相关缓存
            foreach (var commentId in batchDto.CommentIds)
            {
                var comment = await _commentService.GetCommentByIdAsync(commentId, currentUserId.Value);
                if (comment != null)
                {
                    ClearCommentCache(comment.SnippetId);
                }
            }

            _logger.LogInformation("用户 {UserId} 成功批量操作评论，操作类型: {Operation}, 处理数量: {SuccessCount}/{TotalCount}", 
                currentUserId.Value, batchDto.Operation, result.SuccessCount, result.TotalCount);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限批量操作评论: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量操作评论时发生错误");
            return StatusCode(500, new { message = "批量操作评论时发生内部错误" });
        }
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 获取当前登录用户的ID
    /// </summary>
    /// <returns>用户ID，如果未登录则返回null</returns>
    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    /// <summary>
    /// 生成缓存键
    /// </summary>
    /// <param name="filter">筛选条件</param>
    /// <returns>缓存键</returns>
    private string GenerateCacheKey(CommentFilter filter)
    {
        var key = $"comments_{filter.SnippetId}_{filter.Page}_{filter.PageSize}_{filter.SortBy}";
        if (filter.UserId.HasValue)
            key += $"_{filter.UserId}";
        if (filter.Status.HasValue)
            key += $"_{filter.Status}";
        if (!string.IsNullOrEmpty(filter.Search))
            key += $"_{filter.Search}";
        
        return key;
    }

    /// <summary>
    /// 判断是否应该使用缓存
    /// </summary>
    /// <param name="filter">筛选条件</param>
    /// <returns>是否使用缓存</returns>
    private bool ShouldUseCache(CommentFilter filter)
    {
        // 只对简单的查询使用缓存
        return filter != null && 
               filter.SnippetId.HasValue && 
               !filter.UserId.HasValue &&
               filter.Status == null &&
               string.IsNullOrEmpty(filter.Search) &&
               filter.Page <= 5 && // 只缓存前5页
               filter.PageSize <= 20;
    }

    /// <summary>
    /// 清除评论相关缓存
    /// </summary>
    /// <param name="snippetId">代码片段ID</param>
    private void ClearCommentCache(Guid? snippetId)
    {
        if (!snippetId.HasValue)
            return;

        var keysToRemove = _commentCache.Keys
            .Where(key => key.StartsWith($"comments_{snippetId.Value}"))
            .ToList();

        foreach (var key in keysToRemove)
        {
            _commentCache.TryRemove(key, out _);
        }

        _logger.LogDebug("清除评论缓存成功，代码片段ID: {SnippetId}", snippetId.Value);
    }

    #endregion
}

/// <summary>
/// 缓存条目
/// </summary>
internal class CacheEntry
{
    public PaginatedResult<CommentDto> Data { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// 评论审核请求DTO
/// </summary>
public class ModerateCommentDto
{
    /// <summary>
    /// 审核状态
    /// </summary>
    [Required(ErrorMessage = "审核状态不能为空")]
    public CommentStatus Status { get; set; }

    /// <summary>
    /// 审核原因
    /// </summary>
    [StringLength(500, ErrorMessage = "审核原因长度不能超过500个字符")]
    public string? Reason { get; set; }
}

/// <summary>
/// 评论点赞DTO
/// </summary>
public class CommentLikeDto
{
    public Guid Id { get; set; }
    public Guid CommentId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 批量操作结果DTO
/// </summary>
public class BatchOperationResultDto
{
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> ErrorMessages { get; set; } = new();
    public List<Guid> SuccessfulIds { get; set; } = new();
    public List<Guid> FailedIds { get; set; } = new();
}