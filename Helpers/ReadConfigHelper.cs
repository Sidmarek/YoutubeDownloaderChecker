using System;

namespace YoutubeDownloaderChecker.Helpers
{
    static class ReadConfigHelper
    {
        public static void ReadConfig(string youTubeDLPath, string saveDirPathYoutube, string requestDirPath, string dataDirPath)
        {
            if (string.IsNullOrEmpty(youTubeDLPath))
                throw new ArgumentException($"'{nameof(youTubeDLPath)}' cannot be null or empty.", nameof(youTubeDLPath));

            if (string.IsNullOrEmpty(saveDirPathYoutube))
                throw new ArgumentException($"'{nameof(saveDirPathYoutube)}' cannot be null or empty.", nameof(saveDirPathYoutube));

            if (string.IsNullOrEmpty(requestDirPath))
                throw new ArgumentException($"'{nameof(requestDirPath)}' cannot be null or empty.", nameof(requestDirPath));

            if (string.IsNullOrEmpty(dataDirPath))
                throw new ArgumentException($"'{nameof(dataDirPath)}' cannot be null or empty.", nameof(dataDirPath));

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
                            youTubeDLPath = splitedLine[1];
                        }
                        if (splitedLine[0].Contains("Dir"))
                        {
                            saveDirPathYoutube = splitedLine[1];
                        }
                        if (splitedLine[0].Contains("Request"))
                        {
                            requestDirPath = splitedLine[1];
                        }
                        if (splitedLine[0].Contains("Data"))
                        {
                            dataDirPath = splitedLine[1];
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

            return 
        }
    }
}
