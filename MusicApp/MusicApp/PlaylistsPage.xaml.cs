using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicApp.Framework;
using MusicApp.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Yandex.Music.Api.Models.Playlist;

namespace MusicApp;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class PlaylistsPage : ContentPage
{
    private IMusicLoader _loader;

    public PlaylistsPage()
    {
        _loader = (Application.Current as App)?.Loader;
        InitializeComponent();
        BindingContext = new PlaylistsViewModel(_loader);
        
    }

    public PlaylistsViewModel ViewModel => BindingContext as PlaylistsViewModel;


    private async void SelectableItemsView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0)
        {

            var album = (e.CurrentSelection.First() as Music);
            if (album?.Base is YPlaylist playlist)
            {
                var tracks = await _loader.GetTracksAsync(playlist);
                await Navigation.PushAsync(new TrackListPage(tracks, album, _loader));
            }

            (sender as CollectionView)!.SelectedItem = null;
        }
    }

    protected override void OnAppearing()
    {
        PlayerViewModel.Stop();
    }
}