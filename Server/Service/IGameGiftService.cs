using WishServer.Domain;

namespace WishServer.Service
{
    public interface IGameGiftService : IServiceBase
    {
        Task UpSert();
        Task<GameGiftPO?> GetOne(string roomId,string userId);
    }
}
