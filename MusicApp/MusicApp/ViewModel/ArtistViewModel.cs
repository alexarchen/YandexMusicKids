using System.Collections.ObjectModel;
using System.Linq;
using MusicApp.Framework;
using Xamarin.Forms;
using Yandex.Music.Api.Models.Artist;

namespace MusicApp.ViewModel;

public class ArtistViewModel: AlbumsViewModel
{
    private readonly IMusicLoader _loader;
    private readonly YArtist _artist;

    public ArtistViewModel(IMusicLoader loader, YArtist artist)
    {
        _loader = loader;
        _artist = artist;

        Device.InvokeOnMainThreadAsync(async () =>
        {
            var favorite = loader.GetAlbums().ToDictionary(a => a.Id);
            Albums = new ObservableCollection<Music>((await loader.GetAlbumsAsync(artist)).Select(a => new Music(a, favorite.ContainsKey(a.Id))));
            OnPropertyChanged(nameof(Albums));
        });
    }
    
    
}