using System;
using System.Linq;
using YoutubeDownloaderChecker.Entities;

namespace YoutubeDownloaderChecker.Helpers
{
    static class ReadConfigHelper
    {
        public static ConfigResult ReadConfig(string fileName)
        {
            var config = new ConfigResult();
            try
            {
                string[] lines = System.IO.File.ReadAllLines(fileName);

                foreach (var line in lines)
                {
                    var splitedLine = line.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

                    if (splitedLine.Length <= 0)
                    {
                        Console.WriteLine("\n Invalid config argument check if is it not onyly whitespace of new line.");
                        return null;
                    }

                    if (splitedLine.Contains("DL"))
                    {
                        config.YouTubeDLPath = splitedLine[1];
                    }
                    if (splitedLine.Contains("Dir"))
                    {
                        config.SaveDirPathYoutube = splitedLine[1];
                    }
                    if (splitedLine.Contains("Request"))
                    {
                        config.RequestDirPath = splitedLine[1];
                    }
                    if (splitedLine.Contains("Data"))
                    {
                        config.DataDirPath = splitedLine[1];
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\n Error in reading config.txt check if is located next to this program and that has all paths written as in original txt file.");
                System.Threading.Thread.Sleep(5000);
                throw e;
            }

            return config;
        }
    }
}
