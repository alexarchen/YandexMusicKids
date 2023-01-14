using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicApp.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MusicApp;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class Login : ContentPage
{
    public Login(string login, string pass, bool remember)
    {
        InitializeComponent();
        
        BindingContext = new LoginModel() {
            Login = login,
            Password = pass,
            Remember = remember
        };

    }
    
    public class LoginEventArgs: EventArgs
    {
        public LoginModel Login { get; set; } 
    }
    
    public event Action<object, LoginEventArgs> LoginClicked;
    
    private async void LoginButton_Clicked(object sender, EventArgs e)
    {
        LoginClicked?.Invoke(this, new LoginEventArgs() { Login = BindingContext as LoginModel});
    }

}
