using LiteDB;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace YoutubeDownloaderChecker
{
    class Program
    {
        const string youTubeQueryBase = "https://www.youtube.com/results?search_query=";
        private static string saveDirPathYoutube = string.Empty;
        private static string youTubeDLPath = string.Empty;
        private static string requestDirPath = string.Empty;
        private static String resultString = string.Empty;
        private static List<Process> processes = new List<Process>();
        private static string dataDirPath = string.Empty;

        public static string YouTubeDLPath { get => youTubeDLPath; set => youTubeDLPath = value; }
        public static string SaveDirPathYoutube { get => saveDirPathYoutube; set => saveDirPathYoutube = value; }
        public static string RequestDirPath { get => requestDirPath; set => requestDirPath = value; }
        public static string ResultString { get => resultString; set => resultString = value; }
        public static string DataDirPath { get => dataDirPath; set => dataDirPath = value; }

        static void Main(string[] args)
        {

            List<Tuple<int, int>> watchUrlLocations = new List<Tuple<int, int>>();
            int lastPosition = 0;

            readConfig();

            var lines = System.IO.File.ReadAllLines(requestDirPath);

            foreach (string searchQuery in lines)
            {
                ResultString += GetResultHtml(searchQuery);

                GetWatchUrlsFromString(ResultString, watchUrlLocations, ref lastPosition);
            }

            List<string> watchUrls = new List<string>();

            foreach (var watchUrlLocation in watchUrlLocations)
            {
                string watchUrl = ResultString.Substring(watchUrlLocation.Item1, watchUrlLocation.Item2 - watchUrlLocation.Item1);
                string youtubeWatchUrl = "https://www.youtube.com" + watchUrl;
                ///string watchUrlInTitle = watchUrl.Replace('?', '-');

                Console.WriteLine(youtubeWatchUrl);

                var urls = File.ReadLines(DataDirPath + "watchUrls.txt").ToList();
                if (urls.Any(p => !p.Contains(youtubeWatchUrl)))
                {
                    startYouTubeDLProcess(watchUrls, youtubeWatchUrl);
                }
                else
                {
                    Console.WriteLine("Video has been already downloaded");
                    System.Threading.Thread.Sleep(5000);
                }

                //Adds watchUrl into list
                watchUrls.Add(youtubeWatchUrl);
            }


            using (System.IO.StreamWriter file = new System.IO.StreamWriter(DataDirPath + "watchUrls.txt"))
            {
                foreach (string youtubeWatchUrl in watchUrls)
                {
                    file.WriteLine(youtubeWatchUrl);
                }
            }

        }

        private static void startYouTubeDLProcess(List<string> watchUrls, string youtubeWatchUrl)
        {
            Process process = new Process();
            watchUrls.Add(youtubeWatchUrl);
            process.StartInfo.FileName = youTubeDLPath + "youtube-dl.exe";
            process.StartInfo.Arguments = youtubeWatchUrl + " --write-info-json" + " --output " + SaveDirPathYoutube + /*watchUrlInTitle + "_" + */"%(title)s.%(ext)s";
            //process.StartInfo.RedirectStandardOutput = true;


            bool started = process.Start();
            processes.Add(process);

           //string filename = process.StandardOutput.ReadLine();


            var files = Directory.GetFiles(SaveDirPathYoutube, "*.json");
            List<string> tagsList = new List<string>();
            List<string> categoriesList = new List<string>();

            foreach ( var file in files) 
            {
                try
                {
                    using (StreamReader r = new StreamReader(file))
                    {
                        string json = r.ReadToEnd();
                        dynamic items = JsonConvert.DeserializeObject(json);
                        var tags = items["tags"];
                        var categories = items["categories"];

                        if (tags.ToString() != string.Empty)
                        {
                            foreach (var tag in tags)
                            {
                                tagsList.Add(tag.ToString());
                            }
                        }

                        if (categories.ToString() != string.Empty)
                        {
                            foreach (var categorie in categories)
                            {
                                categoriesList.Add(categorie.ToString());
                            }
                        }
                        var description = items["description"].ToString();
                        var title = items["title"].ToString();
                        var uploader = items["uploader"].ToString();
                        var viewCount = items["view_count"];

                        var metadata = new Metadata()
                        {
                            Description = description,
                            Title = title,
                            ViewCount = viewCount,
                            Tags = tagsList,
                            Categories = categoriesList,
                            Uploader = uploader,
                            Downloaded = DateTime.Now
                        };

                        //Clean after one iteration
                        tagsList = new List<string>();
                        categoriesList = new List<string>();

                        using (var db = new LiteDatabase(DataDirPath + "Database.db"))
                        {
                            var collection = db.GetCollection<Metadata>("Metadata");
                            var test = collection.Query().Where(p => p.json == null).ToList();
                            var testing = collection.Query().Where(p => p.Title == metadata.Title).ToList();
                            if (testing.Count == 0)
                            {
                                collection.Insert(metadata);
                                db.Commit();
                                collection.EnsureIndex(x => x.Title);
                            }
                        }
                        //tagsList.Add(tags);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Json file cannot be read named: " + file + ". Ended with error " + e.Message);
                }

            }

            // do something with line
        }

        private static void GetWatchUrlsFromString(string resultString, List<Tuple<int, int>> watchUrlLocations, ref int lastPosition)
        {
            bool endOfString = false;
            lastPosition = 0;
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
                            RequestDirPath = splitedLine[1];
                        }
                        if (splitedLine[0].Contains("Data"))
                        {
                            DataDirPath = splitedLine[1];
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
