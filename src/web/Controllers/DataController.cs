﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Services;
using web.Code;
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
            Bootstrap bootstrap = await _fplApiWrapper.GetBootstrap();

            return Ok(bootstrap);
        }

        [HttpGet("league")]
        public async Task<IActionResult> GetLeague()
        {
            Bootstrap bootstrap = await _fplApiWrapper.GetBootstrap();

            int gameWeekCount = bootstrap.events.Count;
            
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

                //todo: get scored goals to decide a tie-breaker
                
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

                Models.Data.Cup cup = await _fplApiWrapper.GetCup(standing.entry.ToString());

                foreach (var cupMatch in cup.cup_matches)
                {
                    player.Gameweeks.First(x => x.Number == cupMatch.@event).Cup = new Models.View.Cup()
                    {
                        HomeFplPlayerId = cupMatch.entry_1_entry.ToString(),
                        HomeName = cupMatch.entry_1_player_name,
                        HomeTeamName = cupMatch.entry_1_name,
                        HomePoints = cupMatch.entry_1_points,
                        AwayFplPlayerId = cupMatch.entry_2_entry.ToString(),
                        AwayName = cupMatch.entry_2_player_name,
                        AwayTeamName = cupMatch.entry_2_name,
                        AwayPoints = cupMatch.entry_2_points,
                        GameweekNumber = cupMatch.@event,
                        Winner = cupMatch.winner
                    };
                }
                
                league.Players.Add(player);
            }

            List<PlayerStanding> playerStandings = Calculations.CalculateLeagueStandings(league);
            
            return Ok(playerStandings.OrderByDescending(x => x.Points));
        }
    }
}