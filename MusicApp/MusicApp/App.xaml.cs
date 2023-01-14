﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using MusicApp.Framework;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Yandex.Music.Api;
using Yandex.Music.Api.Common;
using Yandex.Music.Api.Common.Providers;
using Yandex.Music.Client;

namespace MusicApp
{
    public class HttpClientFactory : IHttpClientFactory
    {
        public static HttpClient Client { get; private set; }
        public HttpClient CreateClient(AuthStorage storage)
        {
            Client =  new HttpClient(new HttpClientHandler()
            {
                    AutomaticDecompression = DecompressionMethods.GZip,
                    UseCookies = true,
                    UseProxy = false,
                    CookieContainer = storage.Context.Cookies
            });
            Client.Timeout = TimeSpan.FromSeconds(30);
            return Client;
        }
    }
    
    public partial class App : Application
    {
        private readonly YandexMusicApi _api;
        private readonly AuthStorage _storage;
        private readonly MusicLoader _loader;

        public App()
        {
            _api = new YandexMusicApi();
            _storage = new AuthStorage(new HttpClientFactory());
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
                {
                    int wait = 2000;
                    while (true)
                    {
                        try
                        {
                            var delTask = Task.Delay(TimeSpan.FromSeconds(20));
                            await await Task.WhenAny(_api.User.AuthorizeAsync(_storage, token), delTask);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine($"Error login: {e}");
                            await Task.Delay(Math.Min(10000, wait));
                            wait = (int) (wait * 1.5);
                            continue;
                        }

                        break;
                    }
                }


                if (!_storage.IsAuthorized && !string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(pass))
                {
                    await _api.User.AuthorizeAsync(_storage, login, pass);

                    if (_storage.IsAuthorized)
                        Preferences.Set("Token", _storage.Token);
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
            Preferences.Remove("Password");
            Preferences.Set("Token", null);
            
            RequestLogin();
        }

        public async void RequestLogin()
        {
            Login page = new Login(Preferences.Get("Login", ""),"", Preferences.ContainsKey("Password"));
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
                if (args.Login.Remember)
                 Preferences.Set("Password", args.Login.Password);
                else
                    Preferences.Remove("Password");
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
