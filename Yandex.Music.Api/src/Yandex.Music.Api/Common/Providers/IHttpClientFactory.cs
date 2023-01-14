using System.Net.Http;

namespace Yandex.Music.Api.Common.Providers
{
    public interface IHttpClientFactory
    {
        HttpClient CreateClient(AuthStorage storage);
    }
}