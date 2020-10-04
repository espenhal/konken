using System.Collections.Generic;

namespace web.Models.View
{
    public class League
    {
        public string FplLeagueId { get; set; }
        public string Name { get; set; }
        public List<Player> Players { get; set; }
    }

    public class Player
    {
        public string FplPlayerId { get; set; }
        public string Name { get; set; }
        public string TeamName { get; set; }
        public List<Gameweek> Gameweeks { get; set; }
    }

    public class Gameweek
    {
        public int? Number { get; set; }
        public int? Points { get; set; }
        public int? PointsOnBench { get; set; }
        public int? OverallPoints { get; set; }
        public int? Transfers { get; set; }
        public int? TransferCosts { get; set; }
        public string Chip { get; set; }
        public double? Value { get; set; }
        public int? GameweekRank { get; set; }
        public int? OverallRank { get; set; }
        public int? PointsExcludedTransferCosts => Points - TransferCosts;
        public int? ScoredGoals { get; set; }
        public Cup Cup { get; set; }
    }

    public class Cup
    {
        public int? GameweekNumber { get; set; }
        public string HomeFplPlayerId { get; set; }
        public string HomeName { get; set; }
        public string HomeTeamName { get; set; }
        public int? HomePoints { get; set; }
        public string AwayFplPlayerId { get; set; }
        public string AwayName { get; set; }
        public string AwayTeamName { get; set; }
        public int? AwayPoints { get; set; }
        public int? Winner { get; set; }
    }
    
    public class PlayerStanding
    {
        public string FplPlayerId { get; set; }
        public string Name { get; set; }
        public string TeamName { get; set; }
        public int? Points { get; set; }
        public int? PointsOnBench { get; set; }
        public int? Transfers { get; set; }
        public int? TransferCosts { get; set; }
        public int? PointsTransferCostsExcluded { get; set; }
        public List<string> Chips { get; set; }
        public string Value { get; set; }
        public int? Rank { get; set; }
        public double? Cash { get; set; }
        public List<int?> GameweeksWon { get; set; }
        public int? CupRounds { get; set; }
    }

    public class GameweekWinner
    {
        public int? Number { get; set; }
        public string FplPlayerId { get; set; }
        public int? PointsExcludedTransferCosts { get; set; }
        public int? PointsOnBench { get; set; }
        public int? ScoredGoals { get; set; }
    }
}