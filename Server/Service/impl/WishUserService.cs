
using WishServer.Domain;
using WishServer.Model;
using WishServer.Repository;

namespace WishServer.Service.impl
{
    public class WishUserService : ServiceBase, IWishUserService
    {
        private readonly IWishUserRepository _wishUserRepository;
        private readonly DbMasterContext _dbMasterContext;

        public WishUserService(
            IWishUserRepository wishUserRepository,
            DbMasterContext dbMasterContext)
        {

            _wishUserRepository = wishUserRepository;
            _dbMasterContext = dbMasterContext;
        }

        public async Task TestTransaction(long id, bool error)
        {
            WishUserPO u1 = new()
            {
                Id = id,
                Platform = PlatformEnum.KS.ToString(),
                AnchorUid = "",
                AudienceUid = "",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
            };
            
            await using var transaction = await _dbMasterContext.Database.BeginTransactionAsync();
            try
            {
                //await _wishUserRepository.InsertAsync(u1);

                //if (error)
                //{
                //    throw new Exception("aa");
                //}

                //var updateCount = await _wishUserRepository.UpdateAsync(u1, false);
                //Console.WriteLine($"UpdateCount: {updateCount}");
                //await _wishUserRepository.DeleteByIdAsync(id);

                // batchInsert
                List<WishUserPO> pos = Enumerable.Range(0, 10).Select(t =>
                {
                    return new WishUserPO()
                    {
                        Platform = PlatformEnum.KS.ToString(),
                        AnchorUid = t.ToString(),
                        AudienceUid = "",
                        CreateTime = DateTime.Now,
                        UpdateTime = DateTime.Now,
                    };
                }).ToList();
                await _wishUserRepository.BatchInsertAsync(pos);
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
