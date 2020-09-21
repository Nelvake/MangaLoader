using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Domain
{
    public class ChapterInfo
    {
        [JsonProperty("volume")]
        public int Volume { get; set; }
        [JsonProperty("number")]
        public string Number { get; set; }
        [JsonProperty("slug")]
        public string Slug { get; set; }
    }
}
