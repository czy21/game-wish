using Demo.Repository;
using Microsoft.EntityFrameworkCore;
using WishServer.Domain;

namespace WishServer.Repository.impl;

public class WishUserRepository : IWishUserRepository
{
    private readonly DbMasterContext _dbContext;

    public WishUserRepository(DbMasterContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WishUserPO?> SelectById(long id)
    {
        return await _dbContext.WishUsers.FirstOrDefaultAsync(t => t!.Id == id);
    }
}