using System.Net.WebSockets;

namespace WishServer.Model
{
    public class Session
    {
        public ConnectionInfo ConnectionInfo { get; set; }

        public WebSocket WebSocket { get; set; }
    }
}
