using System;
using System.IO;
using System.Net;

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
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(target);
            var response = (HttpWebResponse)myReq.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            response.Close();

            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Scanning: " + target);
            Console.ResetColor();

            Console.WriteLine(responseString);
            Main();
        }
    }
}



