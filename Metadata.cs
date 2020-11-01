using System;
using System.Collections.Generic;
using System.Text;

namespace YoutubeDownloaderChecker
{
    public class Metadata
    {
        //public string Id { get; set;  }
        public string Title { get; set;  }
        public string Description { get; set;  }
        public List<string> Tags { get; set;  }
        public int ViewCount { get; set;  }
        public string json { get; set;  }
    }
}
