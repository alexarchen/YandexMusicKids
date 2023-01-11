using System;
using System.Linq;
using MusicApp.Interfaces;
using MusicApp.Model;
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

        InitializeComponent();
        
        BindingContext = new AlbumsViewModel(loader);
    }

    public AlbumsViewModel ViewModel => BindingContext as AlbumsViewModel;


    private async void SelectableItemsView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0)
        {
            var album = (e.CurrentSelection.First() as Music);
            var albumId = album?.Id;
            if (!string.IsNullOrEmpty(albumId))
            {
                var tracks = await _loader.GetTracksAsync(albumId);
                await Navigation.PushAsync(new TrackListPage(tracks, album, _loader));
            }

            (sender as CollectionView)!.SelectedItem = null;
        }
    }

    private async void Tracks_OnClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new TrackListPage(_loader.GetLikedList(), new Music()
        {
            Artist = "",
            CoverImage = "play.png",
        }, _loader));
    }

    private void AccountButton_OnClicked(object sender, EventArgs e)
    {
         Navigation.PushAsync(new Account(_loader));
    }

    protected override void OnAppearing()
    {
        PlayerViewModel.Stop();
    }
}