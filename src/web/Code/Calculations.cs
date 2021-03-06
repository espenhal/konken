using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using web.Models.View;

namespace web.Code
{
    public static class Calculations
    {
        private const double GameweekWinnerSum = 25;
        private const double WinnerSum = 212.5;
        private const double SecondPlaceSum = 62.5;
        private const double MostValuableWinnerSum = 62.5;
        private const double LongestRunInCupSum = 62.5;
        
        public static List<PlayerStanding> CalculateLeagueStandings(League league)
        {
            List<PlayerStanding> playerStandings = new List<PlayerStanding>();
            
            foreach (var player in league.Players)
            {
                PlayerStanding playerStanding = new PlayerStanding()
                {
                    FplPlayerId = player.FplPlayerId,
                    Name = player.Name,
                    TeamName = player.TeamName,
                    Points = player.Gameweeks.OrderBy(x => x.Number).Last()
                        .OverallPoints, // total uten fratrekk for bytter
                    PointsOnBench = player.Gameweeks.Sum(x => x.PointsOnBench),
                    Transfers = player.Gameweeks.Sum(x => x.Transfers),
                    TransferCosts = player.Gameweeks.Sum(x => x.TransferCosts),
                    PointsTransferCostsExcluded = player.Gameweeks.Sum(x => x.Points), // total med fratrekk for bytter
                    Chips = player.Gameweeks.Select(x => x.Chip).ToList(),
                    Value = player.Gameweeks.OrderBy(x => x.Number).Last().Value.GetValueOrDefault(),
                    Rank = player.Gameweeks.OrderBy(x => x.Number).Last().OverallRank,
                    Cash = CalculateGameweekWinnerCash(player, league),
                    GameweeksWon = CalculatePlayerGameweekWinners(player, league),
                    CupRounds = player.Gameweeks.Count(x => x.Cup != null)
                };
                
                if (playerStanding.Cash != 0)
                    playerStanding.Winnings.Add($"Seiere: {playerStanding.Cash}");

                playerStandings.Add(playerStanding);
            }

            playerStandings = playerStandings.OrderBy(x => x.Points).ToList();
            
            CalculateCupCash(playerStandings, league);
            CalculateEndOfSeasonCash(playerStandings, league);
            CalculateMostValuableCash(playerStandings, league);

            foreach (var playerStanding in playerStandings)
            {
                playerStanding.Winnings.Add($"Total: {playerStanding.Cash}");                
            }
            
            return playerStandings;
        }

        public static string ConvertValueToDouble(double? val)
        {
            if (val == null) return null;
            
            var value = val.Value;
            
            var str = value.ToString(CultureInfo.InvariantCulture);
            try
            {
                return double.Parse(str.Insert(str.Length - 1, ".")).ToString(CultureInfo.InvariantCulture);
            }
            catch
            {
                try
                {
                    return double.Parse(str.Insert(str.Length - 1, ",")).ToString(CultureInfo.InvariantCulture);
                }
                catch
                {
                    return value.ToString(CultureInfo.InvariantCulture);   
                }
            }
        }
        
        private static double CalculateGameweekWinnerCash(Player player, League league)
        {
            return (league.Players.Count * GameweekWinnerSum) * CalculatePlayerGameweekWinners(player, league).Count;
        }
        
        private static List<int?> CalculatePlayerGameweekWinners(Player player, League league)
        {
            return CalculateGameweekWinners(league).Where(x => x.FplPlayerId == player.FplPlayerId)
                .Select(x => x.Number).ToList();
        }
        
