using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Domain
{
    public class Chapter
    {
        [JsonProperty("chapter")]
        public ChapterInfo ChapterInfo { get; set; }
        [JsonProperty("images")]
        public List<string> Images { get; set; } = new List<string>();
    }
}
