using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ML.Service
{
    public class AuthService
    {
        private string AuthToken { get; set; }
        private CookieCollection Cookies { get; set; }
        private readonly string _baseUrl;

        public AuthService(string baseUrl)
        {
            _baseUrl = baseUrl;
            Cookies = new CookieCollection();
            SetCookiesAndToken();
        }

        /// <summary>
        /// Авторизация на сайте
        /// </summary>
        /// <param name="email">Почта или никнейм</param>
        /// <param name="password">Пароль</param>
        /// <returns>Cookies</returns>
        public async Task<List<string>> AuthAsync(string email, string password)
        {
            var url = new Uri($"{_baseUrl}/login");
            var data = new Dictionary<string, string>
            {
                { "_token", AuthToken },
                { "email", email },
                { "password", password },
            };

            var content = new FormUrlEncodedContent(data);

            var cookieContainer = new CookieContainer();
            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = url })
            {
                cookieContainer.Add(url, Cookies);
                var response = await client.PostAsync(url, content);

                return response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value.ToList();
            }
        }

        /// <summary>
        /// Имитация работы сайта для получения AuthToken & Cookies
        /// </summary>
        private async void SetCookiesAndToken()
        {
            CookieContainer cookies = new CookieContainer();
            using (HttpClientHandler handler = new HttpClientHandler())
            {
                handler.CookieContainer = cookies;
                using (HttpClient client = new HttpClient(handler))
                {
                    HttpResponseMessage response = client.GetAsync(_baseUrl).Result;
                    var html = await response.Content.ReadAsStringAsync();

                    Regex regex = new Regex("_token.*(content=\\\"(?<=\")([\\s\\S]+?)(?=\\\"))");
                    AuthToken = regex.Matches(html)[0].Groups[2].Value;
                }
            }
            Uri uri = new Uri(_baseUrl);
            Cookies = cookies.GetCookies(uri);
        }
    }
}
