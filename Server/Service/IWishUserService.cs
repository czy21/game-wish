namespace WishServer.Service
{
    public interface IWishUserService : IServiceBase
    {
        Task TestTransaction(long id, bool error);
    }
}
