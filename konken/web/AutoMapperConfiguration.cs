using AutoMapper;
using common;

namespace web
{
	public static class AutoMapperConfiguration
	{
		public static void Configure()
		{
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
				;
			CreateMap<League, LeagueEntity>()
				;
			CreateMap<LeagueEntity, LeagueGameweek>()
				;
		}
	}

	public class PlayerProfile : Profile
	{
		public PlayerProfile()
		{
			CreateMap<PlayerEntity, Player>()
				;
			CreateMap<Player, PlayerEntity>()
				;
		}
	}

	public class GameweekProfile : Profile
	{
		public GameweekProfile()
		{
			CreateMap<GameweekEntity, Gameweek>()
				.ForMember(dest => dest.Cup, opt => opt.ResolveUsing<GameweekCupResolver>())
				;
			CreateMap<Gameweek, GameweekEntity>()
				.ForMember(dest => dest.HomeFplPlayerId, opt => opt.MapFrom(src => src.Cup.HomeFplPlayerId))
				.ForMember(dest => dest.HomeName, opt => opt.MapFrom(src => src.Cup.HomeName))
				.ForMember(dest => dest.HomeTeamName, opt => opt.MapFrom(src => src.Cup.HomeTeamName))
				.ForMember(dest => dest.HomePoints, opt => opt.MapFrom(src => src.Cup.HomePoints))
				.ForMember(dest => dest.AwayFplPlayerId, opt => opt.MapFrom(src => src.Cup.AwayFplPlayerId))
				.ForMember(dest => dest.AwayName, opt => opt.MapFrom(src => src.Cup.AwayName))
				.ForMember(dest => dest.AwayTeamName, opt => opt.MapFrom(src => src.Cup.AwayTeamName))
				.ForMember(dest => dest.AwayPoints, opt => opt.MapFrom(src => src.Cup.AwayPoints))
				;
			CreateMap<GameweekEntity, PlayerGameweek>()
				;
		}
	}

	public class GameweekCupResolver : IValueResolver<GameweekEntity, Gameweek, Cup>
	{
		public Cup Resolve(GameweekEntity source, Gameweek destination, Cup destMember, ResolutionContext context)
		{
			if (source.HomeFplPlayerId == null || source.HomeName == null || source.HomeTeamName == null ||
			    source.HomePoints == null || source.AwayFplPlayerId == null || source.AwayName == null ||
			    source.AwayTeamName == null || source.AwayPoints == null)
				return null;

			return new Cup()
			{
				GamewekkNumber = source.Number,
				HomeFplPlayerId = source.HomeFplPlayerId,
				HomeName = source.HomeName,
				HomeTeamName = source.HomeTeamName,
				HomePoints = source.HomePoints,
				AwayFplPlayerId = source.AwayFplPlayerId,
				AwayName = source.AwayName,
				AwayTeamName = source.AwayTeamName,
				AwayPoints = source.AwayPoints
			};
		}
	}
}