using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Services;
using web.Models.Data;
using web.Models.View;
using web.Settings;
using LeagueStanding = web.Models.Data.LeagueStanding;

namespace web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : Controller
    {
        private readonly IFplApiWrapper _fplApiWrapper;
        private readonly AppSettings _appSettings;

        public DataController(IFplApiWrapper fplApiWrapper, AppSettings appSettings)
        {
            _fplApiWrapper = fplApiWrapper;
            _appSettings = appSettings;
        }

        [HttpGet("bootstrap")]
        public async Task<IActionResult> GetBootstrap()
        {
            var league = await _fplApiWrapper.GetBootstrap();

            return Ok(league);
        }

        [HttpGet("league")]
        public async Task<IActionResult> GetLeague()
        {
            LeagueStanding leagueData = await _fplApiWrapper.GetLeagueStanding(_appSettings.LeagueId);
            
            Models.View.League league = new Models.View.League()
            {
                FplLeagueId = leagueData.league.id.ToString(),
                Name = leagueData.league.name,
                Players = new List<Player>()
            };
            
            foreach (var standing in leagueData.standings.results)
            {
                Player player = new Player
                {
                    Name = standing.player_name,
                    TeamName = standing.entry_name,
                    FplPlayerId = standing.entry.ToString(),
                    Gameweeks = new List<Gameweek>()
                };

                TeamHistory teamHistory = await _fplApiWrapper.GetTeamHistory(standing.entry.ToString());

                foreach (var gw in teamHistory.current)
                {
                    player.Gameweeks.Add(new Gameweek()
                    {
                        Number = gw.@event,
                        Points = gw.points,
                        PointsOnBench = gw.points_on_bench,
                        GameweekRank = gw.rank,
                        Transfers = gw.event_transfers,
                        TransferCosts = gw.event_transfers_cost,
                        OverallPoints = gw.total_points,
                        OverallRank = gw.overall_rank,
                        Value = gw.value
                    });
                }

                foreach (var chip in teamHistory.chips)
                {
                    player.Gameweeks.First(x => x.Number == chip.@event).Chip = chip.name;
                }
                
                league.Players.Add(player);
            }

            return Ok(league);
        }
    }
}