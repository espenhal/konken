using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Common.Models;
using Polly;
using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Table;
using WebApi.Controllers;
using WebApi.Models;

// Namespace for Table storage types

namespace WebRole1.Controllers
{
    public class KonkenController : BaseApiController
    {
        private readonly Policy _policy = Policy
            .Handle<Exception>()
            .WaitAndRetry(10, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
            );

        private CloudTable Table { get; set; }

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
            Table = tableClient.GetTableReference("konken");

            // Create the table if it doesn't exist.
            Table.CreateIfNotExists();
        }

        [HttpGet, Route("getleague")]
        public async Task<IHttpActionResult> GetLeague(string fplLeagueId)
        {
            TableOperation retrieveLeagueOperation = TableOperation.Retrieve<LeagueEntity>("League", fplLeagueId);

            TableResult retrieveLeagueResult = await Table.ExecuteAsync(retrieveLeagueOperation);

            League league = Mapper.Map<LeagueEntity, League>((LeagueEntity)retrieveLeagueResult.Result);

            TableQuery<PlayerEntity> playerTableQuery =
                new TableQuery<PlayerEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Player"));

            TableQuerySegment<PlayerEntity> playerEntities = await Table.ExecuteQuerySegmentedAsync(playerTableQuery, new TableContinuationToken());

            TableQuery<GameweekEntity> gameweekTableQuery =
                    new TableQuery<GameweekEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Gameweek"));

            TableQuerySegment<GameweekEntity> gameweeksEntities = await Table.ExecuteQuerySegmentedAsync(gameweekTableQuery, new TableContinuationToken());

            league.Players = Mapper.Map<IList<PlayerEntity>, IList<Player>>(playerEntities.Results);

            foreach (var player in league.Players)
            {
                player.Gameweeks =
                    Mapper.Map<IList<GameweekEntity>, IList<Gameweek>>(
                        gameweeksEntities.Results.Where(
                            x => x.FplLeagueId == fplLeagueId && x.FplPlayerId == player.FplPlayerId).ToList());
            }

            return Ok(league);
        }

        [HttpPut, Route("updateleague")]
        public async Task<IHttpActionResult> PutLeague(League league)
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

        //[HttpPost, Route("createleague")]
        //public async Task<IHttpActionResult> PostLeague(League league)
        //{
        //    TableBatchOperation batchOperation = new TableBatchOperation();

        //    foreach (var player in league.Players)
        //    {
        //        PlayerEntity entity = Mapper.Map<Player, PlayerEntity>(player);
        //        entity.PartitionKey = league.FplLeagueId;

        //        batchOperation.Insert(entity);
        //    }

        //    IList<TableResult> restult = await Table.ExecuteBatchAsync(batchOperation);

        //    return Ok();
        //}

        //[HttpPost, Route("updateleague")]
        //public League PutLeague(League league)
        //{
        //    throw new NotImplementedException();
        //}

        //[HttpPost, Route("createLeague")]
        //public League PostLeague(League league)
        //{
        //    throw new NotImplementedException();
        //}

        //[HttpPost, Route("createPlayer")]
        //public Player PostPlayer(Player player)
        //{
        //    throw new NotImplementedException();
        //}

        //[HttpPut, Route("updateLeague")]
        //public League PutLeague(League league)
        //{
        //    throw new NotImplementedException();
        //}

        //[HttpPut, Route("updatePlayer")]
        //public Player PutPlayer(Player player)
        //{
        //    throw new NotImplementedException();
        //}


        //[HttpGet, Route("getleague")]
        //public League GetLeagueHtml(string leagueId)
        //{
        //    //var leagueHtml =
        //    //    _policy.Execute(
        //    //        () =>
        //    //            HtmlScraper.GetHtmlByXPath(
        //    //                $"https://fantasy.premierleague.com/a/leagues/standings/{leagueId}/classic", "//*[@id=\"ismr-classic-standings\"]/div/div/table/tbody"));

        //    //return leagueHtml;


        //    throw new NotImplementedException();
        //}

        //[HttpGet, Route("getplayer")]
        //public string GetPlayerHtml(string playerId)
        //{
        //    //var playerHtml =
        //    //    _policy.Execute(
        //    //        () =>
        //    //            HtmlScraper.GetHtmlByXPath($"https://fantasy.premierleague.com/a/entry/{playerId}/history", "//*[@id=\"ismr-event-history\"]/div/div/div/table/tbody"));

