using System;
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
                HtmlNode[] n = playerNode.ChildNodes.Where(x => x.Name == "td").ToArray();

                var name = n[1].ChildNodes[1].ChildNodes[1].InnerText.Trim();
                var teamName = n[1].ChildNodes[1].ChildNodes[3].InnerText.Trim();
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

        public static IList<Gameweek> GetGameweeks(string html)
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
                    Value = Convert.ToDouble(n[6].InnerText.Trim().Replace("£", "")),
                    Chip = GetGameweekChip(html, n[0].InnerText.Trim().Replace("GW", ""))
                };

                gameweeks.Add(gw);
            }

            return gameweeks;
        }

        private static Chip GetGameweekChip(string html, string gameweekNumber)
        {
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

            // There are various options, set as needed
            htmlDoc.OptionFixNestedTags = true;

            // filePath is a path to a file containing the html
            htmlDoc.LoadHtml(html);

            if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Any()) return Chip.None;

            var nodes =
                htmlDoc.DocumentNode.SelectNodes("/tbody/tr");

            if (nodes == null) return Chip.None;

            foreach (var node in nodes)
            {
                var n = node.ChildNodes.Where(x => x.Name == "td").ToArray();

                if (n[3].InnerText.Trim().Replace("GW", "") == gameweekNumber)
                {
                    switch (n[1].InnerText.Trim())
                    {
                        case "Wildcard":
                            return Chip.Wildcard;
                        case "Bench Boost":
                            return Chip.BenchBoost;
                        case "All Out Attack":
                            return Chip.AllOutAttack;
                        case "Triple Captain":
                            return Chip.TripleCaptain;
                    }
                }
            }

            return Chip.None;
        }

        //private static IList<Chips> GetChipsHistory(string id)
        //{
        //    var html = GetHtml($"https://fantasy.premierleague.com/a/entry/{id}/history");

        //    HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

        //    // There are various options, set as needed
        //    htmlDoc.OptionFixNestedTags = true;

        //    // filePath is a path to a file containing the html
        //    htmlDoc.LoadHtml(html);

        //    if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Any()) return null;

        //    var nodes =
        //        htmlDoc.DocumentNode.SelectNodes("//*[@id=\"ismr-event-chips\"]/div/div/div/table/tbody/tr");

        //    if (nodes == null) return null;

        //    IList<Chips> chipses = new List<Chips>();

        //    foreach (var node in nodes)
        //    {
        //        var n = node.ChildNodes.Where(x => x.Name == "td").ToArray();

        //        switch (n[1].InnerText.Trim())
        //        {
        //            case "Wildcard":
        //                chipses.Add(Chips.AllOutAttack);
        //                break;
        //            case "Bench Boost":
        //                chipses.Add(Chips.AllOutAttack);
        //                break;
        //            case "All Out Attack":
        //                chipses.Add(Chips.AllOutAttack);
        //                break;
        //            case "Triple Captain":
        //                chipses.Add(Chips.AllOutAttack);
        //                break;
        //        }
        //    }

        //    return chipses;
        //}
    }
}
