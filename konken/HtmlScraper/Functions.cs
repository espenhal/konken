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
        private static readonly Policy _policy = Policy
            .Handle<Exception>()
            .WaitAndRetry(5, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
            );

        private static CloudQueue Queue { get; set; }

        public Functions()
        {
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
            Queue = queueClient.GetQueueReference("konkenqueue");
            // Create the queue if it doesn't exist
            Queue.CreateIfNotExists();
        }

        public static async void GetLeague([QueueTrigger("konkenqueue")] string message, TextWriter log)
        {
            var update = JsonConvert.DeserializeObject<UpdateLeagueMessage>(message);

            try
            {
                var leagueHtml = _policy.Execute(() =>
                    Scraper.GetHtmlByXPath(
                        $"https://fantasy.premierleague.com/a/leagues/standings/{update.FplLeagueId}/classic",
                        "//*[@id=\"ismr-classic-standings\"]/div/div/table/tbody"));

                League league = HtmlParser.GetLeague(leagueHtml);
                league.FplLeagueId = update.FplLeagueId;

                foreach (var player in league.Players)
                {
                    var gameweekHtml = _policy.Execute(() =>
                        Scraper.GetHtmlByXPath(
                            $"https://fantasy.premierleague.com/a/entry/{player.FplPlayerId}/history",
                            "//*[@id=\"ismr-event-history\"]/div/div/div/table/tbody"));

                    league.Players.First(X => X.FplPlayerId == player.FplPlayerId).Gameweeks =
                        HtmlParser.GetGameweeks(gameweekHtml);
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

                await Queue.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(new UpdateLeagueMessage { FplLeagueId = update.FplLeagueId, Failures = update.Failures })));
            }
        }
    }
}
