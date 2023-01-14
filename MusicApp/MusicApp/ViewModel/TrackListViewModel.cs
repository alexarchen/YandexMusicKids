using MusicApp.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MediaManager.Library;
using MusicApp.Framework;
using Xamarin.Forms;
using Yandex.Music.Api.Models.Album;
using Yandex.Music.Api.Models.Track;

namespace MusicApp.ViewModel
{
    public class TrackListViewModel : BaseViewModel
    {
        private readonly IMusicLoader _loader;

        public TrackListViewModel(IEnumerable<YTrack> tracks, Music album, IMusicLoader loader)
        {
            _loader = loader;
            _album = album;
            var liked = _loader.GetLikedList().ToDictionary(i => i.GetKey(), i=> true);
            musicList = new ObservableCollection<Music>(tracks.Select(t => new Music(t,liked.TryGetValue(t.GetKey(), out var l) && l)));
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

        private Music _album;
        public Music Album
        {
            get { return _album; }
            set
            {
                _album = value;
                OnPropertyChanged();
            }
        }

        private Music _selectedMusic;

        public TrackListViewModel()
        {
        }

        public Music SelectedMusic
        {
            get { return _selectedMusic; }
            set
            {
                if (value != null)
                {
                    foreach (var a in musicList)
                    {
                        if (a != value && a.IsRecent)
                            a.IsRecent = false;
                    }
                    _selectedMusic = value;
                    if (_selectedMusic != null)
                        _selectedMusic.IsRecent = true;
                }
                else
                    _selectedMusic = null;

                OnPropertyChanged();
            }
        }

        public ICommand SelectionCommand => new Command(PlayMusic);
        public ICommand ArtistTapCommand => new Command(OpenArtist);

        public ICommand LikeCommand => new Command(Like);

        private async void Like()
        {
            if (Album.IsLiked)
            {
                await _loader.LikeAlbum(Album.Base as YAlbum);
            }
            else
            {
                await _loader.DislikeAlbum(Album.Base as YAlbum);
            }

            Album.IsLiked = !Album.IsLiked;
        }

        private void PlayMusic(object music)
        {
            if (_selectedMusic != null || music!= null)
            {

                if (music == null)
                {
                    var playerPage = new PlayerPage(this, _selectedMusic, _loader);
                    var navigation = Application.Current.MainPage as NavigationPage;
                    navigation?.PushAsync(playerPage, true);
                }
                else
                {
                    _= PlayerViewModel.CreateAsync(this, music as Music, _loader);
                }

            }

            SelectedMusic = null;
        }

        public async void OpenArtist(object selectedMusicBase)
        {
            
        }
    }
}
