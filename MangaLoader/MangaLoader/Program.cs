using ML.Service;
using ML.Service.Interfaces;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CommandLine;
using System.Linq;
using ML.Service.Options;

namespace MangaLoader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                await Parser.Default.ParseArguments<ConsoleOption>(args).WithParsedAsync(async x =>
                {
                    var generalUrl = "https://mangalib.me/";

                    var stopwatch = new Stopwatch();
                    IAuth authService = new AuthService(generalUrl);
                    ILib chapterService = new ChapterService(generalUrl, await authService.AuthAsync(x.Login, x.Password));

                    Console.WriteLine("Getting information about a manga...");

                    var chaptersId = await chapterService.GetChaptersId(x.MangaLink);
                    var chapters = await chapterService.GetChaptersInfo(chaptersId);

                    Console.WriteLine("Starting download...");

                    stopwatch.Start();
                    chapterService.DownloadChapters(chapters);
                    stopwatch.Stop();

                    Console.WriteLine($"Download completed, elapsed time - {stopwatch.Elapsed}");
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
        }
    }
}
