using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicApp.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MusicApp;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class Login : ContentPage
{
    public Login(string login, string uid)
    {
        InitializeComponent();
        
        BindingContext = new LoginModel() {
            Login = login,
            DeviceUid = uid
        };


    }
    
    public class LoginEventArgs: EventArgs
    {
        public LoginModel Login { get; set; } 
        public string Code { get; set; }
        
        public string Token { get; set; }
    }
    
    public event Action<object, LoginEventArgs> Logon;


    private void WebView_OnNavigating(object sender, WebNavigatingEventArgs e)
    {
        DisplayAlert("Nav", e.Url, "cancel");
        var qDict = System.Web.HttpUtility.ParseQueryString(new Uri(e.Url).Query);
        if (qDict?.AllKeys.Contains("code")??false)
        {
            Logon?.Invoke(e, new LoginEventArgs()
            {
                Login = BindingContext as LoginModel,
                Code = qDict["code"]
            });
        }
        if (qDict?.AllKeys.Contains("access_token")??false)
        {
            Logon?.Invoke(e, new LoginEventArgs()
            {
                Login = BindingContext as LoginModel,
                Token = qDict["access_token"]
            });
        }
    }
}
