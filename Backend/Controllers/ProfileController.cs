using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkApi.Dtos;
using SocialNetworkApi.Models;
using System.Security.Claims;

namespace SocialNetworkApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    private string MeId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? throw new Exception("No user id claim found in token.");
    }

    // ✅ Get my profile
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var me = await _userManager.FindByIdAsync(MeId());
        if (me == null) return NotFound();

        return Ok(new
        {
            me.Id,
            me.Email,
            me.UserName,
            me.FullName,
            me.Bio,
            me.ProfileImageUrl
        });
    }

    // ✅ Update my profile
    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateMe(UpdateProfileDto dto)
    {
        var me = await _userManager.FindByIdAsync(MeId());
        if (me == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(dto.FullName))
            me.FullName = dto.FullName.Trim();

        me.Bio = dto.Bio; // allow empty or null
        me.ProfileImageUrl = dto.ProfileImageUrl;

        var result = await _userManager.UpdateAsync(me);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { message = "Profile updated successfully." });
    }

    // ✅ View another user's profile (public)
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var u = await _userManager.FindByIdAsync(id);
        if (u == null) return NotFound();

        return Ok(new
        {
            u.Id,
            u.UserName,
            u.FullName,
            u.Bio,
            u.ProfileImageUrl
        });
    }
}