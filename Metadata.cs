﻿using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace YoutubeDownloaderChecker
{
    public class Metadata
    {
        [BsonId]
        public int Id { get; set;  }
        public string Title { get; set;  }
        public string Url { get; set;  }
        public string Description { get; set;  }
        public string Uploader { get; set;  }
        public DateTime Downloaded { get; set;  }
        public DateTime? ReleaseDate { get; set;  }
        public int? Duration { get; set;  }
        public List<string> Tags { get; set;  }
        public List<string> Categories { get; set;  }
        public int ViewCount { get; set;  }
        public VideoVersion VideoVersion { get; set;  }
        public string Json { get; set;  }
        public String BaseVideoString { get; set;  }
    }
}
