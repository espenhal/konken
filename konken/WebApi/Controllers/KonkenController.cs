using System;
using System.Web.Http;
using Polly;
using WebApi.Code;

namespace WebRole1.Controllers
{
    public class KonkenController : ApiController
    {
        private readonly Policy _policy = Policy
            .Handle<Exception>()
            .WaitAndRetry(10, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
            );

        [HttpGet, Route("getleague")]
        public string GetLeagueHtml(string leagueId)
        {
            var leagueHtml =
                _policy.Execute(
                    () =>
                        HtmlScraper.GetHtmlByXPath(
                            $"https://fantasy.premierleague.com/a/leagues/standings/{leagueId}/classic", "//*[@id=\"ismr-classic-standings\"]/div/div/table/tbody"));

            return leagueHtml;
        }

        [HttpGet, Route("getplayer")]
        public string GetPlayerHtml(string playerId)
        {
            var playerHtml =
                _policy.Execute(
                    () =>
                        HtmlScraper.GetHtmlByXPath($"https://fantasy.premierleague.com/a/entry/{playerId}/history", "//*[@id=\"ismr-event-history\"]/div/div/div/table/tbody"));

            return playerHtml;
        }
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