using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using common;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Polly;
using web;

namespace scraper
{
    class Program
    {
        public static string FplLeagueId = "765020";

        private static CloudTable Table { get; set; }

        private static readonly Policy Policy = Policy
            .Handle<Exception>()
            .WaitAndRetry(5, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
            );

        static void Main(string[] args)
        {
            Console.WriteLine("start.");

            // Parse the connection string and return a reference to the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString")
            );

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            // Retrieve a reference to the table.
            Table = tableClient.GetTableReference(ConfigurationManager.AppSettings["tablestoragecontainer"]
#if DEBUG
                + "test"
#endif
            );
            // Create the table if it doesn't exist.
            Table.CreateIfNotExists();

            AutoMapperConfiguration.Configure();

            UpdateLeague(ParseHtml());

            Console.WriteLine("slutt. klikk.");

            Console.ReadLine();
        }

        private static League ParseHtml()
        {
            var leagueHtml = Policy.Execute(() =>
                Scraper.GetHtmlByXPath(
                    $"https://fantasy.premierleague.com/a/leagues/standings/{FplLeagueId}/classic",
                    "//*[@id=\"ismr-classic-standings\"]/div/div/table/tbody"));

            League league = HtmlParser.GetLeague(leagueHtml);
            league.FplLeagueId = FplLeagueId;

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

            return league;
        }

        private static void UpdateLeague(League league)
        {
            // League
            LeagueEntity leagueEntity = AutoMapper.Mapper.Map<League, LeagueEntity>(league);
            leagueEntity.PartitionKey = "League";
            leagueEntity.RowKey = league.FplLeagueId;

            TableOperation leagueInsertOrReplaceOperation = TableOperation.InsertOrReplace(leagueEntity);

            TableResult leagueInsertOrReplaceResult = Table.Execute(leagueInsertOrReplaceOperation);

            // Players & Gameweeks
            TableBatchOperation playerBatchInsertOrReplaceOperation = new TableBatchOperation();

            foreach (var player in league.Players)
            {
                PlayerEntity playerEntity = AutoMapper.Mapper.Map<Player, PlayerEntity>(player);
                playerEntity.PartitionKey = "Player";
                playerEntity.RowKey = league.FplLeagueId + "|" + player.FplPlayerId;

                playerBatchInsertOrReplaceOperation.InsertOrReplace(playerEntity);

                TableBatchOperation gameweekBatchInsertOrReplaceOperation = new TableBatchOperation();

                foreach (var gameweek in player.Gameweeks)
                {
                    GameweekEntity gameweekEntity = AutoMapper.Mapper.Map<Gameweek, GameweekEntity>(gameweek);
                    gameweekEntity.PartitionKey = "Gameweek";
                    gameweekEntity.RowKey = league.FplLeagueId + "|" + player.FplPlayerId + "|" + gameweek.Number;
                    gameweekEntity.FplPlayerId = player.FplPlayerId;
                    gameweekEntity.FplLeagueId = league.FplLeagueId;

                    //gameweekEntity.HomeFplPlayerId = gameweek.Cup.HomeFplPlayerId;
                    //gameweekEntity.HomeName = gameweek.Cup.HomeName;
                    //gameweekEntity.HomeTeamName = gameweek.Cup.HomeTeamName;
                    //gameweekEntity.HomePoints = gameweek.Cup.HomePoints;

                    //gameweekEntity.AwayFplPlayerId = gameweek.Cup.AwayFplPlayerId;
                    //gameweekEntity.AwayName = gameweek.Cup.AwayName;
                    //gameweekEntity.AwayTeamName = gameweek.Cup.AwayTeamName;
                    //gameweekEntity.AwayPoints = gameweek.Cup.AwayPoints;

                    gameweekBatchInsertOrReplaceOperation.InsertOrReplace(gameweekEntity);
                }

                IList<TableResult> gameweekBatchInsertOrReplaceResult = Table.ExecuteBatch(gameweekBatchInsertOrReplaceOperation);
            }

            IList<TableResult> playerBatchInsertOrReplaceResult = Table.ExecuteBatch(playerBatchInsertOrReplaceOperation);
        }
    }
}
