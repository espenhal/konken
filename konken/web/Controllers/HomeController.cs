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
				
				if(DateTime.Now.Hour < 24)
					throw  new Exception("sdfsdfsdf");
			}
			catch (Exception e)
			{
				Log.Error(e, "Error calculating league standing.");
				//return RedirectToAction("Index", "Error", e);
			}

			return View(leagueStanding);
		}

		public ActionResult Rules()
		{
			return View();
		}
	}
}