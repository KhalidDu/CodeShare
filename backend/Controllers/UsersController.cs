using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.DTOs;
using CodeSnippetManager.Api.Models;
using System.Security.Claims;

namespace CodeSnippetManager.Api.Controllers;

/// <summary>
/// 用户管理控制器 - 遵循单一职责原则，只负责用户管理相关的HTTP请求处理
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 获取所有用户列表 (仅管理员)
    /// </summary>
    /// <returns>用户列表</returns>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户列表时发生错误");
            return StatusCode(500, new { message = "获取用户列表时发生内部错误" });
        }
    }

    /// <summary>
    /// 根据ID获取用户详情 (仅管理员或用户本人)
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <returns>用户详情</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // 检查权限：管理员可以查看所有用户，普通用户只能查看自己
            if (currentUserRole != "Admin" && currentUserId != id.ToString())
            {
                return Forbid("权限不足");
            }

            var user = await _userService.GetUserAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "用户不存在" });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户 {UserId} 详情时发生错误", id);
            return StatusCode(500, new { message = "获取用户详情时发生内部错误" });
        }
    }

    /// <summary>
    /// 创建新用户 (仅管理员)
    /// </summary>
    /// <param name="createUserDto">创建用户请求</param>
    /// <returns>创建的用户信息</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userService.CreateUserAsync(createUserDto);
            _logger.LogInformation("管理员创建用户 {Username} 成功", createUserDto.Username);

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("创建用户 {Username} 失败: {Message}", createUserDto.Username, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建用户 {Username} 时发生错误", createUserDto.Username);
            return StatusCode(500, new { message = "创建用户时发生内部错误" });
        }
    }

    /// <summary>
    /// 更新用户信息 (仅管理员或用户本人)
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <param name="updateUserDto">更新用户请求</param>
    /// <returns>更新后的用户信息</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid id, [FromBody] UpdateUserDto updateUserDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // 检查权限：管理员可以更新所有用户，普通用户只能更新自己的基本信息
            if (currentUserRole != "Admin" && currentUserId != id.ToString())
            {
                return Forbid("权限不足");
            }

            // 普通用户不能修改角色和状态
            if (currentUserRole != "Admin")
            {
                updateUserDto.Role = null;
                updateUserDto.IsActive = null;
            }

            var user = await _userService.UpdateUserAsync(id, updateUserDto);
            _logger.LogInformation("用户 {UserId} 信息更新成功", id);

            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("更新用户 {UserId} 失败: {Message}", id, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新用户 {UserId} 时发生错误", id);
            return StatusCode(500, new { message = "更新用户时发生内部错误" });
        }
    }

    /// <summary>
    /// 删除用户 (仅管理员)
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 防止管理员删除自己
            if (currentUserId == id.ToString())
            {
                return BadRequest(new { message = "不能删除自己的账户" });
            }

            var result = await _userService.DeleteUserAsync(id);
            if (!result)
            {
                return NotFound(new { message = "用户不存在" });
            }

            _logger.LogInformation("管理员删除用户 {UserId} 成功", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除用户 {UserId} 时发生错误", id);
            return StatusCode(500, new { message = "删除用户时发生内部错误" });
        }
    }

    /// <summary>
    /// 切换用户状态 (启用/禁用) (仅管理员)
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <param name="isActive">是否启用</param>
    /// <returns>更新后的用户信息</returns>
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserDto>> ToggleUserStatus(Guid id, [FromBody] bool isActive)
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 防止管理员禁用自己
            if (currentUserId == id.ToString() && !isActive)
            {
                return BadRequest(new { message = "不能禁用自己的账户" });
            }

            var updateDto = new UpdateUserDto { IsActive = isActive };
            var user = await _userService.UpdateUserAsync(id, updateDto);

            _logger.LogInformation("管理员{Action}用户 {UserId}", isActive ? "启用" : "禁用", id);
            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("切换用户 {UserId} 状态失败: {Message}", id, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "切换用户 {UserId} 状态时发生错误", id);
            return StatusCode(500, new { message = "切换用户状态时发生内部错误" });
        }
    }

    /// <summary>
    /// 重置用户密码 (仅管理员)
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <param name="resetPasswordDto">重置密码请求</param>
    /// <returns>操作结果</returns>
    [HttpPost("{id}/reset-password")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordDto resetPasswordDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.ResetPasswordAsync(id, resetPasswordDto.NewPassword);
            if (!result)
            {
                return NotFound(new { message = "用户不存在" });
            }

            _logger.LogInformation("管理员重置用户 {UserId} 密码", id);
            return Ok(new { message = "密码重置成功" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "重置用户 {UserId} 密码时发生错误", id);
            return StatusCode(500, new { message = "重置密码时发生内部错误" });
        }
    }
}