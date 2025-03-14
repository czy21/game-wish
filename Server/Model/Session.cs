using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace WishServer.Model
{
    public class Session
    {
        public string ClientId { get; set; }

        public ConnectionInfo ConnectionInfo { get; set; }

        public WebSocket WebSocket { get; set; }

        public string? UserId { get; set; }
    }
}
