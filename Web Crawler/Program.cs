using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Web_Crawler
{
    public class Program
    {
        private static CrawlerEngine crawler_engine = new CrawlerEngine(200);

        static void Main(string[] args)
        {
            var target = args.Length > 0 ? args[0] : "http://reddit.com";
            var mainTasks = new List<Task>();
            var crawlTask = crawler_engine.crawl(target);
            var inputTask = InputAsync();
            var consoleTask = ConsoleAsync();
            mainTasks.Add(crawlTask);
            mainTasks.Add(inputTask);
            mainTasks.Add(consoleTask);
            Task.WaitAll(mainTasks.ToArray());
        }

        private static async Task InputAsync()
        {
            await Task.Factory.StartNew(() =>
            {

                var key = Console.ReadKey().Key;
                while (key != ConsoleKey.Q)
                {
                    if (key == ConsoleKey.RightArrow)
                    {
                        crawler_engine.MaxConcurrency += 20;
                    }
                    else if (key == ConsoleKey.LeftArrow)
                    {
                        crawler_engine.MaxConcurrency += 20;
                    }
                    key = Console.ReadKey().Key;
                }
                Environment.Exit(0);
            });
        }

        private static async Task ConsoleAsync()
        {
            Console.Clear();
            while (true)
            {
                Console.Clear();
                //var lastCrawled = crawler_engine.last_crawled.Length > 50 ? crawler_engine.last_crawled : crawler_engine.last_crawled.Substring(0, 49);
                var lastCrawled = crawler_engine.last_crawled;

                Console.SetCursorPosition(0, 0);
                Console.WriteLine("Max Threads " + crawler_engine.MaxConcurrency);
                Console.WriteLine("Queued " + crawler_engine._urls.Count);
                Console.WriteLine("Crawled " + crawler_engine.numCrawled);
                Console.WriteLine("Tasks Queued " + crawler_engine.TaskCount);
               
                Console.WriteLine("-----------------------------------------------------------" + "\n\n");

                //while (crawler_engine.MessageQueue.Count > 0)
                //{
                //    var message = crawler_engine.MessageQueue.Pop();
                //    Console.WriteLine(message);
                //}
                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }
        }
    }
}