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

        private CrawlLogger crawllogger;
        public Stack<string> _urls = new Stack<string>();
        public long numCrawled;
        public int taskCount;
        private static List<Task<string>> _tasks = new List<Task<string>>();
        private int _maxConcurrency;

        public Stack<string> MessageQueue = new Stack<string>();
        public string last_crawled;

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
        
        public CrawlerEngine(int maxConcurrency = 200)
        {
            ServicePointManager.DefaultConnectionLimit = maxConcurrency;
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            MaxConcurrency = maxConcurrency;
            crawllogger = new CrawlLogger("crawlerlogger-log.txt");
            taskCount = 0;
            numCrawled = 0;
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


        private async Task<string> RequestSite(string target)
        {
            string responseString = "";
            history.Add(target, "true");
            HttpClient client = new HttpClient();
            try
            {      
                Task<HttpResponseMessage> response_contents = client.GetAsync(target);
                // Console.BackgroundColor = ConsoleColor.Blue;
                // Console.ForegroundColor = ConsoleColor.White;
                // Console.ResetColor();
                MessageQueue.Push(target);
                taskCount++;
                HttpResponseMessage response = await response_contents;
                crawllogger.AddLog(target);
                numCrawled++;
                taskCount--;

                try
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = response.Content;
                        responseString = responseContent.ReadAsStringAsync().Result;
                    }
                }
                catch (ArgumentException)
                {
                    responseString = "";
                }
            }
            catch (Exception ex)
            {
                MessageQueue.Push("An element with Key = " + target + " already exists.");
                responseString = "";
                System.Diagnostics.Debug.WriteLine("CAUGHT EXCEPTION:");
                System.Diagnostics.Debug.WriteLine(ex);
            }

            getLinks(responseString, target);
            return responseString;
        }

        private MatchCollection getLinks(string body, string target)
        {
            last_crawled = target;
            Regex re = new Regex("<a.*?href=\"(.*?)\"",
                    RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);

            MatchCollection matches = re.Matches(body);
            parseLinks(matches, target);
            return matches;
        }

        private void parseLinks(MatchCollection matches, string target)
        {
            Regex slash_re = new Regex("/{2,}",
                RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    string current_match;
                    string newTarget;
                    current_match = match.Groups[1].Value;

                    // Internal site match
                    if (current_match.FirstOrDefault() == '/')
                    {
                        newTarget = "http://" + slash_re.Replace((target + current_match).Replace("http://", ""), "/");
                        MessageQueue.Push(newTarget);   

                        if (!history.ContainsKey(newTarget))
                        {
                            //MessageQueue.Push("NEW: " + (newTarget));
                            _urls.Push(newTarget);
                        }
                    }

                    //external site
                    if (current_match.IndexOf("http") == 0 || current_match.IndexOf("https") == 0)
                    {
                        newTarget = current_match;
                        if (!history.ContainsKey(newTarget))
                        {
                            //MessageQueue.Push("NEW EXTERNAL: " + (newTarget));
                            _urls.Push(newTarget);
                        }
                    }
                }
            }
        }


    }
}



