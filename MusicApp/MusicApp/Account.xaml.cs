using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicApp.Framework;
using MusicApp.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MusicApp;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class Account : ContentPage
{
    public Account(IMusicLoader loader)
    {
        InitializeComponent();
        BindingContext = new AccountViewModel(loader);
    }

    public Account()
    {
        InitializeComponent();
        BindingContext = new AccountViewModel((Application.Current as App)?.Loader);
    }

    private void Button_OnClicked(object sender, EventArgs e)
    {
//       Navigation.PopAsync();
       (BindingContext as AccountViewModel)!.Logout();
    }
}