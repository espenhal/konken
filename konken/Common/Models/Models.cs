using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Common.Models
{
    public class LeagueStanding
    {
        public string FplLeagueId { get; set; }
        public string Name { get; set; }
        public List<PlayerStanding> PlayerStandings { get; set; }
        public List<int> Rounds { get; set; }
    }

    public class PlayerStanding
    {
        public string FplPlayerId { get; set; }
        public string Name { get; set; }
        public string TeamName { get; set; }
        public int Points { get; set; }
        public int PointsOnBench { get; set; }
        public int Transfers { get; set; }
        public int TransferCosts { get; set; }
        public int PointsTransferCostsExcluded { get; set; }
        public List<Chip> Chips { get; set; }
        public double Value { get; set; }
        public int Rank { get; set; }
        public double Cash { get; set; }
        public List<int> GameweeksWon { get; set; }
        public int CupRounds { get; set; }
    }

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
        public int Number { get; set; }
        public int Points { get; set; }
        public int PointsOnBench { get; set; }
        public int OverallPoints { get; set; }
        public int Transfers { get; set; }
        public int TransferCosts { get; set; }
        public Chip Chip { get; set; }
        public double Value { get; set; }
        public int GameweekRank { get; set; }
        public int OverallRank { get; set; }
        public int PointsExcludedTransferCosts => Points - Transfers;
        public int ScoredGoals { get; set; }
        public Cup Cup { get; set; }
    }

    public class GameweekWinner
    {
        public int Number { get; set; }
        public string FplPlayerId { get; set; }
        public int PointsExcludedTransferCosts { get; set; }
        public int PointsOnBench { get; set; }
        public int ScoredGoals { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Chip
    {
        None = 0,
        Wildcard = 1,
        AllOutAttack = 2,
        BenchBoost = 3,
        TripleCaptain = 4,
        Wildcard2 = 5
    }

    public class UpdateLeagueMessage
    {
        public string FplLeagueId { get; set; }
        
        public int Failures { get; set; }
    }

    public class Cup
    {
        public int GamewekkNumber { get; set; }
        public string HomeFplPlayerId { get; set; }
        public string HomeName { get; set; }
        public string HomeTeamName { get; set; }
        public int? HomePoints { get; set; }
        public string AwayFplPlayerId { get; set; }
        public string AwayName { get; set; }
        public string AwayTeamName { get; set; }
        public int? AwayPoints { get; set; }
    }
}
