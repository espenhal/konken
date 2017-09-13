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
	public class BaseController : Controller
	{
		public IMapper Mapper { get; set; }

		public CloudTable Table { get; set; }

		public BaseController()
		{
			// Parse the connection string and return a reference to the storage account.
			CloudStorageAccount storageAccount =
				CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

			// Create the table client.
			CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
			// Retrieve a reference to the table.
			Table = tableClient.GetTableReference(ConfigurationManager.AppSettings["tablestoragecontainer"]
#if DEBUG
				+ "test"
#endif
			);
			// Create the table if it doesn't exist.
			Table.CreateIfNotExists();
		}

		#region private parts

		internal async Task<LeagueGameweek> CalculateLeagueGameweek(string fplLeagueId, int round)
		{
			TableOperation retrieveLeagueOperation = TableOperation.Retrieve<LeagueEntity>("League", fplLeagueId);

			TableResult retrieveLeagueResult = await Table.ExecuteAsync(retrieveLeagueOperation);

			LeagueGameweek leagueGameweek = Mapper.Map<LeagueEntity, LeagueGameweek>((LeagueEntity)retrieveLeagueResult.Result);

			if (leagueGameweek != null)
			{
				TableQuery<PlayerEntity> playerTableQuery =
					new TableQuery<PlayerEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey",
						QueryComparisons.Equal, "Player"));

				TableQuerySegment<PlayerEntity> playerEntities =
					await Table.ExecuteQuerySegmentedAsync(playerTableQuery, new TableContinuationToken());

				var partitionKeyFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal,
					"Gameweek");
				string gameweekNumberFilter = TableQuery.GenerateFilterConditionForInt("Number", QueryComparisons.Equal,
					round);

				TableQuery<GameweekEntity> gameweekTableQuery =
					new TableQuery<GameweekEntity>().Where(TableQuery.CombineFilters(partitionKeyFilter,
						TableOperators.And, gameweekNumberFilter));

				TableQuerySegment<GameweekEntity> gameweeksEntities =
					await Table.ExecuteQuerySegmentedAsync(gameweekTableQuery, new TableContinuationToken());

				leagueGameweek.PlayerGameweeks = Mapper.Map<List<GameweekEntity>, List<PlayerGameweek>>(gameweeksEntities.Results);

				foreach (var playerGameweek in leagueGameweek.PlayerGameweeks)
				{
					playerGameweek.Name =
						playerEntities.Results.First(x => x.FplPlayerId == playerGameweek.FplPlayerId).Name;
					playerGameweek.TeamName =
						playerEntities.Results.First(x => x.FplPlayerId == playerGameweek.FplPlayerId).TeamName;
				}
			}
			return leagueGameweek;
		}

		internal async Task<League> CalculateLeague(string fplLeagueId, int? round = null)
		{
			TableOperation retrieveLeagueOperation = TableOperation.Retrieve<LeagueEntity>("League", fplLeagueId);

			TableResult retrieveLeagueResult = await Table.ExecuteAsync(retrieveLeagueOperation);

			League league = Mapper.Map<LeagueEntity, League>((LeagueEntity)retrieveLeagueResult.Result);

			if (league != null)
			{
				TableQuery<PlayerEntity> playerTableQuery =
					new TableQuery<PlayerEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey",
						QueryComparisons.Equal, "Player"));

				TableQuerySegment<PlayerEntity> playerEntities =
					await Table.ExecuteQuerySegmentedAsync(playerTableQuery, new TableContinuationToken());

				TableQuery<GameweekEntity> gameweekTableQuery =
					new TableQuery<GameweekEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey",
						QueryComparisons.Equal, "Gameweek"));

				TableQuerySegment<GameweekEntity> gameweeksEntities =
					await Table.ExecuteQuerySegmentedAsync(gameweekTableQuery, new TableContinuationToken());

				league.Players = Mapper.Map<List<PlayerEntity>, List<Player>>(playerEntities.Results);

				foreach (var player in league.Players)
				{
					player.Gameweeks =
						Mapper.Map<List<GameweekEntity>, List<Gameweek>>(
							gameweeksEntities.Results.Where(
								x => x.FplLeagueId == fplLeagueId && x.FplPlayerId == player.FplPlayerId).ToList());

					if (round == null)
						round = player.Gameweeks.Count;

					player.Gameweeks = player.Gameweeks.Where(x => x.Number <= round).OrderBy(x => x.Number).ToList();
				}
			}
			return league;
		}

		internal async Task<LeagueStanding> CalculateLeagueStanding(string fplLeagueId, int? round)
		{
			var league = await CalculateLeague(fplLeagueId, round);

			LeagueStanding leagueStanding = new LeagueStanding()
			{
				FplLeagueId = league.FplLeagueId,
				Name = league.Name,
				PlayerStandings = new List<PlayerStanding>()
			};
			foreach (var player in league.Players)
			{
				PlayerStanding playerStanding = new PlayerStanding()
				{
					FplPlayerId = player.FplPlayerId,
					Name = player.Name,
					TeamName = player.TeamName,
					Points = player.Gameweeks.OrderBy(x => x.Number).Last().OverallPoints, // total uten fratrekk for bytter
					PointsOnBench = player.Gameweeks.Sum(x => x.PointsOnBench),
					Transfers = player.Gameweeks.Sum(x => x.Transfers),
					TransferCosts = player.Gameweeks.Sum(x => x.TransferCosts),
					PointsTransferCostsExcluded = player.Gameweeks.Sum(x => x.Points), // total med fratrekk for bytter
					Chips = player.Gameweeks.Select(x => x.Chip).ToList(),
					Value = player.Gameweeks.OrderBy(x => x.Number).Last().Value,
					Rank = player.Gameweeks.OrderBy(x => x.Number).Last().OverallRank,
					Cash = CalculateGameweekWinnerCash(player, league),
					GameweeksWon = CalculatePlayerGameweekWinners(player, league),
					CupRounds = player.Gameweeks.Count(x => x.Cup != null)
				};

				leagueStanding.PlayerStandings.Add(playerStanding);
			}

			leagueStanding.PlayerStandings = leagueStanding.PlayerStandings.OrderBy(x => x.Points).ToList();

			CalculateHalfSeasonCash(leagueStanding, league);
			CalculateCupCash(leagueStanding, league);

			return leagueStanding;
		}

		internal double CalculateGameweekWinnerCash(Player player, League league)
		{
			return 200 * CalculatePlayerGameweekWinners(player, league).Count;
		}

		internal static void CalculateHalfSeasonCash(LeagueStanding leagueStanding, League league)
		{
			if (league.Players.First().Gameweeks.Count >= 20)
				leagueStanding.PlayerStandings.Last().Cash += league.Players.Count * 62.5;
		}

		internal static void CalculateCupCash(LeagueStanding leagueStanding, League league)
		{
			Dictionary<string, int> playerCupAppearances = new Dictionary<string, int>();

			foreach (var player in league.Players)
			{
				playerCupAppearances.Add(player.FplPlayerId, player.Gameweeks.Count(x => x.Cup != null));
			}

			int max = playerCupAppearances.Max(x => x.Value);

			if (max == 0)
				return;

			var playersFurthestInCup = playerCupAppearances.Where(x => x.Value == max).Select(x => x.Key).ToList();

			foreach (var player in playersFurthestInCup)
			{
				leagueStanding.PlayerStandings.First(x => x.FplPlayerId == player).Cash += 500.0 / playersFurthestInCup.Count;
			}
		}

		//TODO full season cash

		internal List<int> CalculatePlayerGameweekWinners(Player player, League league)
		{
			return CalculateGameweekWinners(league).Where(x => x.FplPlayerId == player.FplPlayerId).Select(x => x.Number).ToList();
		}

		internal List<GameweekWinner> CalculateGameweekWinners(League league)
		{
			List<GameweekWinner> gameweekWinners = new List<GameweekWinner>();

			var numberOfRounds = league.Players.FirstOrDefault()?.Gameweeks.Count;

			if (numberOfRounds == null)
				return null;

			for (var i = 1; i <= numberOfRounds.Value; i++)
			{
				GameweekWinner gameweekWinner = new GameweekWinner() { Number = i };

				foreach (var player in league.Players)
				{
					if (player?.Gameweeks == null || player.Gameweeks.Count < 1)
						continue;

					var better = false;

					if (string.IsNullOrEmpty(gameweekWinner.FplPlayerId))
					{
						better = true;
					}
					else
					{
						var gameweek = player.Gameweeks.FirstOrDefault(x => x.Number == i);

						if (gameweek == null)
							continue;

						if (gameweek.PointsExcludedTransferCosts > gameweekWinner.PointsExcludedTransferCosts)
						{
							better = true;
						}
						else
						{
							if (gameweek.PointsExcludedTransferCosts == gameweekWinner.PointsExcludedTransferCosts)
							{
								if (gameweek.PointsOnBench > gameweekWinner.PointsOnBench)
									better = true;
								else
								{
									if (gameweek.PointsOnBench == gameweekWinner.PointsOnBench)
									{
										if (gameweek.ScoredGoals > gameweekWinner.ScoredGoals)
											better = true;
										else
										{
											// poengdeling!
										}
									}
								}
							}
						}
					}

					if (better)
					{
						var fplPlayerId = player.FplPlayerId;
						var pointsExcludedTransferCosts =
							player.Gameweeks.FirstOrDefault(x => x.Number == i)?.PointsExcludedTransferCosts;
						var pointsOnBench = player.Gameweeks.FirstOrDefault(x => x.Number == i)?.PointsOnBench;

						if (string.IsNullOrEmpty(fplPlayerId) || pointsExcludedTransferCosts == null || pointsOnBench == null)
							continue;

						gameweekWinner.FplPlayerId = fplPlayerId;
						gameweekWinner.PointsExcludedTransferCosts = pointsExcludedTransferCosts.Value;
						gameweekWinner.PointsOnBench = pointsOnBench.Value;
						gameweekWinner.ScoredGoals = 0;
					}
				}
				gameweekWinners.Add(gameweekWinner);
			}

			return gameweekWinners;
		}

		#endregion
	}
}