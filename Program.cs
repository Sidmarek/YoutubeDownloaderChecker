using CefSharp;
using CefSharp.OffScreen;
using LiteDB;
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
using System.Threading.Tasks;

namespace YoutubeDownloaderChecker
{
    class Program
    {
        const string facebookUrlBase = "https://www.facebook.com/search/videos/?q=";
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
        private static string[] RequestQueries;
        private static ChromiumWebBrowser browser;

        static void Main(string[] args)
        {

            List<Tuple<string, int, int>> watchUrlLocations = new List<Tuple<string, int, int>>();
            List<Tuple<string, string>> watchUrls = new List<Tuple<string, string>>();
            int lastPosition = 0;

            readConfig();
            
            var requestQueries = System.IO.File.ReadAllLines(requestDirPath);
            RequestQueries = requestQueries;

            var settings = new CefSettings()
            {
                //By default CefSharp will use an in-memory cache, you need to specify a Cache Folder to persist data
                CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache")
            };

            //Perform dependency check to make sure all relevant resources are in our output directory.
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);

            //// Create the offscreen Chromium browser.
            //browser = new ChromiumWebBrowser("https://www.facebook.com/");
            //browser.LoadingStateChanged += BrowserLoadingStateChanged;

            foreach (string searchQuery in requestQueries)
            {
                GetFacebookHtmlAsync(facebookUrlBase + searchQuery);

                //string queryHtmlString = GetResultHtml(searchQuery);
                //var newLocations = GetWatchUrlsFromString(queryHtmlString, ref lastPosition);
                //foreach (var newLocation in newLocations)
                //{
                //    //var newItem = new Tuple<string, int, int>(searchQuery, newLocation.Item2, newLocation.Item3);
                //    //watchUrlLocations.Add(newItem);
                //    string watchUrl = queryHtmlString.Substring(newLocation.Item1, newLocation.Item2 - newLocation.Item1);
                //    string youtubeWatchUrl = "https://www.youtube.com" + watchUrl;

                //    //Console.WriteLine(youtubeWatchUrl);
                //    watchUrls.Add(new Tuple<string, string>(searchQuery, youtubeWatchUrl));
                //}
            }
            Console.ReadKey();

            foreach (var watchUrl in watchUrls)
            {
                var process = StartYouTubeDLProcess(watchUrl.Item1, watchUrl.Item2);
                processes.Add(process);
            }
            
            List<string> files = new List<string>();

            foreach (var searchQueryString in requestQueries) 
            {
                files.AddRange(FindFilesPerSearchQuery(searchQueryString));
            }

            List<string> tagsList = new List<string>();
            List<string> categoriesList = new List<string>();

            foreach (var file in files)
            {
                try
                {
                    using (StreamReader r = new StreamReader(file))
                    {
                        Metadata metadata = ParseJson(tagsList, categoriesList, r);

                        //Clean after one iteration
                        tagsList = new List<string>();
                        categoriesList = new List<string>();
                        UpdateOrCreateMetadataFromJsonToLcalDB(metadata);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Json file cannot be read named: " + file + ". Ended with error " + e.Message);
                }

            }

            while (!processes.All(p => p.HasExited == true))
            {

            }

            Console.WriteLine("All videos has been successfuly downloaded and checked ");
        }

        private static async void GetFacebookHtmlAsync(string url)
        {
            //Reduce rendering speed to one frame per second, tweak this to whatever suites you best
            using (var browser = new ChromiumWebBrowser(url))
            {
                //await LoadPageAsync(browser);
                browser.LoadUrlWithPostData
                //Get the browser source
                var source = await browser.GetSourceAsync();

                Console.WriteLine(source);

                //Allow for a little delay before attempting to `Dispose` of the ChromiumWebBrowser,
                // some of the background IPC messages need a few extra ticks to compelte,
                // if you perform some more complex operations this is likely not required.
                await Task.Delay(10);
            }
        }

