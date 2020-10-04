using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://fantasy.premierleague.com/api/");

                using (var response = await client.GetAsync("bootstrap-static/"))
                {
                    var content = await response.Content.ReadAsStringAsync();

                    if (content == null) return null;

                    return JsonConvert.DeserializeObject<Bootstrap>(content);
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
                        var content = await response.Content.ReadAsStringAsync();

                        if (content == null) return null;

                        return JsonConvert.DeserializeObject<LeagueStanding>(content);
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
                        var content = await response.Content.ReadAsStringAsync();

                        if (content == null) return null;

                        return JsonConvert.DeserializeObject<TeamHistory>(content);
                    }
                }
            }
        }

        public async Task<TeamEntry> GetTeam(string teamId)
        {
            await Login();

            using (var handler = new HttpClientHandler() {CookieContainer = _cookieContainer})
            {
                using (var client = new HttpClient(handler))
                {
                    client.BaseAddress = new Uri("https://fantasy.premierleague.com/api/");

                    using (var response = await client.GetAsync($"entry/{teamId}/"))
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        if (content == null) return null;

                        return JsonConvert.DeserializeObject<TeamEntry>(content);
                    }
                }
            }
        }

        public async Task<Cup> GetCup(string teamId)
        {
            await Login();

            using (var handler = new HttpClientHandler() {CookieContainer = _cookieContainer})
            {
                using (var client = new HttpClient(handler))
                {
                    client.BaseAddress = new Uri("https://fantasy.premierleague.com/api/");

                    using (var response = await client.GetAsync($"entry/{teamId}/cup/"))
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        if (content == null) return null;

                        return JsonConvert.DeserializeObject<Cup>(content);
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
        Task<TeamEntry> GetTeam(string teamId);
        Task<Cup> GetCup(string teamId);
    }
}