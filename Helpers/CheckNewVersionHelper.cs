using System.Diagnostics;
using System.Threading.Tasks;
using YoutubeDownloaderChecker.Entities;

namespace YoutubeDownloaderChecker.Helpers
{
    static class CheckNewVersionHelper
    {
        public static async Task<bool> CheckNewVersionByBaseAsync(Process process, Metadata currentVideoMetadata)
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
    }
}
