using ML.Domain;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ML.Service
{
    public class ChapterService
    {
        private readonly string _baseUrl;
        private List<string> Cookies { get; set; }
        private string Domain { get; set; }
        private string MangaName { get; set; }

        public ChapterService(string baseUrl, List<string> cookies)
        {
            _baseUrl = baseUrl;
            Cookies = cookies;
            Domain = "img4.imgslib.ru";
        }

        public async Task<Chapter> GetChapterInfo(string chapterId)
        {
            var request = WebRequest.Create($"{_baseUrl}download/{chapterId}");
            request.Headers.Add("Cookie", Cookies.SingleOrDefault(cookie => cookie.Contains("mangalib_session")));
            using (var stream = new StreamReader((await request.GetResponseAsync()).GetResponseStream()))
            {
                return JsonConvert.DeserializeObject<Chapter>(await stream.ReadToEndAsync());
            }
        }

        public async Task<List<Chapter>> GetChaptersInfo(List<string> chaptersId)
        {
            var chaptersInfo = new List<Chapter>();
            foreach (var id in chaptersId)
            {
                chaptersInfo.Add(await GetChapterInfo(id));
            }

            return chaptersInfo.OrderBy(x => x.ChapterInfo.Volume).ThenBy(y => y.ChapterInfo.Number).ToList();
        }

        public bool DownloadChapter(Chapter chapter)
        {
            var downloadUrl = string.Empty;
            var path = $@"{MangaName}\{chapter.ChapterInfo.Volume} volume\{chapter.ChapterInfo.Number} chapter";
            if (!Directory.Exists(MangaName)) Directory.CreateDirectory(MangaName);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            using (WebClient client = new WebClient())
            {
                foreach (var image in chapter.Images)
                {
                    downloadUrl = $"https://{Domain}/manga/{MangaName}/chapters/{chapter.ChapterInfo.Slug}/{image}";

                    client.DownloadFile(new Uri(downloadUrl), @$"{path}\{image}");
                }
            }

            return true;
        }

        public bool DownloadChapters(List<Chapter> chapters)
        {
            Parallel.ForEach(chapters, new ParallelOptions { MaxDegreeOfParallelism = 10 }, chapter => 
            {
                Console.WriteLine($"Volume {chapter.ChapterInfo.Volume}. Chapter {chapter.ChapterInfo.Number} - start download");
                DownloadChapter(chapter);
                Console.WriteLine($"Volume {chapter.ChapterInfo.Volume}. Chapter {chapter.ChapterInfo.Number} - start success");
            });

            return true;
        }

        public async Task<List<string>> GetChaptersId(string url)
        {
            SetMangaName(url);
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                var html = await response.Content.ReadAsStringAsync();

                var regex = new Regex(@"(?=chapter-item""\s+?(?=data-id.{2}(?<="")([\s\S]+?)(?="")))");
                var ids = regex.Matches(html);
                var chaptersId = new List<string>();

                for (int i = 0; i < ids.Count; i++)
                {
                    chaptersId.Add(ids[i].Groups[1].Value);
                }

                return chaptersId;
            }
        }

        private void SetMangaName(string url)
        {
            var regex = new Regex(@"(?<=me\/).*");
            MangaName = regex.Match(url).Value;
        }
    }
}
