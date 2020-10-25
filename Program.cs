using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace YoutubeDownloaderChecker
{
    class Program
    {
        const string youTubeQueryBase = "https://www.youtube.com/results?search_query=";
        private static string saveDirPathYoutube = string.Empty;
        private static string youTubeDLPath = string.Empty;
        private static string requestDirPath = string.Empty;
        private static String resultString = string.Empty;

        public static string YouTubeDLPath { get => youTubeDLPath; set => youTubeDLPath = value; }
        public static string SaveDirPathYoutube { get => saveDirPathYoutube; set => saveDirPathYoutube = value; }
        public static string RequestDirPath { get => requestDirPath; set => requestDirPath = value; }
        public static string ResultString { get => resultString; set => resultString = value; }

        static void Main(string[] args)
        {
            bool endOfString = false;
            List<Tuple<int, int>> watchUrlLocations = new List<Tuple<int, int>>();
            int lastPosition = 0;

            readConfig();

            var lines = System.IO.File.ReadAllLines(requestDirPath);

            foreach (string searchQuery in lines) 
            {
                ResultString += GetResultHtml(searchQuery);

                GetWatchUrlsFromString(ref endOfString, ResultString, watchUrlLocations, ref lastPosition);
            }

            List<string> watchUrls = new List<string>();

            foreach (var watchUrlLocation in watchUrlLocations)
            {
                string watchUrl = ResultString.Substring(watchUrlLocation.Item1, watchUrlLocation.Item2 - watchUrlLocation.Item1);
                string youtubeWatchUrl = "https://www.youtube.com/" + watchUrl;
                string watchUrlInTitle = watchUrl.Replace('?', '-');

                Console.WriteLine(youtubeWatchUrl);

                var files = Directory.GetFiles(SaveDirPathYoutube).ToList();
                if (files.Any(p => !p.Contains(watchUrlInTitle)))
                {
                    startYouTubeDLProcesses(watchUrls, youtubeWatchUrl, watchUrlInTitle);
                }
                else
                {
                    Console.WriteLine("Video has been already downloaded");
                    System.Threading.Thread.Sleep(5000);
                }
            }

        }

        private static void startYouTubeDLProcesses(List<string> watchUrls, string youtubeWatchUrl, string watchUrlInTitle)
        {
            Process p = new Process();
            watchUrls.Add(youtubeWatchUrl);
            p.StartInfo.FileName = youTubeDLPath + "youtube-dl.exe";
            p.StartInfo.Arguments = youtubeWatchUrl + " --output " + SaveDirPathYoutube + watchUrlInTitle + "_" + "%(title)s.%(ext)s";
            p.Start();
        }

        private static void GetWatchUrlsFromString(ref bool endOfString, string resultString, List<Tuple<int, int>> watchUrlLocations, ref int lastPosition)
        {
            while (!endOfString)
            {
                int newPositionStart = resultString.IndexOf("/watch?", lastPosition);

                if (newPositionStart != lastPosition && newPositionStart != -1)
                {
                    lastPosition = newPositionStart;
                    int newPositionEnd = resultString.IndexOf("\"", lastPosition);
                    watchUrlLocations.Add(new Tuple<int, int>(newPositionStart, newPositionEnd));
                    lastPosition = newPositionEnd;
                }
                else
                {
                    endOfString = true;
                }
            }
        }

        private static string GetResultHtml(string query)
        {
            string resultString;
            WebRequest request = WebRequest.Create(youTubeQueryBase + query);

            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);

            resultString = reader.ReadToEnd();
            reader.Close();
            return resultString;
        }

        private static void readConfig()
        {
            try
            {
                string[] lines = System.IO.File.ReadAllLines("config.txt");

                foreach (var line in lines)
                {
                    var splitedLine = line.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    if (splitedLine[0].Length <= 0)
                        Console.WriteLine("Invalid config argument check if is it not onyly whitespace of new line.");
                    else
                    {
                        if (splitedLine[0].Contains("DL"))
                        {
                            YouTubeDLPath = splitedLine[1];
                        }
                        if (splitedLine[0].Contains("Dir"))
                        {
                            SaveDirPathYoutube = splitedLine[1];
                        }
                        if (splitedLine[0].Contains("Request"))
                        {
                            requestDirPath = splitedLine[1];
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in reading config.txt check if is located next to this program and that has all paths written as in original txt file.");
                System.Threading.Thread.Sleep(5000);
                throw e;
            }
        }
    }
}
