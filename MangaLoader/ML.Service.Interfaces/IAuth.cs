using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ML.Service.Interfaces
{
    public interface IAuth
    {
        Task<List<string>> AuthAsync(string email, string password);
    }
}
