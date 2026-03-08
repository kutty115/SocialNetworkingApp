namespace SocialNetworkApi.Models;

public enum ReportStatus { Open, InReview, Resolved, Rejected }

public class Report
{
    public int Id { get; set; }

    public string ReporterId { get; set; } = "";
    public ApplicationUser? Reporter { get; set; }

    public int? PostId { get; set; }
    public Post? Post { get; set; }

    public string Reason { get; set; } = "";
    public ReportStatus Status { get; set; } = ReportStatus.Open;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}