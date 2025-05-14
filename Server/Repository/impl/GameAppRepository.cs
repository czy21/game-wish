using Microsoft.EntityFrameworkCore;
using Sunny.Framework.DB.Repository;
using WishServer.Domain;
using WishServer.Model.BO;

namespace WishServer.Repository.impl;

public class GameAppRepository(AppDbContext dbContext) : RepositoryBase<long?, GameAppPO>(dbContext), IGameAppRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<GameAppBO> SelectOneByPlatformAndGameCode(string platform, string gameCode)
    {
        var ret = from g in _dbContext.Games
            join ga in _dbContext.GameApps on g.Id equals ga.GameId
            where ga.Platform == platform && g.Code == gameCode
            select new GameAppBO
            {
                Game = g,
                GameApp = ga
            };
        return await ret.FirstOrDefaultAsync();
    }
}