using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.DTOs;
using CodeSnippetManager.Api.Attributes;
using CodeSnippetManager.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace CodeSnippetManager.Api.Controllers;

/// <summary>
/// 标签管理控制器 - 遵循单一职责原则，只负责标签相关的HTTP请求处理
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;
    private readonly ILogger<TagsController> _logger;

    public TagsController(
        ITagService tagService,
        ILogger<TagsController> logger)
    {
        _tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 获取所有标签列表
    /// </summary>
    /// <returns>标签列表</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TagDto>>> GetAllTags()
    {
        try
        {
            var tags = await _tagService.GetAllTagsAsync();
            return Ok(tags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取标签列表时发生错误");
            return StatusCode(500, new { message = "获取标签列表失败" });
        }
    }

    /// <summary>
    /// 根据ID获取标签详情
    /// </summary>
    /// <param name="id">标签ID</param>
    /// <returns>标签详情</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TagDto>> GetTag(Guid id)
    {
        try
        {
            var tag = await _tagService.GetTagAsync(id);
            if (tag == null)
            {
                return NotFound(new { message = "标签不存在" });
            }

            return Ok(tag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取标签 {TagId} 时发生错误", id);
            return StatusCode(500, new { message = "获取标签详情失败" });
        }
    }

    /// <summary>
    /// 创建新标签
    /// </summary>
    /// <param name="createTagDto">创建标签请求</param>
    /// <returns>创建的标签</returns>
    [HttpPost]
    [RequireRole(UserRole.Editor, UserRole.Admin)]
    public async Task<ActionResult<TagDto>> CreateTag([FromBody] CreateTagDto createTagDto)
    {
        try
        {
            // 输入验证
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdTag = await _tagService.CreateTagAsync(createTagDto);
            
            _logger.LogInformation("用户创建了新标签: {TagName}", createdTag.Name);
            
            return CreatedAtAction(
                nameof(GetTag), 
                new { id = createdTag.Id }, 
                createdTag);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "创建标签失败: {Message}", ex.Message);
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "创建标签参数无效: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建标签时发生未知错误");
            return StatusCode(500, new { message = "创建标签失败" });
        }
    }

    /// <summary>
    /// 更新标签信息
    /// </summary>
    /// <param name="id">标签ID</param>
    /// <param name="updateTagDto">更新标签请求</param>
    /// <returns>更新后的标签</returns>
    [HttpPut("{id:guid}")]
    [RequireRole(UserRole.Editor, UserRole.Admin)]
    public async Task<ActionResult<TagDto>> UpdateTag(Guid id, [FromBody] UpdateTagDto updateTagDto)
    {
        try
        {
            // 输入验证
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedTag = await _tagService.UpdateTagAsync(id, updateTagDto);
            
            _logger.LogInformation("标签 {TagId} 已更新", id);
            
            return Ok(updatedTag);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "更新标签 {TagId} 失败: {Message}", id, ex.Message);
            return ex.Message.Contains("不存在") ? NotFound(new { message = ex.Message }) : BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "更新标签 {TagId} 失败: {Message}", id, ex.Message);
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新标签 {TagId} 时发生未知错误", id);
            return StatusCode(500, new { message = "更新标签失败" });
        }
    }

    /// <summary>
    /// 删除标签
    /// </summary>
    /// <param name="id">标签ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id:guid}")]
    [RequireRole(UserRole.Admin)]
    public async Task<ActionResult> DeleteTag(Guid id)
    {
        try
        {
            var success = await _tagService.DeleteTagAsync(id);
            if (!success)
            {
                return NotFound(new { message = "标签不存在" });
            }

            _logger.LogInformation("标签 {TagId} 已删除", id);
            
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "删除标签 {TagId} 失败: {Message}", id, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除标签 {TagId} 时发生未知错误", id);
            return StatusCode(500, new { message = "删除标签失败" });
        }
    }

    /// <summary>
    /// 搜索标签用于自动补全
    /// </summary>
    /// <param name="prefix">搜索前缀</param>
    /// <param name="limit">结果数量限制</param>
    /// <returns>匹配的标签列表</returns>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<TagDto>>> SearchTags(
        [FromQuery, Required] string prefix,
        [FromQuery] int limit = 10)
    {
        try
        {
            // 输入验证
            if (string.IsNullOrWhiteSpace(prefix))
            {
                return BadRequest(new { message = "搜索前缀不能为空" });
            }

            if (limit <= 0 || limit > 50)
            {
                limit = 10;
            }

            var tags = await _tagService.SearchTagsAsync(prefix, limit);
            return Ok(tags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "搜索标签时发生错误，前缀: {Prefix}", prefix);
            return StatusCode(500, new { message = "搜索标签失败" });
        }
    }

    /// <summary>
    /// 获取标签使用统计
    /// </summary>
    /// <returns>标签使用统计列表</returns>
    [HttpGet("statistics")]
    [RequireRole(UserRole.Admin)]
    public async Task<ActionResult<IEnumerable<TagUsageDto>>> GetTagUsageStatistics()
    {
        try
        {
            var statistics = await _tagService.GetTagUsageStatisticsAsync();
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取标签使用统计时发生错误");
            return StatusCode(500, new { message = "获取标签使用统计失败" });
        }
    }

    /// <summary>
    /// 获取最常用的标签
    /// </summary>
    /// <param name="limit">结果数量限制</param>
    /// <returns>最常用标签列表</returns>
    [HttpGet("most-used")]
    public async Task<ActionResult<IEnumerable<TagDto>>> GetMostUsedTags([FromQuery] int limit = 20)
    {
        try
        {
            if (limit <= 0 || limit > 100)
            {
                limit = 20;
            }

            var tags = await _tagService.GetMostUsedTagsAsync(limit);
            return Ok(tags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取最常用标签时发生错误");
            return StatusCode(500, new { message = "获取最常用标签失败" });
        }
    }

    /// <summary>
    /// 检查标签是否可以删除
    /// </summary>
    /// <param name="id">标签ID</param>
    /// <returns>是否可以删除</returns>
    [HttpGet("{id:guid}/can-delete")]
    [RequireRole(UserRole.Admin)]
    public async Task<ActionResult<bool>> CanDeleteTag(Guid id)
    {
        try
        {
            var canDelete = await _tagService.CanDeleteTagAsync(id);
            return Ok(new { canDelete });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查标签 {TagId} 是否可删除时发生错误", id);
            return StatusCode(500, new { message = "检查标签删除状态失败" });
        }
    }

    /// <summary>
    /// 获取代码片段关联的标签
    /// </summary>
    /// <param name="snippetId">代码片段ID</param>
    /// <returns>关联的标签列表</returns>
    [HttpGet("by-snippet/{snippetId:guid}")]
    public async Task<ActionResult<IEnumerable<TagDto>>> GetTagsBySnippetId(Guid snippetId)
    {
        try
        {
            var tags = await _tagService.GetTagsBySnippetIdAsync(snippetId);
            return Ok(tags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取代码片段 {SnippetId} 的标签时发生错误", snippetId);
            return StatusCode(500, new { message = "获取代码片段标签失败" });
        }
    }
}