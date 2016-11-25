using System;
using System.IO;
using Common.Code;
using Microsoft.Azure.WebJobs;
using HtmlScraper.Code;
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

        public static void ProcessQueueMessage([QueueTrigger("konkenQueue")] string message, TextWriter log)
        {
            log.WriteLine($"ProcessQueueMessage {message}");

            var leagueHtml =
                _policy.Execute(
                    () =>
                        Scraper.GetHtmlByXPath(
                            $"https://fantasy.premierleague.com/a/leagues/standings/{message}/classic", "//*[@id=\"ismr-classic-standings\"]/div/div/table/tbody"));
            
            log.WriteLine("League html fetched");

            var league = HtmlParser.GetLeague(leagueHtml);
            league.FplLeagueId = message;
            

        }

        //        public static void ProcessTimer([TimerTrigger("00:00:10", RunOnStartup = true)] TimerInfo info, [Queue("queue")] out string message, TextWriter log)
        //        {
        //            message = info.FormatNextOccurrences(1);

        //            log.WriteLine($"ProcessTimer {info.FormatNextOccurrences(1)}");
        //#if DEBUG
        //            Console.WriteLine($"ProcessTimer {info.FormatNextOccurrences(1)}");
        //#endif
        //        }

        //public static void ProcessStartupJob([TimerTrigger("00:00:05", RunOnStartup = true, UseMonitor = true)] TimerInfo timerInfo, TextWriter log)
        //{
        //    Console.WriteLine("Timer job fired!");

        //    //string scheduleStatus = string.Format("Status: Last='{0}', Next='{1}', IsPastDue={2}",
        //    //    timerInfo.ScheduleStatus.Last, timerInfo.ScheduleStatus.Next, timerInfo.IsPastDue);
        //    //Console.WriteLine(scheduleStatus);
        //}

        //public static void ProcessCronJob([TimerTrigger("00:00:15", RunOnStartup = true)] TimerInfo timerInfo, TextWriter log)
        //{
        //    Console.WriteLine("Timer job fired!");
        //}
    }
}
