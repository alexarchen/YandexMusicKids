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
public partial class ArtistPage : AlbumsPage
{
    private readonly YArtist _artist;

    public ArtistPage(YArtist artist):base()
    {
        _artist = artist;
        BindingContext = new ArtistViewModel(_loader, artist);
    }
/*
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
    */
}