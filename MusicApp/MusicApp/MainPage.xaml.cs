using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicApp.Interfaces;
using MusicApp.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MusicApp;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class MainPage : ContentPage
{
    private readonly IMusicLoader _loader;

    public MainPage(IMusicLoader loader)
    {
        _loader = loader;

        BindingContext = new MainPageViewModel(loader);
        
        InitializeComponent();
    }
}