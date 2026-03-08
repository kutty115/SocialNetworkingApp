using Microsoft.AspNetCore.Identity;

namespace SocialNetworkApi.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = "";
    public string Bio { get; set; } = "";
    public string ProfileImageUrl { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}