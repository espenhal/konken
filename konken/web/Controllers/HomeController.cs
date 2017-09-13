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

namespace web.Controllers
{
	public class HomeController : BaseController
	{
		public async Task<ActionResult> Index()
		{
			var leagueStanding = await CalculateLeagueStanding(ConfigurationManager.AppSettings["leagueid"], null);

			leagueStanding.PlayerStandings = leagueStanding.PlayerStandings.OrderByDescending(x => x.Points).ToList();

			return View(leagueStanding);
		}

		public ActionResult Rules()
		{
			return View();
		}
	}
}