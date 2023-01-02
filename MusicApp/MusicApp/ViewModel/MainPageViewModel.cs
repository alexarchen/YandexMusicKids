using System;
using System.Collections.ObjectModel;
using System.Linq;
using MusicApp.Interfaces;
using MusicApp.Model;

namespace MusicApp.ViewModel;

public class MainPageViewModel: BaseViewModel
{
    public ObservableCollection<Music> FavoriteAlbums { get; }

    public MainPageViewModel(IMusicLoader musicLoader)
    {
        FavoriteAlbums = new ObservableCollection<Music>(musicLoader.GetAlbums().Select(a=>new Music()
        {
            Id = a.Id, Artist = string.Join(",",a.Artists?.Select(aa=>aa.Name) ?? Array.Empty<string>()), CoverImage = a.CoverUri
        }));
    }
    
}