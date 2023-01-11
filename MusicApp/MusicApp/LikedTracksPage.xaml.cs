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
public partial class LikedTracksPage : TrackListPage
{
    private IMusicLoader _musicLoader;
    public LikedTracksPage():base()
    {
        _musicLoader = (Application.Current as App)!.Loader;
        _musicLoader.Reloaded += MusicLoaderOnReloaded;
        
    }

    private static string LikeImage = "https://music.yandex.ru/blocks/playlist-cover/playlist-cover_like_2x.png";

    void CreateBindingContext()
    {
        var tracks = _musicLoader.GetLikedList();
        
        BindingContext = new TrackListViewModel(tracks, new Music()
        {
            Artist = "",
            Title = "Избранное",
            CoverImage = LikeImage,
        }, _musicLoader);

    }

    private void MusicLoaderOnReloaded(object arg1, EventArgs arg2)
    {
        CreateBindingContext();
    }
}