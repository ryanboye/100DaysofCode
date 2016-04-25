﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Web_Crawler
{
    class Program
    {
        static Dictionary<string, string> targets = new Dictionary<string, string>();
        static Dictionary<string, string> history = new Dictionary<string, string>();
        private static readonly Regex slash_re = new Regex("/\\/{ 2,}",
                RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
        private static readonly Regex re = new Regex("<a.*?href=\"(.*?)\"",
                RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);

    static void Main()
        {
            // var target = Console.ReadLine();
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
        }

        static void RequestSite(string target)
        {
            var client = new HttpClient();
            var response = client.GetAsync(target).Result;

            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Scanning: " + target);
            Console.ResetColor();

            if (response.IsSuccessStatusCode)
            {
                // by calling .Result you are performing a synchronous call
                var responseContent = response.Content;

                // by calling .Result you are synchronously reading the result
                string responseString = responseContent.ReadAsStringAsync().Result;

                getLinks(responseString, target);
            }

            //Main();
            Console.ReadLine();
        }

        static void getLinks(string body, string target)
        {
            MatchCollection matches = re.Matches(body);

            foreach (Match match in matches)
            {
                string current_match = match.Groups[1].Value;
                string newTarget = target + current_match;
                Console.WriteLine("Match" + current_match);

                char first_letter = current_match[0];

                //Internal site match
                if (first_letter == '/')
                {
                    var newTarget = 'http://' + (target + match).replace('http://', '').replace(slash_re, '/');
                    // console.log((target + match).green);
                    // console.log(history);
                    if (history[newTarget] == null)
                    {
                        console.log('NEW: '.green + (newTarget));
                    }
                    else if (history[newTarget] == 1)
                    {
                        console.log('EXISTS: '.yellow + (newTarget));
                    }
                    if (history[newTarget] == null)
                    {
                        getSite(newTarget);
                    }
                }

                // external site yeeee
                if (match.indexOf('http://') == '0')
                {
                    var newTarget = match;
                    if (history[newTarget] == null)
                    {
                        console.log('NEW: '.green + (newTarget));
                    }
                    else if (history[newTarget] == 1)
                    {
                        console.log('EXISTS: '.yellow + (newTarget));
                    }
                    if (history[newTarget] == null)
                    {
                        getSite(newTarget);
                    }
                }
            }
            Console.ReadLine();
        }
    }
}



