using System;
using System.Collections.Generic;
using SearchConsole.Core;

namespace SearchConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //https://console.developers.google.com/apis/credentials
            SearchConsole.Core.SearchConsole s = new SearchConsole.Core.SearchConsole(
                @"D:\client_id.json",
                "aprimikirio",
                "GoogleWebMasters.Auth.Store",
                "SearchConsoleProject",
                "https://example.com/");

                List<QuerryResponce> results = new List<QuerryResponce>();

                results = s.RequestForSPC(new DateTime(1997, 3, 24), null);

            foreach (QuerryResponce result in results)
            {
                Console.WriteLine("Text: " + result.Text);
                Console.WriteLine("URL: " + result.URL);
                Console.WriteLine("Country: " + result.Country);
                Console.WriteLine("Device: " + result.Device);
                Console.WriteLine("Clicks: " + result.Clicks);
                Console.WriteLine("Impressions: " + result.Impressions);
                Console.WriteLine("CTR: " + result.CTR);
                Console.WriteLine("Position: " + result.Position);
                Console.WriteLine("Date: " + result.Date + "\n");
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
