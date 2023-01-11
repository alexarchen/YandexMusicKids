using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MusicApp.Interfaces;
using MusicApp.Model;
using Xamarin.Forms;

namespace MusicApp.ViewModel;

public class MainPageViewModel: BaseViewModel
{
    private readonly IMusicLoader _musicLoader;
    public ObservableCollection<Music> FavoriteAlbums { get; private set; } = new();

    public MainPageViewModel(IMusicLoader musicLoader)
    {
        _musicLoader = musicLoader;
        FavoriteAlbums = new ObservableCollection<Music>(_musicLoader.GetAlbums().GroupBy(a => a.Id).Select(g => g.First()).Select(a => new Music(a)));
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
            var ids = FavoriteAlbums.Select((a, i) => (a, i)).ToDictionary(o => o.a.Id, o => o.i);

            var albums = _musicLoader.GetAlbums().GroupBy(a => a.Id).Select(g => g.First()).Select(a => new Music(a))
                .ToDictionary(a => a.Id);

            foreach (var album in albums.Values)
            {
                if (!ids.ContainsKey(album.Id))
                    FavoriteAlbums.Add(album);
            }

            //tOdo: delete from collection

            IsRefreshing = false;
            OnPropertyChanged(nameof(IsRefreshing));
        });
        // OnPropertyChanged(nameof(FavoriteAlbums));
    }
        

    public MainPageViewModel()
    {
    }
}