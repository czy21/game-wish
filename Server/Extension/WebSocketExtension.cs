using System.Net.WebSockets;
using System.Text;
using WishServer.Util;

namespace WishServer.Extension
{
    public static class WebSocketExtension
    {

        public static Task SendTextAsync(this WebSocket webSocket, string value)
        {
            return webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(value)), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public static Task SendJsonAsnyc(this WebSocket webSocket,object value) {
            return webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonUtil.Serialize(value))), WebSocketMessageType.Text, true, CancellationToken.None);
        }

    }
}
