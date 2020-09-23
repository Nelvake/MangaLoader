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
        /// <returns>Cookies возвращаемые после авторизации</returns>
        public async Task<List<string>> AuthAsync(string email, string password)
        {
            var url = new Uri($"{_baseUrl}login");

            // Form-data с полями для авторизации на сайте
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
                // Добавляем cookie для авторизации
                cookieContainer.Add(url, Cookies);
                var response = await client.PostAsync(url, content);

                CheckAuthorization(response);

                // Вытаскиваем из ответа cookie для дальнейших манипуляций с сайтом
                // mangalib_session - отвечает за текущую сессию в которой содержится информации об авторизации
                return response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value.ToList();
            }
        }

        /// <summary>
        /// Проверка авторизации
        /// </summary>
        /// <param name="response">Ответ от сервера</param>
        private void CheckAuthorization(HttpResponseMessage response)
        {
            // Проверяем наличие cookies которые приходять при авторизации
            if (response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value.ToList().Count != 3)
                throw new Exception("Authorization error");

            Console.WriteLine("Authorization was successful");
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
                    // Делаем запрос для получение главной страницы
                    HttpResponseMessage response = client.GetAsync(_baseUrl).Result;
                    var html = await response.Content.ReadAsStringAsync();

                    Regex regex = new Regex("_token.*(content=\\\"(?<=\")([\\s\\S]+?)(?=\\\"))");
                    // Вытаскиваем csrf-token
                    AuthToken = regex.Matches(html)[0].Groups[2].Value;
                }
            }
            Uri uri = new Uri(_baseUrl);
            // Получаем cookie с помощью которых будет прооходить авторизация
            Cookies = cookies.GetCookies(uri);
        }
    }
}
