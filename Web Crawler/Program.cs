using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Web_Crawler
{
    class Program
    {
        static Dictionary<string, string> targets =
            new Dictionary<string, string>();

        static void Main()
        {
            var target = Console.ReadLine();

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
        }

        static void RequestSite(string target)
        {
            var client = new HttpClient();
            var response = client.GetAsync(target).Result;

            if (response.IsSuccessStatusCode)
            {
                // by calling .Result you are performing a synchronous call
                var responseContent = response.Content;

                // by calling .Result you are synchronously reading the result
                string responseString = responseContent.ReadAsStringAsync().Result;

                getLinks(responseString, target);
            }

            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Scanning: " + target);
            Console.ResetColor();
          
            Main();
        }

        static void getLinks(string body, string target)
        {
            
            Regex rx= new Regex("(?=href=\"([^ \"]*)\")[^>]*>[^<]*<\\/a>",
                RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);

            MatchCollection matches = rx.Matches(body);

            foreach (Match match in matches)
            {
                string newTarget = target + match.Groups[1].Value;
               
                try
                {
                    targets.Add(newTarget, newTarget);  //yea I'm sorry
                    RequestSite(newTarget);
                }
                catch (ArgumentException)
                {
                    Console.WriteLine("An element with Key = {0} already exists.", newTarget);
                }
            }
        }
    }
}



