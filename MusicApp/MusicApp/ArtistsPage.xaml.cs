using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicApp.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Yandex.Music.Api.Models.Artist;

namespace MusicApp;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class ArtistsPage : ContentPage
{
    public ArtistsPage()
    {
        InitializeComponent();
        BindingContext = new ArtistsViewModel();
    }

    private async void SelectableItemsView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0)
        {
            if ((e.CurrentSelection.First() as Music)?.Base is YArtist artist)
            {
                await Navigation.PushAsync(new ArtistPage(artist));
            }

            (sender as CollectionView)!.SelectedItem = null;
        }
    }
}