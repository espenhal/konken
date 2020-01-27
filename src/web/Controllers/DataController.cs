using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Services;
using web.Models.Data;
using web.Settings;

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
            LeagueStanding league = await _fplApiWrapper.GetLeagueStanding(_appSettings.LeagueId);

            List<TeamHistory> teamHistories = new List<TeamHistory>();
            foreach (var standing in league.standings.results)
            {
                teamHistories.Add(await _fplApiWrapper.GetTeamHistory(standing.entry.ToString()));
            }
            
            return Ok(new {league, teamHistories});
        }
    }
}