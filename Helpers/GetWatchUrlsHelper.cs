using System;
using System.Collections.Generic;

namespace YoutubeDownloaderChecker.Helpers
{
    static class GetWatchUrlsHelper
    {
        public static List<Tuple<int, int>> GetWatchUrlsFromString(string resultString, string searchPattern, string endPattern, ref int lastPosition)
        {
            List<Tuple<int, int>> watchUrlLocations = new List<Tuple<int, int>>();

            bool endOfString = false;
            lastPosition = 0;

            while (!endOfString)
            {
                int newPositionStart = resultString.IndexOf(searchPattern, lastPosition);
                if (newPositionStart != lastPosition && newPositionStart != -1)
                {
                    newPositionStart += searchPattern.Length;
                    lastPosition = newPositionStart;
                    //int newPositionEnd = resultString.IndexOf("&q=", lastPosition);
                    int newPositionEnd = resultString.IndexOf(endPattern, lastPosition);
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
    }
}
