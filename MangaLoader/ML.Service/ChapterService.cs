﻿using ML.Domain;
using ML.Service.Interfaces;
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
    public class ChapterService : ILib
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

        /// <summary>
        /// Получение информации о главе
        /// </summary>
        /// <param name="chapterId">Номер главы</param>
        /// <returns></returns>
        public async Task<Chapter> GetChapterInfo(string chapterId)
        {
            var request = WebRequest.Create($"{_baseUrl}download/{chapterId}");
            request.Headers.Add("Cookie", Cookies.SingleOrDefault(cookie => cookie.Contains("mangalib_session")));
            using (var stream = new StreamReader((await request.GetResponseAsync()).GetResponseStream()))
            {
                return JsonConvert.DeserializeObject<Chapter>(await stream.ReadToEndAsync());
            }
        }

        /// <summary>
        /// Получение информации о нескольких главах
        /// </summary>
        /// <param name="chaptersId">Список номеров глав</param>
        /// <returns></returns>
        public async Task<List<Chapter>> GetChaptersInfo(List<string> chaptersId)
        {
            var chaptersInfo = new List<Chapter>();
            foreach (var id in chaptersId)
            {
                chaptersInfo.Add(await GetChapterInfo(id));
            }

            return chaptersInfo.OrderBy(x => x.ChapterInfo.Volume).ThenBy(y => y.ChapterInfo.Number).ToList();
        }

        /// <summary>
        /// Загрузка одной главы
        /// </summary>
        /// <param name="chapter">Глава</param>
        /// <returns></returns>
        public bool DownloadChapter(Chapter chapter)
        {
            try
            {
                var downloadUrl = string.Empty;
                var path = $@"{MangaName}\{chapter.ChapterInfo.Volume} volume\{chapter?.ChapterInfo.Number} chapter";
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
                    return true;
                }
            }
            catch (WebException)
            {
                return false;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Загрузка нескольких глав
        /// </summary>
        /// <param name="chapters"></param>
        /// <returns></returns>
        public bool DownloadChapters(List<Chapter> chapters)
        {
            try
            {
                Parallel.ForEach(chapters, new ParallelOptions { MaxDegreeOfParallelism = 15 }, chapter =>
                {
                    ShowDownloadStatus(chapter, DownloadChapter(chapter));
                });

                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Отображение в консоли статус загрузки
        /// </summary>
        /// <param name="chapter">Скачиваемая глава</param>
        /// <param name="status">Статус загрузки</param>
        private void ShowDownloadStatus(Chapter chapter, bool status)
        {
            Console.Write($" - Volume {chapter.ChapterInfo.Volume}. Chapter {chapter.ChapterInfo.Number}\t");
            if (status)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(" | Download success");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" | Download error");
            }
            Console.ResetColor();
        }

        /// <summary>
        /// Получает номера глав по ссылке
        /// </summary>
        /// <param name="url">Ссылка на мангу</param>
        /// <returns></returns>
        public async Task<List<string>> GetChaptersId(string url)
        {
            SetMangaName(url);
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                var html = await response.Content.ReadAsStringAsync();

                var regex = new Regex(@"(?=chapter-item""\s+?(?=data-id.{2}(?<="")([\s\S]+?)(?="")))");
                var chaptersId = new List<string>();
                regex.Matches(html).ToList().ForEach(x => chaptersId.Add(x.Groups[1].Value));

                return chaptersId;
            }
        }

        /// <summary>
        /// Инициализирует название манги по ссылке
        /// </summary>
        /// <param name="url">Ссылка на мангу</param>
        private void SetMangaName(string url)
        {
            var regex = new Regex(@"(?<=me\/).*");
            MangaName = regex.Match(url).Value;
        }
    }
}
