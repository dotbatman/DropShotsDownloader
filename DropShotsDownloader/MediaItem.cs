using System;
using System.Collections.Generic;
using System.Text;

namespace DropShotsDownloader
{
    class Stupid
    {
        public Stuff media { get; set; }
    }
    class Stuff
    {
        public List<MediaItem> Item { get; set; }
    }

    class MediaItem
    {
        public string type { get; set; } 
        public DateTime date { get; set; } 
        public TimeSpan time { get; set; }
        public string download { get; set; }
        public string downloadname { get; set; }
    }
}
