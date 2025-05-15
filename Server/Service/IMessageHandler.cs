using WishServer.Model;

namespace WishServer.Service;

public interface IMessageHandler
{
    PlatformEnum GetPlatform();

    public string GetAccessTokenKey(string gameCode)
    {
        return $"game:{GetPlatform()}:{gameCode}:access_token";
    }

    Task<string> GetAccessToken(string gameCode);

    public string GetRoomKey(string roomId)
    {
        return $"game:{GetPlatform()}:ROOM:{roomId}";
    }

    public string GetMsgKey(string msgId)
    {
        return $"game:{GetPlatform()}:MSG:{msgId}";
    }

    Task Init(Session session);
    Task Exit(Session session);
}