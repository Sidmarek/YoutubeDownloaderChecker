using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace YoutubeDownloaderChecker.Helpers
{
    static class FindFilesPerSeachQueryHelper
    {
        public static List<string> FindFilesPerSearchQuery(string searchQueryForDirectoryName, string saveDirPathYoutube)
        {
            List<string> files = new List<string>();

            string saveDirPath = saveDirPathYoutube + searchQueryForDirectoryName;

            if (!Directory.Exists(saveDirPath))
            {
                Directory.CreateDirectory(saveDirPath);
            }

            files.AddRange(Directory.GetFiles(saveDirPath, "*.json").ToList());
            Directory.CreateDirectory(saveDirPathYoutube + searchQueryForDirectoryName + @"\JSON");
            files.AddRange(Directory.GetFiles(saveDirPathYoutube + searchQueryForDirectoryName + @"\JSON\", "*.json").ToList());

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

            List<string> movedFiles = Directory.GetFiles(saveDirPathYoutube + searchQueryForDirectoryName + @"\JSON\", "*.json").ToList();

            if (!files.Any(p => movedFiles.Any(q => q == p)))
            {
                files.AddRange(movedFiles);
            }

            return files;
        }
    }
}
