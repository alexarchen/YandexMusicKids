using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using MusicApp.Framework;
using Xamarin.Forms;

namespace MusicApp.ViewModel;

public class ArtistsViewModel: BaseViewModel
{
    private IMusicLoader _loader;
    
    public ObservableCollection<Music> Artists { get; protected set; } = new();

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            _isRefreshing = value; 
            OnPropertyChanged();
        }
    }
    public ICommand RefreshCommand => new Command(Refresh);
    async void Refresh()
    {
        IsRefreshing = true;
        if (_loader != null)
            await _loader.Reload();
        else
            IsRefreshing = false;
    }

    public ArtistsViewModel()
    {
        _loader = (Application.Current as App)!.Loader;
        Artists = new ObservableCollection<Music>(_loader.GetArtists().GroupBy(a => a.Id).Select(g => g.First()).Select(a => new Music(a, true)));
        _loader.Reloaded+= LoaderOnReloaded;
    }

    private bool _isRefreshing;
    private void LoaderOnReloaded(object arg1, EventArgs arg2)
    {
        Device.InvokeOnMainThreadAsync(() =>
        {

            Debug.WriteLine("Reload artists");
            try
            {
                var ids = Artists.Select((a, i) => (a, i)).ToDictionary(o => o.a.Id, o => o.i);

                var artists = _loader.GetArtists().GroupBy(a => a.Id).Select(g => g.First()).Select(a => new Music(a, true))
                    .ToDictionary(a => a.Id);

                foreach (var artist in artists.Values)
                {
                    if (!ids.ContainsKey(artist.Id))
                        Artists.Add(artist);
                }

                foreach (var artist in Artists.Where(a=>!artists.ContainsKey(a.Id)))
                {
                        Artists.Remove(artist);
                }

                IsRefreshing = false;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error {e}");
            }
        });
        
    }
}