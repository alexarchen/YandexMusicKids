using System;
using System.Text.Encodings.Web;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Webkit;
using Java.Interop;
using Java.Net;
using MusicApp.ViewModel;

namespace MusicApp.Droid
{
    /*
    <intent-filter android:autoVerify="true">
        <action android:name="android.intent.action.VIEW" />

        <category android:name="android.intent.category.DEFAULT" />
        <category android:name="android.intent.category.BROWSABLE" />

        <data
    android:host="yx${YANDEX_CLIENT_ID}.${YANDEX_OAUTH_HOST}"
    android:path="/auth/finish"
    android:scheme="https" />
        </intent-filter>
        */
    
    [Activity(Label = "Login", Exported = true)]
    [IntentFilter(new [] { Intent.ActionView }, AutoVerify = true,
        Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault },
        DataHost = $"yx{App.CLIENT_ID}.oauth.yandex.ru",
        DataScheme = "https",
        DataPath = "/auth/finish")]
    public class LoginActivity: Activity
    {
        public const string EXTRA_LOGIN_INTENT = "com.alexarchen.EXTRA.Login";
        public const string EXTRA_TOKEN = "com.alexarchen.EXTRA.Token";

//        public const string RedirectUrl = $"https://yx{App.CLIENT_ID}.oauth.yandex.ru/auth/finish?platform=android"; //$"https://yx{App.CLIENT_ID}/finish?platform=android";//
        public const string RedirectUrl = "https://music.yandex.ru"; 
            
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var login = Intent?.GetStringExtra(EXTRA_LOGIN_INTENT);
            var state = "1";
            var redirect = UrlEncoder.Create().Encode(RedirectUrl);
            string loginUrl = $"https://oauth.yandex.ru/authorize?response_type=token&client_id={App.CLIENT_ID}";
            
            Android.Webkit.CookieManager.Instance?.RemoveAllCookies(null);
            Android.Webkit.CookieManager.Instance?.Flush();
            
            WebView webView = new WebView(this);
            webView.SetWebViewClient(new WebViewClient(this));
            webView.Settings.UserAgentString = "Mozilla/5.0 (X11; Linux x86_64; rv:109.0) Gecko/20100101 Firefox/111.0";
            webView.LoadUrl(loginUrl);
            webView.Settings.JavaScriptEnabled = true;
            Log.Info("Login",$"Loading: {loginUrl}");

            SetContentView(webView);
        }

        private void ParseTokenFromUrl(string url)
        {
            Android.Net.Uri data = Android.Net.Uri.Parse(url); 
            String fragment = data.Fragment;
            Android.Net.Uri dummyUri = Android.Net.Uri.Parse("dummy://dummy?" + fragment);

            Intent result = new Intent();

            String token = dummyUri.GetQueryParameter("access_token");
            String expiresInString = dummyUri.GetQueryParameter("expires_in"); 
            result.PutExtra(EXTRA_TOKEN, token);
            SetResult(Result.Ok, result);
            Finish();
        }

        private class WebViewClient : Android.Webkit.WebViewClient
        {
            private LoginActivity _loginActivity;

            public WebViewClient(LoginActivity loginActivity)
            {
                _loginActivity = loginActivity;
            }

            public override void OnPageStarted(WebView view, string url, Bitmap favicon)
            {
                Log.Info("Login",$"Loading page: {url}");
                
                if (url.StartsWith(RedirectUrl)) 
                {
                    _loginActivity.ParseTokenFromUrl(url);
                }
                else
                {
                    base.OnPageStarted(view, url, favicon);
                }
                
            }

        }
    }
}