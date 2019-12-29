using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using web;

namespace Services
{
    public class FplApiWrapper : IFplApiWrapper
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public FplApiWrapper(IHttpClientFactory httpClientFactory, string settingsLeagueId)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> GetBootstrap()
        {
            var url = "bootstrap-static/";
            
            using (var client = _httpClientFactory.CreateClient(HttpClients.FplApi))
            {
                using (var response = await client.GetAsync(url))
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        public async Task<string> GetLeague(string leagueId)
        {
            var url = $"leagues-classic/{leagueId}/standings";
            
            using (var client = _httpClientFactory.CreateClient(HttpClients.FplApi))
            {
                using (var response = await client.GetAsync(url))
                {
                    return await response.Content.ReadAsStringAsync();
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