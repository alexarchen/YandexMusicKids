using System;
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
            _loader = new MusicLoader(_api, _storage);
            
            InitializeComponent();

            MainPage = new NavigationPage(new MainPage());
        }

        protected override void OnStart()
        {
            string login = Preferences.Get("Login", null);
            string pass = Preferences.Get("Password", null);
            string token = Preferences.Get("Token", null);

            if (!string.IsNullOrEmpty(token))
                _api.User.Authorize(_storage, token);

            if (!_storage.IsAuthorized)
            {
                _api.User.Authorize(_storage, login, pass);
                Preferences.Set("Token","");
            }
            if (!_storage.IsAuthorized)
            {
                RequestLogin();
            }
            else
            {
                _ = _loader.Reload();
            }

        }

        public void RequestLogin()
        {
            Login page = new Login(Preferences.Get("Login", ""),"");
            page.LoginClicked += LoginClicked;

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
                
                await _loader.Reload();
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
