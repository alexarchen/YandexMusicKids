using System;
using System.Linq;
using MusicApp.Framework;
using MusicApp.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MusicApp;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class AlbumsPage : ContentPage
{
    protected IMusicLoader _loader;
    private readonly ILogger _logger;

    public AlbumsPage()
    {
        _loader = (Application.Current as App)?.Loader;
        _logger = (Application.Current as App)?.Logger;
            
        InitializeComponent();
        BindingContext = new AlbumsViewModel(_loader);
        
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
                await Navigation.PushAsync(new TrackListPage(tracks, album, _loader, _logger));
            }

            (sender as CollectionView)!.SelectedItem = null;
        }
    }

    protected override void OnAppearing()
    {
        PlayerViewModel.Stop();
    }
}