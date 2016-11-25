using System;
using System.IO;
using Microsoft.Azure;
using Microsoft.Azure.WebJobs;

namespace HtmlScraper
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            SetupWatcher();

            var config = new JobHostConfiguration(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            
            //config.UseTimers();
#if DEBUG
            //config.Tracing.ConsoleLevel = TraceLevel.Info;
            //config.Queues.MaxPollingInterval = TimeSpan.FromSeconds(5);
            //config.Singleton.ListenerLockPeriod = TimeSpan.FromSeconds(15);
            config.UseDevelopmentSettings();
#endif

            var host = new JobHost(config);
            // The following code ensures that the WebJob will be running continuously
            host.RunAndBlock();
        }

        private static bool _running = true;
        private static string _shutdownFile;

        private static void SetupWatcher()
        {
            if (!string.IsNullOrWhiteSpace(_shutdownFile) && Directory.Exists(_shutdownFile))
            {
                // Get the shutdown file path from the environment
                _shutdownFile = Environment.GetEnvironmentVariable("WEBJOBS_SHUTDOWN_FILE");

                // Setup a file system watcher on that file's directory to know when the file is created
                var fileSystemWatcher = new FileSystemWatcher(Path.GetDirectoryName(_shutdownFile));
                fileSystemWatcher.Created += OnChanged;
                fileSystemWatcher.Changed += OnChanged;
                fileSystemWatcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastWrite;
                fileSystemWatcher.IncludeSubdirectories = false;
                fileSystemWatcher.EnableRaisingEvents = true;
            }

            //// Run as long as we didn't get a shutdown notification
            //while (_running)
            //{
            //    // Here is my actual work
            //    Console.WriteLine("Running and waiting " + DateTime.UtcNow);
            //    Thread.Sleep(1000);
            //}

            //Console.WriteLine("Stopped " + DateTime.UtcNow);
        }

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath.IndexOf(Path.GetFileName(_shutdownFile), StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // Found the file mark this WebJob as finished
                _running = false;
            }
        }
    }
}
