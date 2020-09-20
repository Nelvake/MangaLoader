using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MangaLoader
{
    public class Manga
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("eng_name")]
        public string EngName { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("rus_name")]
        public string RusName { get; set; }
        [JsonProperty("slug")]
        public string Slug { get; set; }

        public override string ToString()
        {
            return $"Id: {Id}\nName: {Name}\nEngName: {EngName}\nRusName: {RusName}\nSlug: {Slug}\n";
        }
    }
}
