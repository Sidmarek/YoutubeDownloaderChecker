using LiteDB;
using System;

namespace YoutubeDownloaderChecker.Entities
{
    public class VideoVersion
    {
        [BsonId]
        public int Id { get; set; }
        public Version Version { get; set; }
        public long LongIntVersion { get; set; }
        public string Title { get; set; }
    }
}
