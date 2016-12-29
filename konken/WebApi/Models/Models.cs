using System;
using System.Collections.Generic;
using System.Threading;
using Common.Models;
using Microsoft.WindowsAzure.Storage.Table;

namespace WebApi.Models
{
    public class LeagueEntity : TableEntity
    {
        public LeagueEntity(string partitionKey, string rowKey) : base(partitionKey, rowKey)
        {
        }
        public LeagueEntity()
        {
        }
        public string FplLeagueId { get; set; }
        public string Name { get; set; }
    }

    public class PlayerEntity : TableEntity
    {
        public PlayerEntity(string partitionKey, string rowKey) : base(partitionKey, rowKey)
        {
        }
        public PlayerEntity()
        {
        }
        public string FplLeagueId { get; set; }
        public string FplPlayerId { get; set; }
        public string Name { get; set; }
        public string TeamName { get; set; }
    }

    public class GameweekEntity : TableEntity
    {
        public GameweekEntity(string partitionKey, string rowKey) : base(partitionKey, rowKey)
        {
        }
        public GameweekEntity()
        {
        }
        public string FplPlayerId { get; set; }
        public string FplLeagueId { get; set; }
        public int Number { get; set; }
        public int Points { get; set; }
        public int PointsOnBench { get; set; }
        public int OverallPoints { get; set; }
        public int Transfers { get; set; }
        public int TransferCosts { get; set; }
        public int ChipValue { get; set; }
        [IgnoreProperty]
        public Chip Chip
        {
            get { return (Chip) ChipValue; }
            set { ChipValue = (int) value; }
        }
        public double Value { get; set; }
        public int GameweekRank { get; set; }
        public int OverallRank { get; set; }
    }
}