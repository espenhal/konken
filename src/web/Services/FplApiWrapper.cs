using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using web;
using web.Models.Data;

namespace Services
{
    public class FplApiWrapper : IFplApiWrapper
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private CookieContainer _cookieContainer = new CookieContainer();

        private IMemoryCache _cache;

        public FplApiWrapper(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache, string settingsLeagueId)
        {
            _httpClientFactory = httpClientFactory;
            _cache = memoryCache;
        }

        private async Task Login()
        {
            if (!_cache.TryGetValue(CacheKeys.Login, out CookieContainer cookieContainer))
            {
                using (var handler = new HttpClientHandler())
                {
                    using (var client = new HttpClient(handler))
                    {
                        client.BaseAddress = new Uri("https://users.premierleague.com/accounts/login/");

                        /*                     
                         request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
    request.AddParameter("password", "fhkpc3");
    request.AddParameter("login", "espenhal@hotmail.com");
    request.AddParameter("redirect_uri", "https://fantasy.premierleague.com/a/login");
    request.AddParameter("app", "plfpl-web");
                         */

                        HttpContent httpContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("password", "fhkpc3"),
                            new KeyValuePair<string, string>("login", "espenhal@hotmail.com"),
                            new KeyValuePair<string, string>("redirect_uri",
                                "https://fantasy.premierleague.com/a/login"),
                            new KeyValuePair<string, string>("app", "plfpl-web")
                        });

                        using (var response = await client.PostAsync("", httpContent))
                        {
                            var result = await response.Content.ReadAsStringAsync();

                            CookieCollection cookies =
                                handler.CookieContainer.GetCookies(new Uri("https://users.premierleague.com"));

                            cookieContainer = handler.CookieContainer;

                            var cacheEntryOptions = new MemoryCacheEntryOptions
                            {
                                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                            };

                            _cache.Set(CacheKeys.Login, cookieContainer, cacheEntryOptions);
                        }
                    }
                }
            }

            _cookieContainer = cookieContainer;
        }

        public async Task<Bootstrap> GetBootstrap()
        {
            using (var handler = new HttpClientHandler())
            {
                using (var client = new HttpClient(handler))
                {
                    client.BaseAddress = new Uri("https://fantasy.premierleague.com/api/");

                    using (var response = await client.GetAsync("bootstrap-static"))
                    {
                        return JsonConvert.DeserializeObject<Bootstrap>(await response.Content.ReadAsStringAsync());
                    }
                }
            }
        }

        public async Task<LeagueStanding> GetLeagueStanding(string leagueId)
        {
            await Login();

            using (var handler = new HttpClientHandler() {CookieContainer = _cookieContainer})
            {
                using (var client = new HttpClient(handler))
                {
                    client.BaseAddress = new Uri("https://fantasy.premierleague.com/api/");

                    using (var response = await client.GetAsync($"leagues-classic/{leagueId}/standings"))
                    {
                        return JsonConvert.DeserializeObject<LeagueStanding>(
                            await response.Content.ReadAsStringAsync());
                    }
                }
            }
        }

        public async Task<TeamHistory> GetTeamHistory(string teamId)
        {
            await Login();

            using (var handler = new HttpClientHandler() {CookieContainer = _cookieContainer})
            {
                using (var client = new HttpClient(handler))
                {
                    client.BaseAddress = new Uri("https://fantasy.premierleague.com/api/");

                    using (var response = await client.GetAsync($"entry/{teamId}/history/"))
                    {
                        return JsonConvert.DeserializeObject<TeamHistory>(await response.Content.ReadAsStringAsync());
                    }
                }
            }
        }
    }

    public interface IFplApiWrapper
    {
        Task<Bootstrap> GetBootstrap();
        Task<LeagueStanding> GetLeagueStanding(string leagueId);
        Task<TeamHistory> GetTeamHistory(string teamId);
    }
}