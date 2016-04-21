using System;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Web_Crawler
{
    class Program
    {
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

                getLinks(responseString);
            }

            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Scanning: " + target);
            Console.ResetColor();
          
            Main();
        }

        static void getLinks(string body)
        {
            
            Regex rx= new Regex("@(?=href=\"([^\"]*)\")[^>]*>([^<]*)<\\/a>",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

            MatchCollection matches = rx.Matches(body);

            foreach (Match match in matches)
            {
                GroupCollection groups = match.Groups;
                Console.WriteLine("'{0}' repeated at positions {1} and {2}",
                                  groups["word"].Value,
                                  groups[0].Index,
                                  groups[1].Index);
            }
        }
    }
}



