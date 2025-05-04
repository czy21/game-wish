using WishServer.Model.DTO;
using WishServer.Repository;

namespace WishServer.Service.impl
{
    public class GameRoomService : ServiceBase, IGameRoomService
    {
        IGameRoomRepository _gameRoomRepository;
        public GameRoomService(
            IGameRoomRepository gameRoomRepository
            ) { 
            _gameRoomRepository = gameRoomRepository;
        }

        public async Task<GameRoomDTO?> AggRoom(string roomId)
        {
            return await _gameRoomRepository.AggRoom(roomId);
        }
    }
}
