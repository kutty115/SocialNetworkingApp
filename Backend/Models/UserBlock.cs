using System.ComponentModel.DataAnnotations;

namespace SocialNetworkApi.Models;

public class UserBlock
{
    public int Id { get; set; }

    [Required]
    public string BlockerId { get; set; } = default!;
    public ApplicationUser Blocker { get; set; } = default!;

    [Required]
    public string BlockedId { get; set; } = default!;
    public ApplicationUser Blocked { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}