        //    //return playerHtml;
        //    throw new NotImplementedException();
        //}
    }
}

//private const string CacheName = "konkenfplleague";
//private const string CacheNameUpdate = "konkenfplleagueupdate";
//private MemoryCache _cache = MemoryCache.Default;

//        // GET api/<controller>
//        //[CacheOutput(ClientTimeSpan = 100, ServerTimeSpan = 100)]
//        public HttpResponseMessage Get()
//        {
//            if (_cache == null)
//                _cache = MemoryCache.Default;

//            if (_cache.Contains(CacheName))
//            {
//                if (_cache.Contains(CacheNameUpdate))
//                    _cache.Remove(CacheNameUpdate);

//                return Request.CreateResponse(HttpStatusCode.OK, new LeagueResponse { League = (League)_cache.Get(CacheName) });
//            }

//            if (_cache.Contains(CacheNameUpdate))
//            {
//                return Request.CreateResponse(HttpStatusCode.OK, new LeagueResponse { CacheUpdate = (DateTime)_cache.Get(CacheNameUpdate) });
//            }

//            _cache.Set(CacheNameUpdate, DateTime.UtcNow, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(2) });

//            try
//            {
//                var league = GetLeague(AppSettingsReader.FPLLeagueId);

//                if (league == null)
//                    return Request.CreateResponse(HttpStatusCode.NoContent, new LeagueResponse() { ErrorResponse = new ErrorResponse() { Message = "Unable to fecth league" } });

//                _cache.Set(CacheName, league, new CacheItemPolicy { SlidingExpiration = new TimeSpan(30, 0, 0) });

//                return Request.CreateResponse(HttpStatusCode.OK, new LeagueResponse { League = league });
//            }
//            catch (Exception e)
//            {
//                return Request.CreateResponse(HttpStatusCode.InternalServerError, new LeagueResponse { ErrorResponse = new ErrorResponse { Exception = e, Message = "Error fecthing league" } });
//            }
//            finally
//            {
//                if (_cache.Contains(CacheNameUpdate))
//                    _cache.Remove(CacheNameUpdate);
//            }
//        }

//        private League GetLeague(string leagueId)
//        {
//            var policy = Policy
//                .Handle<Exception>()
//                .WaitAndRetry(20, retryAttempt =>
//                    TimeSpan.FromSeconds(Math.Pow(3, retryAttempt))
//                );

//            var leagueHtml =
//                policy.Execute(
//                    () =>
//                        HtmlScraper.GetHtml(
//                            $"https://fantasy.premierleague.com/a/leagues/standings/{leagueId}/classic", "ismr-main"));

//            var league = HtmlParser.GetLeague(leagueHtml);

//            return league;
//        }

//        private Player GetPlayer(string playerId)
//        {
//            var policy = Policy
//                .Handle<Exception>()
//                .WaitAndRetry(20, retryAttempt =>
//                    TimeSpan.FromSeconds(Math.Pow(3, retryAttempt))
//                );

//            var playerHtml =
//                policy.Execute(
//                    () =>
//                        HtmlScraper.GetHtml($"https://fantasy.premierleague.com/a/entry/{playerId}/history", "ismr-main"));



//            var player.Gameweeks = policy.Execute(() => HtmlParser.GetNewGameWeekHistory(playerHtml));

//            //foreach (var player in league.Players)
//            //{
//            //    var playerHtml = HtmlScraper.GetHtml($"https://fantasy.premierleague.com/a/entry/{player.FplPlayerId}/history", "ismr-main");

//            //    player.Gameweeks = policy.Execute(() => HtmlParser.GetNewGameWeekHistory(playerHtml));
//            //}

//            //sjekke om alle spillere har like mange runder, hvis ikke kaste exception og få Polly til å kjøre en runde til

//            //return league;
//        }

//        // PUT
//        public HttpResponseMessage Put()
//        {
//            if (!_cache.Contains(CacheNameUpdate))
//            {
//                return Request.CreateResponse(HttpStatusCode.NoContent);
//            }

//            // oppdater cache

//            return Request.CreateResponse(HttpStatusCode.OK);
//        }

//        // DELETE
//        public HttpResponseMessage Delete()
//        {
//            _cache.Remove(CacheName);
//            _cache.Remove(CacheNameUpdate);

//            return Request.CreateResponse(HttpStatusCode.OK);
//        }
//    }
//}