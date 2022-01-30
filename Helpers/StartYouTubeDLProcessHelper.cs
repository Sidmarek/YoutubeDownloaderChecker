using LiteDB;
using System;
using System.Diagnostics;
using YoutubeDownloaderChecker.Entities;

namespace YoutubeDownloaderChecker.Helpers
{
    static class StartYouTubeDLProcessHelper
    {
        public static Process StartYouTubeDLProcess(string searchQueryForDirectoryName, string youtubeWatchUrl,
            string youTubeDLPath, string saveDirPathYoutube, string dataDirPath)
        {
            Process process = new Process();
            process.StartInfo.FileName = youTubeDLPath + "youtube-dl.exe";
            process.StartInfo.Arguments = youtubeWatchUrl + " --write-info-json" + " --dump-pages --output " + saveDirPathYoutube + searchQueryForDirectoryName + "/" + "%(title)s.%(ext)s";
            process.StartInfo.RedirectStandardOutput = true;
            Console.WriteLine("Downloading youtube video from URL: " + youtubeWatchUrl);

            bool started = process.Start();


            Metadata currentVideoMetadata = null;
            //To get previouse base 64 dump
            using (var db = new LiteDatabase(dataDirPath + "Database.db"))
            {
                var collection = db.GetCollection<Metadata>("Metadata");
                var metadatas = collection.Query().Where(p => p.Json == null).ToList();
                if (collection.Query().Where(p 
                    => p.BaseVideoString != null && p.Url == youtubeWatchUrl).ToList().Count > 0)
                {
                    currentVideoMetadata = collection.Query().Where(p=> 
                        p.BaseVideoString != null && p.Url == youtubeWatchUrl).First();
                }
            }

            var newVideo = CheckNewVersionHelper.CheckNewVersionByBaseAsync(process, currentVideoMetadata);

            //Skip first line only console output
            process.StandardOutput.ReadLine();

            //Skip second line and store int var it could be useful 
            var urlInConsole = process.StandardOutput.ReadLine();
            //Whole base64 video String
            var wholeVieo = process.StandardOutput.ReadToEndAsync();

            return process;
        }
    }
}
