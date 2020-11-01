using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace YoutubeDownloaderChecker
{
    public class VideoVersion
    {
        [BsonId]
        public int Id { get; set; }
        public Version Version { get; set; }
        public string Title { get; set; }
    }
}
