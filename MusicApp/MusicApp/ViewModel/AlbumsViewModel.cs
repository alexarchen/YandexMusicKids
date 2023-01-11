using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MusicApp.Framework;
using MusicApp.Model;
using Xamarin.Forms;

namespace MusicApp.ViewModel;

public class AlbumsViewModel: BaseViewModel
{
    protected IMusicLoader _musicLoader;
    public ObservableCollection<Music> Albums { get; protected set; } = new();

    public AlbumsViewModel(IMusicLoader musicLoader)
    {
        _musicLoader = musicLoader;
        Albums = new ObservableCollection<Music>(_musicLoader.GetAlbums().GroupBy(a => a.Id).Select(g => g.First()).Select(a => new Music(a, true)));
        _musicLoader.Reloaded += Reloaded;
    }

    
    public ICommand RefreshCommand => new Command(Refresh);
    public bool IsRefreshing { get; set; }= false;

    async void Refresh()
    {
        IsRefreshing = true;
        if (_musicLoader != null)
            await _musicLoader.Reload();
        else
            IsRefreshing = false;
        OnPropertyChanged(nameof(IsRefreshing));
    }

 
    void Reloaded(object obj, EventArgs args)
    {
        Device.InvokeOnMainThreadAsync(() =>
        {

            Debug.WriteLine("Reload albums");
            try
            {
                var ids = Albums.Select((a, i) => (a, i)).ToDictionary(o => o.a.Id, o => o.i);

                var albums = _musicLoader.GetAlbums().GroupBy(a => a.Id).Select(g => g.First()).Select(a => new Music(a, true))
                    .ToDictionary(a => a.Id);

                foreach (var album in albums.Values)
                {
                    if (!ids.ContainsKey(album.Id))
                        Albums.Add(album);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Exception {e}");
            }

            //tOdo: delete from collection

            IsRefreshing = false;
            OnPropertyChanged(nameof(IsRefreshing));
        });
        // OnPropertyChanged(nameof(Albums));
    }
        

    public AlbumsViewModel()
    {
    }
}