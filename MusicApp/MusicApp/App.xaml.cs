using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using MusicApp.Framework;
using MusicApp.ViewModel;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Yandex.Music.Api;
using Yandex.Music.Api.Common;
using Yandex.Music.Api.Common.Providers;
using Yandex.Music.Api.Requests.Account;

namespace MusicApp
{
    public partial class App : Application
    {
        private readonly Yandex.Music.Api.YandexMusicApi _api;
        private readonly AuthStorage _storage;
        private readonly MusicLoader _loader;
        public const string CLIENT_ID = "23cabbbdc6cd418abb4b39c32c41195d";// "ddbcdca5635f4637891b02c84ccda50f";
        public const string CLIENT_SECRET = "e0181ca2628746039469ac34f70bb4e2";

        private ILoginRequester _loginRequester;
        private readonly ILogger _logger;

        public App(ILoginRequester loginRequester, ILogger logger)
        {
            _loginRequester = loginRequester;
            _logger = logger;
            _api = new Yandex.Music.Api.YandexMusicApi();
            _storage = new AuthStorage();
            _loader = new MusicLoader(_api, _storage, this, logger, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "loader.cache"));
            
            InitializeComponent();

            var p = new NavigationPage(new MainPage());
            MainPage = p;

        }

        public IMusicLoader Loader => _loader;
        public ILogger Logger => _logger;
        protected override void OnStart()
        {
                
            FileStream stream = new FileStream(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "debug.log"), FileMode.Create,
                FileAccess.Write, FileShare.Read);

            int l = Trace.Listeners.Add(new TextWriterTraceListener(stream, "debugfile"));
            Trace.Listeners[l].TraceOutputOptions = TraceOptions.DateTime;
            
            
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
            Preferences.Set("Token", null);
            
            RequestLogin();
        }

        public async void RequestLogin()
        {
            /*
            string uid = Preferences.Get("UUID", Guid.NewGuid().ToString());
            Preferences.Set("UUID",uid);

            Login page = new Login(Preferences.Get("Logon", ""),uid);
            page.Logon += Logon;
            await MainPage.Navigation.PushModalAsync(page, true);
            
            */

            try
            {
                var token = await _loginRequester.RequestLogin(new LoginModel() { Login = Preferences.Get("Logon", null) });
                if (token != null)
                    Logon(this, new Login.LoginEventArgs() { Token = token });
            }
            catch (Exception e)
            {
                
            }
            

        }

        async void Logon(object o, Login.LoginEventArgs args)
        {
            try
            {
                if (args.Code!=null)
                    await _api.User.AuthorizeViaCodeAsync(_storage, new TokenRequest()
                    {
                        Code = args.Code, ClientId = CLIENT_ID, ClientSecret = CLIENT_SECRET,
                        DeviceId = Preferences.Get("UUID", ""), DeviceName = Device.RuntimePlatform
                    });
                else
                    await _api.User.AuthorizeAsync(_storage, args.Token);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Authorize: "+e);
            }

            if (_storage.IsAuthorized)
            {
                Preferences.Set("Token",_storage.Token);
                Preferences.Set("Login",_storage.User.Login);

                await ((o as Login)?.Navigation.PopModalAsync() ?? Task.CompletedTask);
                
                _ = _loader.Reload();
            }
            else
            {
                await ((o as Login) ?? MainPage).DisplayAlert("Attention!", "Error during login! Check login, password and Internet", "Ok");
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
