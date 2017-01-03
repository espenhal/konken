﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Models;
using HtmlAgilityPack;

namespace Common.Code
{
    class HtmlParser
    {
        public static League GetLeague(string html)
        {
            var htmlDoc = new HtmlAgilityPack.HtmlDocument
            {
                // There are various options, set as needed
                OptionFixNestedTags = true,
            };

            htmlDoc.LoadHtml(html);

            //if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Any()) return null;

            if (htmlDoc.DocumentNode == null) return null;

            var league = new League()
            {
                Name = "Konken"//htmlDoc.DocumentNode.SelectSingleNode("//*[@id=\"ismr-main\"]/div/h2").InnerText.Trim()
            };

            var playerNodes =
                htmlDoc.DocumentNode.SelectNodes("/tbody/tr");

            if (playerNodes == null) return null;

            var players = new List<Player>();

            foreach (var playerNode in playerNodes)
            {
#if DEBUG
                //if (players.Count == 2) continue;
#endif
                HtmlNode[] n = playerNode.ChildNodes.Where(x => x.Name == "td").ToArray();

                var name = n[1].ChildNodes[1].ChildNodes[3].InnerText.Trim();
                var teamName = n[1].ChildNodes[1].ChildNodes[1].InnerText.Trim();
                var fplPlayerId = GetFplPlayerId(n);

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(teamName) || string.IsNullOrEmpty(fplPlayerId)) continue;

                var player = new Player()
                {
                    Name = name,
                    TeamName = teamName,
                    FplPlayerId = fplPlayerId,
                    //Gameweeks = GetNewGameWeekHistory(fplPlayerId)
                };

                players.Add(player);
            }

            league.Players = players;

            return league;
        }

        private static string GetFplPlayerId(HtmlNode[] n)
        {
            var playerLink = n[1].ChildNodes.FirstOrDefault(
                x =>
                    x.Name == "a" && x.HasAttributes &&
                    !string.IsNullOrEmpty(x.GetAttributeValue("href", string.Empty)))?
                .Attributes.FirstOrDefault(x => x.Name.Equals("href"))?.Value;

            return playerLink?.Substring(playerLink.LastIndexOf("/", StringComparison.Ordinal) + 1);
        }

        public static List<Gameweek> GetGameweeks(string html)
        {
            var htmlDoc = new HtmlAgilityPack.HtmlDocument
            {
                // There are various options, set as needed
                OptionFixNestedTags = true
            };

            htmlDoc.LoadHtml(html);

            if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Any()) return null;

            var nodes =
                htmlDoc.DocumentNode.SelectNodes("/tbody/tr");

            if (nodes == null) return null;

            var gameweeks = new List<Gameweek>();

            foreach (var node in nodes)
            {
                var n = node.ChildNodes.Where(x => x.Name == "td").ToArray();

                var gw = new Gameweek()
                {
                    Number = Convert.ToInt32(n[0].InnerText.Trim().Replace("GW", "")),
                    Points = Convert.ToInt32(n[1].InnerText.Trim()),
                    PointsOnBench = Convert.ToInt32(n[2].InnerText.Trim()),
                    GameweekRank = Convert.ToInt32(n[3].InnerText.Trim().Replace(",", "")),
                    Transfers = Convert.ToInt32(n[4].InnerText.Trim()),
                    TransferCosts = Convert.ToInt32(n[5].InnerText.Trim()),
                    OverallPoints = Convert.ToInt32(n[6].InnerText.Trim().Replace(",", "")),
                    OverallRank = Convert.ToInt32(n[7].InnerText.Trim().Replace(",", "")),
                    Value = Convert.ToDouble(n[8].InnerText.Trim().Replace("£", "").Replace(".", ",")),
                    Chip = Chip.None
                };

                gameweeks.Add(gw);
            }

            return gameweeks;
        }

        public static void GetGameweekChip(Player player, string html)
        {
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

            // There are various options, set as needed
            htmlDoc.OptionFixNestedTags = true;

            // filePath is a path to a file containing the html
            htmlDoc.LoadHtml(html);

            if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Any()) return;

            var nodes =
                htmlDoc.DocumentNode.SelectNodes("/tbody/tr");

            if (nodes == null) return;

            foreach (var node in nodes)
            {
                var n = node.ChildNodes.Where(x => x.Name == "td").ToArray();

                var gameweekNumber = Convert.ToInt32(n[3].InnerText.Trim().Replace("GW", "")) - 1;

                switch (n[1].InnerText.Trim())
                {
                    case "Wildcard":
                        player.Gameweeks[gameweekNumber].Chip = Chip.Wildcard;
                        break;
                    case "Bench Boost":
                        player.Gameweeks[gameweekNumber].Chip = Chip.BenchBoost;
                        break;
                    case "All Out Attack":
                        player.Gameweeks[gameweekNumber].Chip = Chip.AllOutAttack;
                        break;
                    case "Triple Captain":
                        player.Gameweeks[gameweekNumber].Chip = Chip.TripleCaptain;
                        break;
                }
            }
        }
        public static void GetCupHistory(Player player, string html)
        {
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

            // There are various options, set as needed
            htmlDoc.OptionFixNestedTags = true;

            // filePath is a path to a file containing the html
            htmlDoc.LoadHtml(html);

            if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Any()) return;

            var nodes =
                htmlDoc.DocumentNode.SelectNodes("/tbody/tr");

            if (nodes == null) return;
            
            foreach (var node in nodes)
            {
                var n = node.ChildNodes.Where(x => x.Name == "td").ToArray();

                var gameweekNumber = Convert.ToInt32(n[0].InnerText.Trim().Replace(" ", "")) - 1;

                player.Gameweeks[gameweekNumber].Cup = true;
            }
        }
    }
}
