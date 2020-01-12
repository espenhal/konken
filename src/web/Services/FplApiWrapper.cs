using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using web;

namespace Services
{
    public class FplApiWrapper : IFplApiWrapper
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private CookieContainer _cookieContainer = new CookieContainer();

        public FplApiWrapper(IHttpClientFactory httpClientFactory, string settingsLeagueId)
        {
            _httpClientFactory = httpClientFactory;
        }

        private async Task Login()
        {
            using (var handler = new HttpClientHandler() {CookieContainer = _cookieContainer})
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
                        new KeyValuePair<string, string>("redirect_uri", "https://fantasy.premierleague.com/a/login"),
                        new KeyValuePair<string, string>("app", "plfpl-web")
                    });

                    using (var response = await client.PostAsync("", httpContent))
                    {
                        var result = await response.Content.ReadAsStringAsync();

                        CookieCollection cookies =
                            handler.CookieContainer.GetCookies(new Uri("https://users.premierleague.com"));

                        _cookieContainer = handler.CookieContainer;
                    }
                }
            }
        }

        public async Task<string> GetBootstrap()
        {
            using (var handler = new HttpClientHandler() {CookieContainer = _cookieContainer})
            {
                using (var client = new HttpClient(handler))
                {
                    client.BaseAddress = new Uri("https://fantasy.premierleague.com/api/");

                    using (var response = await client.GetAsync("bootstrap-static"))
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                }
            }
        }

        public async Task<string> GetLeague(string leagueId)
        {
            await Login();

            using (var handler = new HttpClientHandler() {CookieContainer = _cookieContainer})
            {
                using (var client = new HttpClient(handler))
                {
                    client.BaseAddress = new Uri("https://fantasy.premierleague.com/api/");

                    using (var response = await client.GetAsync($"leagues-classic/{leagueId}/standings"))
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                }
            }
        }
    }

    public interface IFplApiWrapper
    {
        Task<string> GetBootstrap();
        Task<string> GetLeague(string leagueId);
    }
}