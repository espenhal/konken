using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using common;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Serilog;
using web.Models;

namespace web.Controllers
{
	public class HomeController : BaseController
	{
		public async Task<ActionResult> Index()
		{
			LeagueStanding leagueStanding = new LeagueStanding();

			try
			{
				leagueStanding = await CalculateLeagueStanding(ConfigurationManager.AppSettings["leagueid"], null);

				leagueStanding.PlayerStandings = leagueStanding.PlayerStandings.OrderByDescending(x => x.Points).ToList();
			}
			catch (Exception e)
			{
				Log.Error(e, "Error calculating league standing.");
				//return RedirectToAction("Index", "Error", e);
			}

			return View(leagueStanding);
		}

	    public async Task<ActionResult> Gameweeks(int? gameweek = null)
	    {
            List<PlayerGameweek> leagueGameweek = new List<PlayerGameweek>();
	        List<int> gameweeks = new List<int>();

            try
	        {
	            var fplLeagueId = ConfigurationManager.AppSettings["leagueid"];
                
                var league = await CalculateLeague(fplLeagueId);

	            gameweeks = league.Players.First().Gameweeks.OrderByDescending(x => x.Number).Select(x => x.Number).ToList();

	            if (gameweek == null || gameweek < 1 || gameweek > gameweeks.Max()) gameweek = gameweeks.Max();

                leagueGameweek = (await CalculateLeagueGameweek(fplLeagueId, gameweek.Value)).PlayerGameweeks
	                .OrderByDescending(x => x.PointsExcludedTransferCosts).ThenByDescending(x => x.PointsOnBench)
	                .ThenByDescending(x => x.ScoredGoals).ToList();
            }
	        catch (Exception e)
	        {
	            Log.Error(e, "Error calculating league standing.");
                //return RedirectToAction("Index", "Error", e);
	        }

	        return View(new Gameweeks { LeagueGameweek = leagueGameweek, GameweekCount = gameweeks, GameweekSelected = gameweek });
        }

        public async Task<ActionResult> Rules()
		{
		    League league = new League();

		    try
		    {
		        league = await CalculateLeague(ConfigurationManager.AppSettings["leagueid"], null);
		    }
		    catch (Exception e)
		    {
		        Log.Error(e, "Error calculating league standing.");
		        //return RedirectToAction("Index", "Error", e);
		    }
            
		    return View(league);
        }
	}
}