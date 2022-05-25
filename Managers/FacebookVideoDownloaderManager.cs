using CefSharp.OffScreen;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YoutubeDownloaderChecker.Entities;
using YoutubeDownloaderChecker.Helpers;

namespace YoutubeDownloaderChecker.Managers
{
    class FacebookVideoDownloaderManager : FacebookVideoDownloaderManagerBase
    {
        const string facebookUrlBase = "https://www.facebook.com/search/videos/?q=";
        
        const string youtubeSearchQuery = "/watch?v=";
        const string facebookSearchQuery = @"/watch/live/?v=";
        const string facebookEndPatern = @"&";
        //const string facebookSearchQuery = @"watch\/?ref=search&v=";

        private static List<Process> processes = new List<Process>();

        private static string[] RequestQueries;
        private static ChromiumWebBrowser browser;

        public static void GetFacebookVideos()
        {
            List<Tuple<string, int, int>> watchUrlLocations = new List<Tuple<string, int, int>>();
            List<Tuple<string, string>> watchUrls = new List<Tuple<string, string>>();
            int lastPosition = 0;

            var config = ReadConfigHelper.ReadConfig("config.txt");

            var requestQueries = File.ReadAllLines(config.RequestDirPath);
            RequestQueries = requestQueries;

            //Logs into the facebook via ChromeDriver
            var chromeDriver = new ChromeDriver();
            LoginIntoFacebook(chromeDriver);

            foreach (string searchQuery in requestQueries)
            {
                var facebookHtmlString = GetFacebookHtmlAsync(chromeDriver, facebookUrlBase + searchQuery).Result;
                var videosLocationInString = GetWatchUrlsHelper.GetWatchUrlsFromString(facebookHtmlString, facebookSearchQuery, facebookEndPatern, ref lastPosition);
                //string queryHtmlString = GetResultHtml(searchQuery);
                //var newLocations = GetWatchUrlsFromString(queryHtmlString, ref lastPosition);
                foreach (var newLocation in videosLocationInString)
                {
                    //var newItem = new Tuple<string, int, int>(searchQuery, newLocation.Item2, newLocation.Item3);
                    //watchUrlLocations.Add(newItem);
                    string watchUrl = facebookHtmlString.Substring(newLocation.Item1, newLocation.Item2 - newLocation.Item1);
                    //string youtubeWatchUrl = "https://www.youtube.com" + watchUrl;
                    string facebookWatchUrl = "https://www.facebook.com" + watchUrl /*"&q=" + searchQuery*/;

                    //Console.WriteLine(youtubeWatchUrl);
                    watchUrls.Add(new Tuple<string, string>(searchQuery, facebookWatchUrl));
                }
            }
            Console.ReadKey();

            foreach (var watchUrl in watchUrls)
            {
                var process = StartYouTubeDLProcessHelper.StartYouTubeDLProcess(
                    watchUrl.Item1, watchUrl.Item2, config.YouTubeDLPath, config.SaveDirPathYoutube, config.DataDirPath);
                processes.Add(process);
            }

            List<string> files = new List<string>();

            foreach (var searchQueryString in requestQueries)
            {
                files.AddRange(FindFilesPerSeachQueryHelper.FindFilesPerSearchQuery(searchQueryString, config.SaveDirPathYoutube));
            }

            List<string> tagsList = new List<string>();
            List<string> categoriesList = new List<string>();

            foreach (var file in files)
            {
                try
                {
                    using (StreamReader r = new StreamReader(file))
                    {
                        Metadata metadata = MetadataJsonParserHelper.ParseJson(tagsList, categoriesList, r);

                        //Clean after one iteration
                        tagsList = new List<string>();
                        categoriesList = new List<string>();
                        UpdateOrCreateMetadataHelper.UpdateOrCreateMetadataFromJsonToLocalDB(metadata, config.DataDirPath);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("\n Json file cannot be read named: " + file + ". Ended with error " + e.Message);
                }

            }

            while (!processes.All(p => p.HasExited == true))
            {
                //Here it will wait until all process for downloading videos has ended.
                Task.Delay(10);
            }

            Console.WriteLine("\n All videos has been successfuly downloaded and checked ");
        }
    }
}
