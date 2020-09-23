using ML.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ML.Service.Interfaces
{
    public interface ILib
    {
        Task<Chapter> GetChapterInfo(string chapterId);
        Task<List<Chapter>> GetChaptersInfo(List<string> chaptersId);
        bool DownloadChapter(Chapter chapter);
        bool DownloadChapters(List<Chapter> chapters);
        Task<List<string>> GetChaptersId(string url);
    }
}