        private static List<GameweekWinner> CalculateGameweekWinners(League league)
        {
            List<GameweekWinner> gameweekWinners = new List<GameweekWinner>();

            var numberOfRounds = league.Players.FirstOrDefault()?.Gameweeks.Count;

            if (numberOfRounds == null)
                return null;

            for (var i = 1; i <= numberOfRounds.Value; i++)
            {
                GameweekWinner gameweekWinner = new GameweekWinner() {Number = i};

                foreach (var player in league.Players)
                {
                    if (player?.Gameweeks == null || player.Gameweeks.Count < 1)
                        continue;

                    var better = false;

                    if (string.IsNullOrEmpty(gameweekWinner.FplPlayerId))
                    {
                        better = true;
                    }
                    else
                    {
                        var gameweek = player.Gameweeks.FirstOrDefault(x => x.Number == i);

                        if (gameweek == null)
                            continue;

                        if (gameweek.PointsExcludedTransferCosts > gameweekWinner.PointsExcludedTransferCosts)
                        {
                            better = true;
                        }
                        else
                        {
                            if (gameweek.PointsExcludedTransferCosts == gameweekWinner.PointsExcludedTransferCosts)
                            {
                                if (gameweek.PointsOnBench > gameweekWinner.PointsOnBench)
                                    better = true;
                                else
                                {
                                    if (gameweek.PointsOnBench == gameweekWinner.PointsOnBench)
                                    {
                                        if (gameweek.ScoredGoals > gameweekWinner.ScoredGoals)
                                            better = true;
                                        else
                                        {
                                            // poengdeling!
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (better)
                    {
                        var fplPlayerId = player.FplPlayerId;
                        var pointsExcludedTransferCosts =
                            player.Gameweeks.FirstOrDefault(x => x.Number == i)?.PointsExcludedTransferCosts;
                        var pointsOnBench = player.Gameweeks.FirstOrDefault(x => x.Number == i)?.PointsOnBench;

                        if (string.IsNullOrEmpty(fplPlayerId) || pointsExcludedTransferCosts == null ||
                            pointsOnBench == null)
                            continue;

                        gameweekWinner.FplPlayerId = fplPlayerId;
                        gameweekWinner.PointsExcludedTransferCosts = pointsExcludedTransferCosts.Value;
                        gameweekWinner.PointsOnBench = pointsOnBench.Value;
                        gameweekWinner.ScoredGoals = 0;
                    }
                }

                gameweekWinners.Add(gameweekWinner);
            }

            return gameweekWinners;
        }

        private static void CalculateCupCash(List<PlayerStanding> playerStandings, League league)
        {
            Dictionary<string, int> playerCupAppearances = new Dictionary<string, int>();

            foreach (var player in league.Players)
            {
                playerCupAppearances.Add(player.FplPlayerId, player.Gameweeks.Count(x => x.Cup != null));
            }

            int max = playerCupAppearances.Max(x => x.Value);

            if (max == 0)
                return;

            var playersFurthestInCup = playerCupAppearances.Where(x => x.Value == max).Select(x => x.Key).ToList();

            foreach (var player in playersFurthestInCup)
            {
                var p = playerStandings.FirstOrDefault(x => x.FplPlayerId == player);

                if (p == null) return;

                var sum = league.Players.Count * LongestRunInCupSum / playersFurthestInCup.Count;
                
                p.Cash += sum;
                
                p.Winnings.Add($"Cup: {sum}");
            }
        }
        
        private static void CalculateEndOfSeasonCash(List<PlayerStanding> playerStandings, League league)
        {
            if (league.Players.First().Gameweeks.Count >= 38) {
                var winner = playerStandings.Last();

                var sum = league.Players.Count * WinnerSum;
                
                winner.Cash += sum;
                
                winner.Winnings.Add($"1.pl: {sum}");

                var second = playerStandings.Last(x => x != winner);
                
                var sumtwo = league.Players.Count * SecondPlaceSum;
                
                second.Cash += sumtwo;
                
                second.Winnings.Add($"2.pl: {sumtwo}");
            }
        }
        
        private static void CalculateMostValuableCash(IReadOnlyCollection<PlayerStanding> playerStandings, League league)
        {
            if (league.Players.First().Gameweeks.Count < 38) return;

            var max = playerStandings.Max(x => x.Value);

            var mostValuableSquads = playerStandings.Where(x => x.Value == max).ToList();

            foreach (var player in mostValuableSquads)
            {
                var p = playerStandings.First(x => x.FplPlayerId == player.FplPlayerId);

                var sum = (league.Players.Count * MostValuableWinnerSum) / mostValuableSquads.Count;
                
                p.Cash += sum;
                
                p.Winnings.Add($"Lagverdi: {sum}");
            }
        }
    }
}