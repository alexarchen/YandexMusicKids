using MusicApp.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MusicApp.Interfaces;
using Xamarin.Forms;
using Yandex.Music.Api.Models.Track;

namespace MusicApp.ViewModel
{
    public class TrackListViewModel : BaseViewModel
    {
        private readonly IMusicLoader _loader;

        public TrackListViewModel(IList<YTrack> tracks, IMusicLoader loader)
        {
            _loader = loader;
            musicList = new ObservableCollection<Music>(tracks.Select(t => new Music()
            {
                Artist = string.Join(",",t.Artists.Select(a=>a.Name)??Array.Empty<string>()),
                Title = t.Title,
                CoverImage = t.CoverUri,
                Id = t.Id
            }));
        }

        ObservableCollection<Music> musicList;
        public ObservableCollection<Music> MusicList
        {
            get { return musicList; }
            set
            {
                musicList = value;
                OnPropertyChanged();
            }
        }

        private Music recentMusic;
        public Music RecentMusic
        {
            get { return recentMusic; }
            set
            {
                recentMusic = value;
                OnPropertyChanged();
            }
        }

        private Music selectedMusic;
        public Music SelectedMusic
        {
            get { return selectedMusic; }
            set
            {
                selectedMusic = value;
                OnPropertyChanged();
            }
        }

        public ICommand SelectionCommand => new Command(PlayMusic);

        private void PlayMusic()
        {
            if (selectedMusic != null)
            {
                var viewModel = new PlayerViewModel(selectedMusic, musicList, _loader) ;
                var playerPage = new PlayerPage { BindingContext = viewModel };

                var navigation = Application.Current.MainPage as NavigationPage;
                navigation?.PushAsync(playerPage, true);
            }
        }

    }
}
