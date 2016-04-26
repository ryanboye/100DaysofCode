using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Web_Crawler
{
    class Program
    {
        static Dictionary<string, string> targets = new Dictionary<string, string>();
        static Dictionary<string, string> history = new Dictionary<string, string>();
        private static readonly Regex slash_re = new Regex("/{2,}",
                RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
        private static readonly Regex re = new Regex("<a.*?href=\"(.*?)\"",
                RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);

        static void Main()
        {

            //var target = Console.ReadLine();
            var target = "http://leftronic.com";

            if (target.IndexOf("http://") != -1)
            {
                RequestSite(target);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Please enter a valid URL");
                Console.ResetColor();

                Main();
            }
            Console.ReadLine();
        }

        static async void RequestSite(string target)
        {
            HttpClient client = new HttpClient();
            Task<HttpResponseMessage> response_contents = client.GetAsync(target);

            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Scanning:");
            Console.ResetColor();
            Console.Write(" " + target + "\n");

            HttpResponseMessage response = await response_contents;

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    history.Add(target, "true");
                }
                catch (ArgumentException)
                {
                    Console.WriteLine("An element with Key = {0} already exists.", target);
                }
                // by calling .Result you are performing a synchronous call
                var responseContent = response.Content;

                // by calling .Result you are synchronously reading the result
                string responseString = responseContent.ReadAsStringAsync().Result;

                getLinks(responseString, target);
            }
        }

        static void getLinks(string body, string target)
        {
            MatchCollection matches = re.Matches(body);

            if(matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    var test = matches;
                    string current_match = "";  
                    current_match = match.Groups[1].Value;                   

                    string newTarget = target + current_match;
                    //Console.WriteLine(match.Groups);

   

                    ////Internal site match
                    //if (first_letter == '/')
                    //{
                    //    newTarget = "http://" + slash_re.Replace((target + current_match).Replace("http://", ""), "/");

                    //    Console.ForegroundColor = ConsoleColor.Green;
                    //    //Console.WriteLine(newTarget);
                    //    Console.ResetColor();

                    //    if (!history.ContainsKey(newTarget))
                    //    {
                    //        Console.ForegroundColor = ConsoleColor.Green;
                    //        Console.WriteLine("NEW: " + (newTarget));
                    //        Console.ResetColor();
                    //        RequestSite(newTarget);
                    //    }
                    //    else if (history.ContainsKey(newTarget))
                    //    {
                    //        Console.ForegroundColor = ConsoleColor.Yellow;
                    //        Console.WriteLine("EXISTS: " + (newTarget));
                    //        Console.ResetColor();
                    //    }

                    //}

                    // external site 

                    if (current_match.IndexOf("http") == 0)
                    {
                        newTarget = current_match;
                        if (!history.ContainsKey(newTarget))
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("NEW EXTERNAL: " + (newTarget));
                            Console.ResetColor();
                            RequestSite(newTarget);
                        }
                        else if (history.ContainsKey(newTarget))
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("EXISTING EXTERNAL: " + (newTarget));
                            Console.ResetColor();
                        }
                    }
                }
            }
            

        }
    }
}



