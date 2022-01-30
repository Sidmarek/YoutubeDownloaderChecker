using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using YoutubeDownloaderChecker.Entities;

namespace YoutubeDownloaderChecker.Helpers
{
    static class MetadataJsonParserHelper
    {
        public static Metadata ParseJson(List<string> tagsList, List<string> categoriesList, StreamReader r)
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
    }
}
