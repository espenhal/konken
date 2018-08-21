using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using common;

namespace web.Models
{
    public class Gameweeks
    {
        public List<PlayerGameweek> LeagueGameweek { get; set; }
        public List<int> GameweekCount { get; set; }
        public int? GameweekSelected { get; set; }
    }
}