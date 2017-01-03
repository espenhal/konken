using System;
using System.Configuration;
using System.IO;
using System.Linq;
using Common.Code;
using Common.Models;
using Microsoft.Azure.WebJobs;
using HtmlScraper.Code;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using Polly;

namespace HtmlScraper
{
    public class Functions
    {
        private static readonly Policy Policy = Policy
            .Handle<Exception>()
            .WaitAndRetry(5, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
            );
        
        public static async void GetLeague([QueueTrigger("konkenqueue")] string message, TextWriter log)
        {
            var update = JsonConvert.DeserializeObject<UpdateLeagueMessage>(message);

            try
            {
                var leagueHtml = Policy.Execute(() =>
                    Scraper.GetHtmlByXPath(
                        $"https://fantasy.premierleague.com/a/leagues/standings/{update.FplLeagueId}/classic",
                        "//*[@id=\"ismr-classic-standings\"]/div/div/table/tbody"));

                League league = HtmlParser.GetLeague(leagueHtml);
                league.FplLeagueId = update.FplLeagueId;

                foreach (var player in league.Players)
                {
                    var gameweekHtml = Policy.Execute(() =>
                        Scraper.GetHtmlByXPath(
                            $"https://fantasy.premierleague.com/a/entry/{player.FplPlayerId}/history",
                            "//*[@id=\"ismr-event-history\"]/div/div/div/table/tbody"));

                    league.Players.First(x => x.FplPlayerId == player.FplPlayerId).Gameweeks =
                        HtmlParser.GetGameweeks(gameweekHtml);
                }

                foreach (var player in league.Players)
                {
                    var gameweekChipsHtml = Policy.Execute(() =>
                        Scraper.GetHtmlByXPath(
                            $"https://fantasy.premierleague.com/a/entry/{player.FplPlayerId}/history",
                            "//*[@id=\"ismr-event-chips\"]/div/div/div/table/tbody"));

                    HtmlParser.GetGameweekChip(player, gameweekChipsHtml);
                }

                foreach (var player in league.Players)
                {
                    var cupHistoryHtml = Policy.Execute(() =>
                        Scraper.GetHtmlByXPath(
                            $"https://fantasy.premierleague.com/a/leagues/cup/{player.FplPlayerId}",
                            "//*[@id=\"ismr-cup-matches\"]/div/div/div/table/tbody", false));

                    if (!string.IsNullOrWhiteSpace(cupHistoryHtml))
                        HtmlParser.GetCupHistory(player, cupHistoryHtml);
                }

                var response = await HttpClientProxy.Post(
#if DEBUG
                        ConfigurationManager.AppSettings["apiurltest"]
#else
                        ConfigurationManager.AppSettings["apiurl"]
#endif
                        , "updateleague", JsonConvert.SerializeObject(league));
            }
            catch (Exception e)
            {
                update.Failures++;

                if (update.Failures >= 5) throw new Exception("Too many retries!");

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
#if DEBUG
                "UseDevelopmentStorage=true;"
#else
                CloudConfigurationManager.GetSetting("StorageConnectionString")
#endif
                );

                // Create the queue client
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                // Retrieve a reference to the queue
                CloudQueue queue = queueClient.GetQueueReference("konkenqueue");
                // Create the queue if it doesn't exist
                queue.CreateIfNotExists();

                await queue.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(new UpdateLeagueMessage { FplLeagueId = update.FplLeagueId, Failures = update.Failures })));
            }
        }
    }
}
