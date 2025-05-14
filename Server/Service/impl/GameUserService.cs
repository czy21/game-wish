using System.Linq.Expressions;
using WishServer.Domain;
using WishServer.Repository;

namespace WishServer.Service.impl;

public class GameUserService : ServiceBase, IGameUserService
{
    private readonly IGameUserRepository _gameUserRepository;


    public GameUserService(IGameUserRepository gameUserRepository)
    {
        _gameUserRepository = gameUserRepository;
    }

    public async Task UpSert()
    {
        var po = new GameUserPO
        {
            GameId = 1,
            UserId = "u2",
            AvatarUrl = "a1",
            Nickname = "n5"
        };
        List<GameUserPO> a = new();
        var updators = new Dictionary<Expression<Func<GameUserPO, object>>, string>
        {
            { t => t.Nickname, "values(nickname)" }
        };
        var ret = await _gameUserRepository.UpsertAsync(po, updators);
        Console.WriteLine(ret);
    }
}