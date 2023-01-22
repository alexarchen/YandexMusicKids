using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using MusicApp.Framework;
using Xamarin.Forms;

namespace MusicApp.ViewModel;

public class PlaylistsViewModel: BaseViewModel
{
    private readonly IMusicLoader _musicLoader;
    private bool _isRefreshing;
    public ObservableCollection<Music> Playlists { get; protected set; } = new();

    public PlaylistsViewModel(IMusicLoader musicLoader)
    {
        _musicLoader = musicLoader;
        Playlists = new ObservableCollection<Music>(_musicLoader.GetPlaylists().Select(a => new Music(a)));
        _musicLoader.Reloaded += Reloaded;
    }

    
    public ICommand RefreshCommand => new Command(Refresh);

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            _isRefreshing = value;
            OnPropertyChanged();
        }
    }

    async void Refresh()
    {
        IsRefreshing = true;
        OnPropertyChanged(nameof(IsRefreshing));
        
        if (_musicLoader != null)
            await _musicLoader.Reload();
        else
            IsRefreshing = false;
    }

 
    void Reloaded(object obj, EventArgs args)
    {
        Device.InvokeOnMainThreadAsync(() =>
        {

            Debug.WriteLine("Reload playlists");
            try
            {
                Playlists = new ObservableCollection<Music>(_musicLoader.GetPlaylists().Select(a => new Music(a)));
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Exception {e}");
            }

            OnPropertyChanged(nameof(Playlists));
            IsRefreshing = false;
        });
    }
        

    public PlaylistsViewModel()
    {
    }
    
}