using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WishServer.Domain;
using WishServer.Repository;

namespace WishServer.Service.impl
{
    public class GameGiftService : ServiceBase, IGameGiftService
    {

        IGameGiftRepository _gameGiftRepository;


        public GameGiftService(IGameGiftRepository gameGiftRepository)
        {
            _gameGiftRepository = gameGiftRepository;
        }

        public async Task<GameGiftPO> GetOne(string roomId, string userId)
        {
            return await _gameGiftRepository.GetDbSet().Where(t=> roomId == t.RoomId && userId.Equals(t.UserId)).FirstOrDefaultAsync();
        }

        public async Task UpSert()
        {
            GameGiftPO po = new GameGiftPO
            {
                GameId = 1,
                Platform = "DY",
                RoomId = "1",
                RoundId = 1,
                UserId = "u2",
                GiftId = "g1",
                GiftCount = 1,
                GiftMoney = 1,
            };
            List<GameGiftPO> a = new List<GameGiftPO> { };
            var updators = new Dictionary<Expression<Func<GameGiftPO, object>>, string>
            {
                {t=>t.GiftCount,"gift_count+values(gift_count)"},
                {t=>t.GiftMoney,"gift_money+values(gift_money)"}
            };
            var ret = await _gameGiftRepository.UpsertAsync(po, updators);
            Console.WriteLine(ret);
        }
    }
}
