using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Common.Models;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using WebApi.Code;
using WebApi.Models;

namespace WebApi.Controllers
{
    public class KonkenController : BaseApiController
    {
        private CloudTable Table { get; set; }
        private CloudQueue Queue { get; set; }

        public KonkenController()
        {
            // Parse the connection string and return a reference to the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
#if DEBUG
                "UseDevelopmentStorage=true;"
#else
                CloudConfigurationManager.GetSetting("StorageConnectionString")
#endif
                );

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            // Retrieve a reference to the table.
            Table = tableClient.GetTableReference("konkentable");
            // Create the table if it doesn't exist.
            Table.CreateIfNotExists();

            // Create the queue client
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            // Retrieve a reference to the queue
            Queue = queueClient.GetQueueReference("konkenqueue");
            // Create the queue if it doesn't exist
            Queue.CreateIfNotExists();
        }

        [HttpGet, Route("scrapeleague")]
        public async Task<IHttpActionResult> ScrapeLeague(string fplLeagueId)
        {
            var queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(new UpdateLeagueMessage { FplLeagueId = fplLeagueId }));
            await Queue.AddMessageAsync(queueMessage);

            return Ok();
        }

        [HttpGet, Route("getleague")]
        public async Task<IHttpActionResult> Get(string fplLeagueId, int? round = null)
        {
            try
            {
                return Ok(await CalculateLeague(fplLeagueId, round));
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        [HttpPost, Route("updateleague")]
        public async Task<IHttpActionResult> Post(League league)
        {
            // League
            LeagueEntity leagueEntity = Mapper.Map<League, LeagueEntity>(league);
            leagueEntity.PartitionKey = "League";
            leagueEntity.RowKey = league.FplLeagueId;

            TableOperation leagueInsertOrReplaceOperation = TableOperation.InsertOrReplace(leagueEntity);

            TableResult leagueInsertOrReplaceResult = await Table.ExecuteAsync(leagueInsertOrReplaceOperation);

            // Players & Gameweeks
            TableBatchOperation playerBatchInsertOrReplaceOperation = new TableBatchOperation();

            foreach (var player in league.Players)
            {
                PlayerEntity playerEntity = Mapper.Map<Player, PlayerEntity>(player);
                playerEntity.PartitionKey = "Player";
                playerEntity.RowKey = league.FplLeagueId + "|" + player.FplPlayerId;

                playerBatchInsertOrReplaceOperation.InsertOrReplace(playerEntity);

                TableBatchOperation gameweekBatchInsertOrReplaceOperation = new TableBatchOperation();

                foreach (var gameweek in player.Gameweeks)
                {
                    GameweekEntity gameweekEntity = Mapper.Map<Gameweek, GameweekEntity>(gameweek);
                    gameweekEntity.PartitionKey = "Gameweek";
                    gameweekEntity.RowKey = league.FplLeagueId + "|" + player.FplPlayerId + "|" + gameweek.Number;
                    gameweekEntity.FplPlayerId = player.FplPlayerId;
                    gameweekEntity.FplLeagueId = league.FplLeagueId;

                    gameweekBatchInsertOrReplaceOperation.InsertOrReplace(gameweekEntity);
                }

                IList<TableResult> gameweekBatchInsertOrReplaceResult = await Table.ExecuteBatchAsync(gameweekBatchInsertOrReplaceOperation);
            }

            IList<TableResult> playerBatchInsertOrReplaceResult = await Table.ExecuteBatchAsync(playerBatchInsertOrReplaceOperation);

            return Ok();
        }

        [HttpGet, Route("getstanding")]
        public async Task<IHttpActionResult> GetLeagueStanding(string fplLeagueId, int? round = null)
        {
            try
            {
                return Ok(await CalculateLeagueStanding(fplLeagueId, round));
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        private async Task<LeagueStanding> CalculateLeagueStanding(string fplLeagueId, int? round)
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
                    CupRounds = player.Gameweeks.Count(x => x.Cup)
                };

                leagueStanding.PlayerStandings.Add(playerStanding);
            }

            leagueStanding.PlayerStandings = leagueStanding.PlayerStandings.OrderBy(x => x.Points).ToList();

            CalculateHalfSeasonCash(leagueStanding, league);
            CalculateCupCash(leagueStanding, league);

            return leagueStanding;
        }

        private double CalculateGameweekWinnerCash(Player player, League league)
        {
            return 200 * CalculatePlayerGameweekWinners(player, league).Count;
        }

        private static void CalculateHalfSeasonCash(LeagueStanding leagueStanding, League league)
        {
            if (league.Players.First().Gameweeks.Count >= 20)
                leagueStanding.PlayerStandings.Last().Cash += league.Players.Count * 62.5;
        }
        
        private static void CalculateCupCash(LeagueStanding leagueStanding, League league)
        {
            Dictionary<string, int> playerCupAppearances = new Dictionary<string, int>();

            foreach (var player in league.Players)
            {
                playerCupAppearances.Add(player.FplPlayerId, player.Gameweeks.Count(x => x.Cup));
            }

            int max = playerCupAppearances.Max(x => x.Value);

            var playersFurthestInCup = playerCupAppearances.Where(x => x.Value == max).Select(x => x.Key).ToList();

            foreach (var player in playersFurthestInCup)
            {
                leagueStanding.PlayerStandings.First(x => x.FplPlayerId == player).Cash += 500.0 / playersFurthestInCup.Count;
            }
        }

        //TODO full season cash

        private List<int> CalculatePlayerGameweekWinners(Player player, League league)
        {
            return CalculateGameweekWinners(league).Where(x => x.FplPlayerId == player.FplPlayerId).Select(x => x.Number).ToList();
        }

        private List<GameweekWinner> CalculateGameweekWinners(League league)
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

        private async Task<League> CalculateLeague(string fplLeagueId, int? round = null)
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
    }
}