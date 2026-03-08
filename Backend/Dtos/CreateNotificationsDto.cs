namespace SocialNetworkApi.DTOs
{
    public class CreateNotificationDto
    {
        public string Type { get; set; } = "";
        public string Text { get; set; } = "";
        public string? Link { get; set; }
    }
}