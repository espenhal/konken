using System;
using System.IO;
using Microsoft.Azure.WebJobs;

namespace HtmlScraper
{
    public class Functions
    {
        public static void ProcessQueueMessage([QueueTrigger("queue")] string message, TextWriter log)
        {
            log.WriteLine($"ProcessQueueMessage {message}");
#if DEBUG
            Console.WriteLine($"ProcessQueueMessage {message}");
#endif
        }

        public static void ProcessTimer([TimerTrigger("00:00:10", RunOnStartup = true)] TimerInfo info, [Queue("queue")] out string message, TextWriter log)
        {
            message = info.FormatNextOccurrences(1);

            log.WriteLine($"ProcessTimer {info.FormatNextOccurrences(1)}");
#if DEBUG
            Console.WriteLine($"ProcessTimer {info.FormatNextOccurrences(1)}");
#endif
        }

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
