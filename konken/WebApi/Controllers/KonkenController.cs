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
        public async Task<IHttpActionResult> Get(string fplLeagueId)
        {
            try
            {
                return Ok(await GetLeague(fplLeagueId));
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
        public async Task<IHttpActionResult> GetLeagueStanding(string fplLeagueId)
        {
            try
            {
                var league = await GetLeague(fplLeagueId);

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
                        Points = player.Gameweeks.Sum(x => x.Points),
                        PointsOnBench = player.Gameweeks.Sum(x => x.PointsOnBench),
                        Transfers = player.Gameweeks.Sum(x => x.Transfers),
                        TransferCosts = player.Gameweeks.Sum(x => x.TransferCosts),
                        Chips = CalculateChips(player, league),
                        Value = player.Gameweeks.OrderBy(x => x.Number).Last().Value,
                        Rank = 1,
                        Cash = CalculateCash(player, league),
                        Rounds = CalculateRounds(player, league)
                    };

                    leagueStanding.PlayerStandings.Add(playerStanding);
                }

                leagueStanding.PlayerStandings = leagueStanding.PlayerStandings.OrderBy(x => x.Points).ToList();

                return Ok(leagueStanding);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        private IList<int> CalculateRounds(Player player, League league)
        {
            return new List<int>();
            //var rounds = new List<int>();

            //for (int i = 0; i < player.Gameweeks.Count; i++)
            //{
            //    var check = true;
            //    foreach (var compare in league.Players)
            //    {
            //        if (player.Gameweeks[i].Points >)
            //    }
            //    if (check)
            //        rounds.Add();
            //}

            ////for (int i = 1; i <= player.Gameweeks.Count; i++)
            ////{
            ////    if (player.Gameweeks[i].Points >)
            ////}

            ////foreach (var compare in league.Players)
            ////{


            ////}
            //return rounds;
        }

        private int CalculateCash(Player player, League league)
        {
            return 0;
        }

        private IList<Chip> CalculateChips(Player player, League league)
        {
            return new List<Chip>();
        }

        private async Task<League> GetLeague(string fplLeagueId)
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

                league.Players = Mapper.Map<IList<PlayerEntity>, IList<Player>>(playerEntities.Results);

                foreach (var player in league.Players)
                {
                    player.Gameweeks =
                        Mapper.Map<IList<GameweekEntity>, IList<Gameweek>>(
                            gameweeksEntities.Results.Where(
                                x => x.FplLeagueId == fplLeagueId && x.FplPlayerId == player.FplPlayerId).ToList());

                    player.Gameweeks = player.Gameweeks.OrderBy(x => x.Number).ToList();
                }
            }
            return league;
        }
    }
}