using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Yandex.Music.Api.Models.Common;

namespace Yandex.Music.Api.Common.Providers
{
    /// <summary>
    /// Стандартный провайдер запросов
    /// </summary>
    public class DefaultRequestProvider: CommonRequestProvider
    {
        #region Вспомогательные функции

        private Exception ProcessException(Exception ex)
        {
            if (ex is not WebException webException) 
                return ex;

            using StreamReader sr = new(webException.Response.GetResponseStream());
            string result = sr.ReadToEnd();


            YErrorResponse exception = JsonConvert.DeserializeObject<YErrorResponse>(result);

            return exception ?? ex;
        }

        #endregion Вспомогательные функции

        #region Основные функции

        private HttpClient _client;

        public DefaultRequestProvider(AuthStorage authStorage) : base(authStorage)
        {
#if NETCOREAPP
                _client = new(new SocketsHttpHandler {
                    Proxy = storage.Context.WebProxy,
                    AutomaticDecompression = DecompressionMethods.GZip,
                    UseCookies = true,
                    CookieContainer = storage.Context.Cookies,
                });
#endif

#if NETSTANDARD2_0
            _client = HttpClientFactory.Create(new HttpClientHandler()
            {
                Proxy = storage.Context.WebProxy,
                AutomaticDecompression = DecompressionMethods.GZip,
                UseCookies = true,
                UseProxy = false,
                CookieContainer = storage.Context.Cookies
            });
#endif
            _client.Timeout = TimeSpan.FromSeconds(30);
        }

        #endregion Основные функции

        #region IRequestProvider

        public override async Task<HttpResponseMessage> GetWebResponseAsync(HttpRequestMessage message)
        {
            try
            {
                var sw = new Stopwatch();
                sw.Start();
                using var ctx = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var res = await _client.SendAsync(message, ctx.Token);
                Debug.WriteLine($"Request {message.RequestUri} finished in {sw.ElapsedMilliseconds} ms with status {res.StatusCode}");
                return res;
            }
            catch (Exception ex)
            {
                throw ProcessException(ex);
            }
        }

        #endregion IRequestProvider
    }
}