        public static Task LoadPageAsync(IWebBrowser browser)
        {
            var tcs = new TaskCompletionSource<bool>();

            EventHandler<LoadingStateChangedEventArgs> handler = null;
            handler = (sender, args) =>
            {
                //Wait for while page to finish loading not just the first frame
                if (!args.IsLoading)
                {
                    browser.LoadingStateChanged -= handler;
                    tcs.TrySetResult(true);
                }
            };

            browser.LoadingStateChanged += handler;

            return tcs.Task;
        }

        private static void UpdateOrCreateMetadataFromJsonToLcalDB(Metadata metadata)
        {
            using (var db = new LiteDatabase(DataDirPath + "Database.db"))
            {
                var collection = db.GetCollection<Metadata>("Metadata");
                var videoVersionCollection = db.GetCollection<VideoVersion>("VideoVersion");
                var test = collection.Query().Where(p => p.Json == null).ToList();
                var matedataSameTitleList = collection.Query().Where(p => p.Title == metadata.Title).ToList();

                if (matedataSameTitleList.Count == 0)
                {
                    videoVersionCollection.Insert(metadata.VideoVersion);
                    var bsonValue = collection.Insert(metadata);
                    Console.WriteLine($"Metadata for video: {metadata.Title} has been set to Id {bsonValue.ToString()}.");
                }
                else
                {
                    var foundMetadata = matedataSameTitleList.FindLast(p => p.VideoVersion.LongIntVersion >= 0);

                    if (foundMetadata != null)
                    {
                        if (foundMetadata.BaseVideoString != metadata.BaseVideoString ||
                            foundMetadata.Categories != metadata.Categories ||
                            foundMetadata.Description != metadata.Description ||
                            foundMetadata.Downloaded != metadata.Downloaded ||
                            foundMetadata.Duration != metadata.Duration ||
                            foundMetadata.Json != metadata.Json ||
                            foundMetadata.ReleaseDate != metadata.ReleaseDate ||
                            foundMetadata.Tags != metadata.Tags ||
                            foundMetadata.Title != metadata.Title ||
                            foundMetadata.Uploader != metadata.Uploader ||
                            foundMetadata.Url != metadata.Url ||
                            foundMetadata.VideoVersion != metadata.VideoVersion ||
                            foundMetadata.ViewCount != metadata.ViewCount)
                        {
                            metadata.VideoVersion.LongIntVersion = foundMetadata.VideoVersion.LongIntVersion++;
                            collection.Insert(metadata);
                            Console.WriteLine($"Metadata for video: {metadata.Title} has been updated to version {metadata.VideoVersion.LongIntVersion}.");
                        }
                    }
                }
                db.Commit();
                collection.EnsureIndex(x => x.Id);
            }
        }

        private static Metadata ParseJson(List<string> tagsList, List<string> categoriesList, StreamReader r)
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
            int? duration = null;
            if (items["duration"] != null)
            {
                duration = items["duration"];
            }
            var releaseDateString = items["release_date"].ToString();

            DateTime? releaseDate = null;

            if (releaseDateString != null && releaseDateString != string.Empty)
            {
                releaseDate = DateTime.Parse(releaseDateString);
            }

            var title = items["title"].ToString();
            var uploader = items["uploader"].ToString();
            var url = items["webpage_url"].ToString();
            var viewCount = items["view_count"];

            return new Metadata()
            {
                Description = description,
                Title = title,
                ViewCount = viewCount,
                Tags = tagsList,
                Categories = categoriesList,
                Duration = duration,
                ReleaseDate = releaseDate,
                Uploader = uploader,
                BaseVideoString = null,
                Url = url,
                VideoVersion = new VideoVersion() { Title = title, Version = new Version("1.0"), LongIntVersion = 1 },
                Json = json,
                Downloaded = DateTime.Now
            };
        }

