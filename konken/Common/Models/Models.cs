using System;
using System.Collections.Generic;

namespace Common.Models
{
    public class LeagueStanding
    {
        public string FplLeagueId { get; set; }
        public string Name { get; set; }
        public IList<PlayerStanding> PlayerStandings { get; set; }
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
        public IList<Chip> Chips { get; set; }
        public double Value { get; set; }
        public int Rank { get; set; }
        public int Cash { get; set; }
        public IList<int> Rounds { get; set; }
    }

    public class League
    {
        public string FplLeagueId { get; set; }
        public string Name { get; set; }
        public IList<Player> Players { get; set; }
    }

    public class Player
    {
        public string FplPlayerId { get; set; }
        public string Name { get; set; }
        public string TeamName { get; set; }
        public IList<Gameweek> Gameweeks { get; set; }
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
    }

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
}
