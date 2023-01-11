using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Yandex.Music.Api.Common;
using Yandex.Music.Api.Extensions;
using Yandex.Music.Api.Requests.Common;

namespace Yandex.Music.Api.API
{
    public class YCustomAPI: YCommonAPI
    {
        public YCustomAPI(YandexMusicApi yandex) : base(yandex)
        {
        }

        public async Task<string> GetAsync(AuthStorage storage, string request)
        {
            using (var client = new HttpClient())
            {

                var msg = new HttpRequestMessage(HttpMethod.Get, "https://api.music.yandex.net/" + request);
                msg.Headers.TryAddWithoutValidation(HttpRequestHeader.AcceptCharset.GetName(), Encoding.UTF8.WebName);

                // Добавление заголовка авторизации
                if (!string.IsNullOrEmpty(storage.Token))
                    msg.Headers.TryAddWithoutValidation(HttpRequestHeader.Authorization.GetName(), $"OAuth {storage.Token}");
                
                var res = await client.SendAsync(msg);
                if (res.IsSuccessStatusCode)
                {
                    return await res.Content.ReadAsStringAsync();
                }
                
                return "Error: "+res.StatusCode;
            }

        }
    }
}