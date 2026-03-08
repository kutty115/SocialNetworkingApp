using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SocialNetworkApi.Realtime
{
    public class NameIdUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
            => connection.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}