        private static List<string> FindFilesPerSearchQuery(string searchQueryForDirectoryName)
        {
            List<string> files = new List<string>();

            string saveDirPath = SaveDirPathYoutube + searchQueryForDirectoryName;

            if (!Directory.Exists(saveDirPath))
            {
                Directory.CreateDirectory(saveDirPath);
            }

            files.AddRange(Directory.GetFiles(saveDirPath, "*.json").ToList());
            Directory.CreateDirectory(SaveDirPathYoutube + searchQueryForDirectoryName + @"\JSON");
            files.AddRange(Directory.GetFiles(SaveDirPathYoutube + searchQueryForDirectoryName + @"\JSON\", "*.json").ToList());

            bool hasFilesMoved = false;
            foreach (var file in files)
            {
                if (!file.Contains(@"\JSON\"))
                {
                    hasFilesMoved = true;
                    string movedFile = file.Insert(file.LastIndexOf(@"\"), @"\JSON\");

                    if (File.Exists(movedFile))
                    {
                        File.Delete(file);
                    }
                    else
                    {
                        Directory.Move(file, movedFile);
                    }
                }
            }

            if (hasFilesMoved == true)
            {
                files.Clear();
            }

            List<string> movedFiles = Directory.GetFiles(SaveDirPathYoutube + searchQueryForDirectoryName + @"\JSON\", "*.json").ToList();

            if (!files.Any(p => movedFiles.Any(q => q == p)))
            {
                files.AddRange(movedFiles);
            }

            return files;
        }

        private static Process StartYouTubeDLProcess(string searchQueryForDirectoryName, string youtubeWatchUrl)
        {
            Process process = new Process();
            process.StartInfo.FileName = youTubeDLPath + "youtube-dl.exe";
            process.StartInfo.Arguments = youtubeWatchUrl + " --write-info-json" + " --dump-pages --output " + SaveDirPathYoutube + searchQueryForDirectoryName + "/" + "%(title)s.%(ext)s";
            process.StartInfo.RedirectStandardOutput = true;
            Console.WriteLine("Downloading youtube video from URL: " + youtubeWatchUrl);

            bool started = process.Start();


            Metadata currentVideoMetadata = null;
            //To get previouse base 64 dump
            using (var db = new LiteDatabase(DataDirPath + "Database.db"))
            {
                var collection = db.GetCollection<Metadata>("Metadata");
                var metadatas = collection.Query().Where(p => p.Json == null).ToList();
                if (collection.Query().Where(p => p.BaseVideoString != null && p.Url == youtubeWatchUrl).ToList().Count > 0)
                {
                    currentVideoMetadata = collection.Query().Where(p => p.BaseVideoString != null && p.Url == youtubeWatchUrl).First();
                }
            }

            var newVideo = CheckNewVersionByBaseAsync(process, currentVideoMetadata);

            //Skip first line only console output
            process.StandardOutput.ReadLine();

            //Skip second line and store int var it could be useful 
            var urlInConsole = process.StandardOutput.ReadLine();
            //Whole base64 video String
            var wholeVieo = process.StandardOutput.ReadToEndAsync();

           

            return process;
        }

        private static async Task<bool> CheckNewVersionByBaseAsync(Process process, Metadata currentVideoMetadata)
        {
            while (!process.StandardOutput.EndOfStream)
            {
                var baseLine = process.StandardOutput.ReadLine();

                if (currentVideoMetadata == null)
                    break;
                if (currentVideoMetadata.BaseVideoString.Contains(baseLine))
                {
                    ///TODO versions are the same
                    return false;
                }
                else
                {
                    ///TODO new version
                }

            }
            return true;
        }

        private static List<Tuple<int, int>> GetWatchUrlsFromString(string resultString, ref int lastPosition)
        {
            List<Tuple<int, int>> watchUrlLocations = new List<Tuple<int, int>>();

            bool endOfString = false;
            lastPosition = 0;

            while (!endOfString)
            {
                int newPositionStart = resultString.IndexOf("/watch?v=", lastPosition);

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

            return watchUrlLocations;
        }

        private static string GetResultHtml(string query)
        {
            string resultString;
            WebRequest request = WebRequest.Create(youTubeQueryBase + query);

            WebResponse response = request.GetResponse();
            WebRequest requestOfResponse = WebRequest.Create(response.ResponseUri);
            WebResponse responsefResponse = requestOfResponse.GetResponse();
            response.Close();
            Stream dataStream = responsefResponse.GetResponseStream();
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
