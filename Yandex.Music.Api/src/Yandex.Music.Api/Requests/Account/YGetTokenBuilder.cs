using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Yandex.Music.Api.Common;
using Yandex.Music.Api.Models.Account;
using Yandex.Music.Api.Requests.Common;

namespace Yandex.Music.Api.Requests.Account
{
    public class TokenRequest
    {
        public string Code { get; set; }
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        
        public string ClientId { get; set; }
        
        public string ClientSecret { get; set; }
    }
        
    [YOAuthRequest(WebRequestMethods.Http.Post, "token")]
    public class YGetTokenBuilder : YRequestBuilder<YAuth, TokenRequest>
    {

        public YGetTokenBuilder(YandexMusicApi yandex, AuthStorage auth) : base(yandex, auth)
        {
        }

        protected override HttpContent GetContent(TokenRequest req)
        {
            return new FormUrlEncodedContent(new Dictionary<string, string> {
                { "grant_type", "authorization_code" },
                { "client_id", req.ClientId },
                { "client_secret", req.ClientSecret },
                { "device_id", req.DeviceId },
                { "device_name", req.DeviceName },
                { "code", req.Code}
            });
        }
    }

}