using AutoMapper;
using WishServer.Domain;
using WishServer.Model.DTO;

namespace WishServer.AutoMapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<GameRoomPO, GameRoomDTO>();
            CreateMap<GameRoomDTO, GameRoomPO>();
        }
    }
}
