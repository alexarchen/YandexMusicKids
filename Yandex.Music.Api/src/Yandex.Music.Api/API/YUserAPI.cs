using System;
using System.Threading.Tasks;

using Yandex.Music.Api.Common;
using Yandex.Music.Api.Models.Account;
using Yandex.Music.Api.Models.Common;
using Yandex.Music.Api.Requests.Account;

namespace Yandex.Music.Api.API
{
    public class YUserAPI : YCommonAPI
    {
        #region Вспомогательные функции

        private Task<YAuth> AuthPassport(AuthStorage storage, string login, string password)
        {
            return new YAuthorizeBuilder(api, storage)
                .Build((login, password))
                .GetResponseAsync();
        }

        #endregion Вспомогательные функции

        #region Основные функции

        public YUserAPI(YandexMusicApi yandex): base(yandex)
        {
        }
        
        /// <summary>
        /// Авторизация по коду
        /// </summary>
        /// <param name="storage">Хранилище</param>
        /// <param name="token">Токен авторизации</param>
        /// <returns></returns>
        public async Task AuthorizeViaCodeAsync(AuthStorage storage, TokenRequest req)
        {
            if (string.IsNullOrEmpty(req?.Code))
                throw new Exception("Задан пустой код авторизации.");

            var tokenResp = await new YGetTokenBuilder(api, storage).Build(req).GetResponseAsync();
            storage.Token = tokenResp.AccessToken;

            // Пытаемся получить информацию о пользователе
            var authInfo = await GetUserAuthAsync(storage);

            // Если не авторизован, то авторизуем
            if (string.IsNullOrEmpty(authInfo?.Result?.Account?.Uid))
                throw new Exception("Пользователь незалогинен.");

            // Флаг авторизации
            storage.IsAuthorized = true;
            //var authUserDetails = await GetUserAuthDetailsAsync(storage);
            //var authUser = authUserDetails.User;

            storage.User = authInfo.Result.Account;
        }
        /// <summary>
        /// Авторизация
        /// </summary>
        /// <param name="storage">Хранилище</param>
        /// <param name="token">Токен авторизации</param>
        /// <returns></returns>
        public async Task AuthorizeAsync(AuthStorage storage, string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new Exception("Задан пустой токен авторизации.");

            storage.Token = token;

            // Пытаемся получить информацию о пользователе
            var authInfo = await GetUserAuthAsync(storage);

            // Если не авторизован, то авторизуем
            if (string.IsNullOrEmpty(authInfo?.Result?.Account?.Uid))
                throw new Exception("Пользователь незалогинен.");

            // Флаг авторизации
            storage.IsAuthorized = true;
            //var authUserDetails = await GetUserAuthDetailsAsync(storage);
            //var authUser = authUserDetails.User;

            storage.User = authInfo.Result.Account;
        }

        /// <summary>
        /// Авторизация
        /// </summary>
        /// <param name="storage">Хранилище</param>
        /// <param name="token">Токен авторизации</param>
        /// <returns></returns>
        public void Authorize(AuthStorage storage, string token)
        {
            AuthorizeAsync(storage, token).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Авторизация
        /// </summary>
        /// <param name="storage">Хранилище</param>
        /// <param name="login">Логин</param>
        /// <param name="password">Пароль</param>
        /// <returns></returns>
        public async Task AuthorizeAsync(AuthStorage storage, string login, string password)
        {
            YAuth result = await AuthPassport(storage, login, password);

            await AuthorizeAsync(storage, result.AccessToken);
        }

        /// <summary>
        /// Авторизация
        /// </summary>
        /// <param name="storage">Хранилище</param>
        /// <param name="login">Логин</param>
        /// <param name="password">Пароль</param>
        /// <returns></returns>
        public void Authorize(AuthStorage storage, string login, string password)
        {
            AuthorizeAsync(storage, login, password).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Получение информации об авторизации
        /// </summary>
        /// <param name="storage">Хранилище</param>
        /// <returns></returns>
        public Task<YResponse<YAccountResult>> GetUserAuthAsync(AuthStorage storage)
        {
            return new YGetAuthInfoBuilder(api, storage)
                .Build(null)
                .GetResponseAsync();
        }

        #endregion Основные функции
    }
}