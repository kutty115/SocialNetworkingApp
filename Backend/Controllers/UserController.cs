using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkApi.Models;
using System.Security.Claims;

namespace SocialNetworkApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await _userManager.FindByIdAsync(userId);

        return Ok(new { user!.Id, user.Email, user.FullName, user.Bio, user.ProfileImageUrl });
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] ApplicationUser updated)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await _userManager.FindByIdAsync(userId);

        user!.FullName = updated.FullName;
        user.Bio = updated.Bio;
        user.ProfileImageUrl = updated.ProfileImageUrl;

        await _userManager.UpdateAsync(user);

        return Ok(new { message = "Profile updated" });
    }

    // ✅ ADD THIS: /api/users  (for chat user list)
    [HttpGet]
    public IActionResult GetAllUsers()
    {
        var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var users = _userManager.Users
            .Where(u => u.Id != myId)
            .Select(u => new
            {
                id = u.Id,
                fullName = u.FullName,
                email = u.Email,
                profileImageUrl = u.ProfileImageUrl
            })
            .ToList();

        return Ok(users);
    }
}