using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Web_Crawler
{
    class CrawlerEngine
    {
        public static Dictionary<string, string> targets = new Dictionary<string, string>();
        public static Dictionary<string, string> history = new Dictionary<string, string>();

        private static CrawlLogger crawllogger;
        public static Stack<string> _urls = new Stack<string>();
        public static int taskCount;
        private static List<Task<string>> _tasks = new List<Task<string>>();
        private int _maxConcurrency;

        private static Stack<string> messageQueue = new Stack<string>();
        public Stack<string> MessageQueue
        {
            get
            {
                return messageQueue;
            }
        }
        private static string _lastCrawled;
        public string last_crawled
        {
            get
            {
                return _lastCrawled;
            }
        }

        public int MaxConcurrency
        {
            get
            {
                return _maxConcurrency;
            }
            set
            {
                ServicePointManager.DefaultConnectionLimit = value + 100;
                _maxConcurrency = value;
            }
        }
        public int TaskCount { get; set; }
        
        public CrawlerEngine(int maxConcurrency = 20)
        {
            MaxConcurrency = maxConcurrency;
            crawllogger = new CrawlLogger("crawlerlogger-log.txt");
            taskCount = 0;
        }

        public async Task<int> crawl(string target)
        {

            _urls.Push(target);
            while (_urls.Count > 0 || _tasks.Count > 0)
            {
                while (taskCount < MaxConcurrency && _urls.Count > 0)
                {
                    _tasks.Add(RequestSite(_urls.Pop()));
                }

                Task<string> t = await Task.WhenAny(_tasks);

                var completed = _tasks.Where(x => x.Status == TaskStatus.RanToCompletion).ToList();
                foreach (var task in completed)
                {
                    getLinks(task.Result, target);
                    _tasks.Remove(task);
                }
            }

            return history.Count();
        }


        static async Task<string> RequestSite(string target)
        {
            string responseString = "";
            try
            {
                history.Add(target, "true");
                HttpClient client = new HttpClient();
                Task<HttpResponseMessage> response_contents = client.GetAsync(target);

                // Console.BackgroundColor = ConsoleColor.Blue;
                // Console.ForegroundColor = ConsoleColor.White;
                // Console.ResetColor();
                messageQueue.Push("Scanning:" + target + "\n");
                taskCount++;
                HttpResponseMessage response = await response_contents;
                crawllogger.AddLog(target);
                taskCount--;

                try
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = response.Content;
                        responseString = responseContent.ReadAsStringAsync().Result;
                    }
                }
                catch (Exception ex)
                {
                    responseString = "";
                }
            }
            catch (ArgumentException)
            {
                messageQueue.Push("An element with Key = " + target + " already exists.");
                responseString = "";
            }

            getLinks(responseString, target);
            return responseString;

        }

        static void getLinks(string body, string target)
        {
            _lastCrawled = target;
            Regex slash_re = new Regex("/{2,}",
                RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
            Regex re = new Regex("<a.*?href=\"(.*?)\"",
                    RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);

            MatchCollection matches = re.Matches(body);

            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    var test = matches;
                    string current_match = "";
                    current_match = match.Groups[1].Value;

                    string newTarget = target + current_match;
                    //messageQueue.Push(match.Groups);



                    // Internal site match
                    if (current_match.FirstOrDefault() == '/')
                    {
                        newTarget = "http://" + slash_re.Replace((target + current_match).Replace("http://", ""), "/");

                        // Console.ForegroundColor = ConsoleColor.Green;
                        //messageQueue.Push(newTarget);
                        // Console.ResetColor();

                        if (!history.ContainsKey(newTarget))
                        {
                            // Console.ForegroundColor = ConsoleColor.Green;
                            //messageQueue.Push("NEW: " + (newTarget));
                            // Console.ResetColor();
                            _urls.Push(newTarget);
                        }
                        else if (history.ContainsKey(newTarget))
                        {
                            // Console.ForegroundColor = ConsoleColor.Yellow;
                            //messageQueue.Push("EXISTS: " + (newTarget));
                            // Console.ResetColor();
                        }

                    }

                    //external site

                    if (current_match.IndexOf("http") == 0 || current_match.IndexOf("https") == 0)
                    {
                        newTarget = current_match;
                        if (!history.ContainsKey(newTarget))
                        {
                            // Console.ForegroundColor = ConsoleColor.Green;
                            //messageQueue.Push("NEW EXTERNAL: " + (newTarget));
                            // Console.ResetColor();
                            _urls.Push(newTarget);
                        }
                        else if (history.ContainsKey(newTarget))
                        {
                            // Console.ForegroundColor = ConsoleColor.Yellow;
                            //messageQueue.Push("EXISTING EXTERNAL: " + (newTarget));
                            // Console.ResetColor();
                        }
                    }
                }
            }
        }
    }
}



