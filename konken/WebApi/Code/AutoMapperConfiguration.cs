using System;
using AutoMapper;
using Common.Models;
using WebApi.Models;

namespace WebApi.Code
{
    public static class AutoMapperConfiguration
    {
        public static void Configure()
        {
            //var config = new MapperConfiguration(cfg =>
            //{
            //    cfg.CreateMap<Player, NewPlayer>();
            //});
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<LeagueProfile>();
                cfg.AddProfile<PlayerProfile>();
                cfg.AddProfile<GameweekProfile>();
            });
            config.AssertConfigurationIsValid();
        }
    }

    public class LeagueProfile : Profile
    {
        public LeagueProfile()
        {
            CreateMap<LeagueEntity, League>()
                //.ForMember(dest => dest.FplLeagueId, opt => opt.MapFrom(src => src.RowKey))
                ;
            CreateMap<League, LeagueEntity>()
                //.ForMember(dest => dest.RowKey, opt => opt.MapFrom(src => src.FplLeagueId))
                ;
            //    .ForMember(dest => dest.Id, opt => opt.UseValue(Guid.NewGuid()))
            //    .ForMember(dest => dest.Players, opt => opt.Ignore())
            //    ;
            //CreateMap<UpdateLeague, League>();
        }
    }

    public class PlayerProfile : Profile
    {
        public PlayerProfile()
        {
            CreateMap<PlayerEntity, Player>()
                //.ForMember(dest => dest.FplPlayerId, opt => opt.MapFrom(src => src.RowKey))
                ;
            CreateMap<Player, PlayerEntity>()
                //.ForMember(dest => dest.RowKey, opt => opt.MapFrom(src => src.FplPlayerId))
                ;
            //    
            //    ;
            //CreateMap<Player, GameWeekEntity>()
            //    .ForMember(dest => dest.PartitionKey, opt => opt.Ignore())
            //    .ForMember(dest => dest.RowKey, opt => opt.MapFrom(src => src.FplPlayerId))
            //    ;
        }
    }

    public class GameweekProfile : Profile
    {
        public GameweekProfile()
        {
            CreateMap<GameweekEntity, Gameweek>()
                //.ForMember(dest => dest.Number, opt => opt.MapFrom(src => src.RowKey))
                ;
            CreateMap<Gameweek, GameweekEntity>()
                //.ForMember(dest => dest.RowKey, opt => opt.MapFrom(src => src.Number))
                ;
            //    .ForMember(dest => dest.Id, opt => opt.UseValue(Guid.NewGuid()))
            //    .ForMember(dest => dest.Player, opt => opt.Ignore())
            //    ;
            //CreateMap<UpdateGameweek, Gameweek>();
        }
    }
}