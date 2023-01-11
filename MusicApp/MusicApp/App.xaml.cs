using System;
using System.IO;
using System.Threading.Tasks;
using MusicApp.Framework;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Yandex.Music.Api;
using Yandex.Music.Api.Common;
using Yandex.Music.Client;

namespace MusicApp
{
    public partial class App : Application
    {
        private readonly YandexMusicApi _api;
        private readonly Yandex.Music.Api.Common.AuthStorage _storage;
        private readonly MusicLoader _loader;

        public App()
        {
            _api = new YandexMusicApi();
            _storage = new AuthStorage();
            _loader = new MusicLoader(_api, _storage, this, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "loader.cache"));
            
            InitializeComponent();

            MainPage = new NavigationPage(new MainPage());
        }

        public IMusicLoader Loader => _loader;
        protected override void OnStart()
        {
            string login = Preferences.Get("Login", null);
            string pass = Preferences.Get("Password", null);
            string token = Preferences.Get("Token", null);
            Task.Run(async () =>
            {
                if (!string.IsNullOrEmpty(token))
                    await _api.User.AuthorizeAsync(_storage, token);

                if (!_storage.IsAuthorized)
                {
                    if (!string.IsNullOrEmpty(login) && string.IsNullOrEmpty(pass))
                        await _api.User.AuthorizeAsync(_storage, login, pass);

                    Preferences.Set("Token", "");
                }

                if (!_storage.IsAuthorized)
                {
                   Device.BeginInvokeOnMainThread(RequestLogin);
                }
                else
                {
                    await _loader.Reload();
                }
            });

        }

        public void Logout()
        {
            string login = Preferences.Get("Login", null);
            Preferences.Set("Password", null);
            Preferences.Set("Token", null);
            
            RequestLogin();
        }

        public async void RequestLogin()
        {
            Login page = new Login(Preferences.Get("Login", ""),"");
            page.LoginClicked += LoginClicked;
            await MainPage.Navigation.PushModalAsync(page, true);
        }

        async void LoginClicked(object o, Login.LoginEventArgs args)
        {
            await _api.User.AuthorizeAsync(_storage, args.Login.Login, args.Login.Password);
                
            if (_storage.IsAuthorized)
            {
                (o as Login)!.LoginClicked -= LoginClicked;
                await (o as Login)!.Navigation.PopModalAsync();
                
                Preferences.Set("Login", args.Login.Login);
                Preferences.Set("Password", args.Login.Password);
                Preferences.Set("Token",_storage.Token);

                await (o as Login)!.Navigation.PopModalAsync();
                
                _ = _loader.Reload();
            }
            else
            {
                await (o as Login)!.DisplayAlert("Attention!", "Error during login! Check login, password and Internet", "Ok");
            }
                
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
