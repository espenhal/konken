using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using web.Models.View;

namespace web.Code
{
    public class Calculations
    {
        public async Task<League> CalculateLeague(string fplLeagueId, int? round = null)
        {
            return new League();
//            //fetch league
//            League league = 
//
//            if (league != null)
//            {
//                //fetch players
//
//                //fetch gameweeks
//
//                league.Players = ;
//
//                foreach (var player in league.Players)
//                {
//                    player.Gameweeks =
//                        Mapper.Map<List<GameweekEntity>, List<Gameweek>>(
//                            gameweeksEntities.Results.Where(
//                                x => x.FplLeagueId == fplLeagueId && x.FplPlayerId == player.FplPlayerId).ToList());
//
//                    if (round == null)
//                        round = player.Gameweeks.Count;
//
//                    player.Gameweeks = player.Gameweeks.Where(x => x.Number <= round).OrderBy(x => x.Number).ToList();
//                }
//            }
//            return league;
        }
    }
}