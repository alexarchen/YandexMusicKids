using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using MusicApp.Framework;
using Xamarin.Forms;

namespace MusicApp.ViewModel;

public class RecommendsViewModel: AlbumsViewModel
{
    
    public RecommendsViewModel( IMusicLoader musicLoader) : base()
    {
        _musicLoader = musicLoader;
        Albums = new ObservableCollection<Music>(_musicLoader.GetAlbums().GroupBy(a => a.Id).Select(g => g.First()).Select(a => new Music(a, true)));
        _musicLoader.Reloaded += MusicLoaderOnReloaded;
    }

    private void MusicLoaderOnReloaded(object arg1, EventArgs arg2)
    {
        Device.InvokeOnMainThreadAsync(() =>
        {

            Debug.WriteLine("Reload albums");
            try
            {
                var ids = Albums.Select((a, i) => (a, i)).ToDictionary(o => o.a.Id, o => o.i);

                var albums = _musicLoader.GetAllAlbums().GroupBy(a => a.Id).Select(g => g.First()).Select(a => new Music(a, true))
                    .ToDictionary(a => a.Id);

                foreach (var album in albums.Values)
                {
                    if (!ids.ContainsKey(album.Id))
                        Albums.Add(album);
                }

                //tOdo: delete from collection

                IsRefreshing = false;
                OnPropertyChanged(nameof(IsRefreshing));
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error {e}");
            }
        });
    }
}