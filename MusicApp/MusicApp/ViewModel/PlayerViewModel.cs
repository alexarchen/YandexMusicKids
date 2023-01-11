using MediaManager;
using MusicApp.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MediaManager.Library;
using MediaManager.Media;
using MediaManager.Playback;
using MediaManager.Player;
using MusicApp.Framework;
using Xamarin.Essentials;
using Xamarin.Forms;
using Yandex.Music.Api.Models.Track;

namespace MusicApp.ViewModel
{
    public class PlayerViewModel : BaseViewModel, IAsyncDisposable
    {
        private IMusicLoader _loader;
        private IMediaManager MediaManager {get; set; } = CrossMediaManager.Current;
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private bool _bLoaded;

        public static PlayerViewModel LastPlayerModel;
        TrackListViewModel _trackListViewModel;
        private Task _queueTask;


        Music FindPrevMusic(Music music)
        {
            if (_musicList.IndexOf(music) is var i and > 0)
                return _musicList[i - 1];
            return null;
        }

        Music FindNextMusic(Music music)
        {
            if ((_musicList.IndexOf(music) is var i ) && (i<_musicList.Count-1))
                return _musicList[i + 1];
            return null;
        }

        /// <summary>
        /// Global Stop playing
        /// </summary>
        public static void Stop()
        {
            if (LastPlayerModel != null)
            {
                LastPlayerModel.MediaManager.Stop();
                LastPlayerModel.Dispose();
                LastPlayerModel = null;
            }
        }
            
        public static async Task<PlayerViewModel> CreateAsync(TrackListViewModel trackListViewModel, Music music, IMusicLoader loader)
        {
            var model = new PlayerViewModel(trackListViewModel, loader);
            
            var next = model.FindNextMusic(music);
            var prev = model.FindPrevMusic(music);
            List<Music> tracks = new List<Music>();
            //if (prev!=null) tracks.Add(prev);
            tracks.Add(music);
            if (next!=null) tracks.Add(next);
            model.SelectedMusic = music;

            List<MediaItem> queue = new List<MediaItem>();
            
            foreach (var item in tracks)
            {
                if (item.Url == null)
                {
                    item.Url = await model._loader.GetTrackUrl(item.Base as YTrack);
                    if (item.Url==null) return
                    null;
                }
                queue.Add(new MediaItem()
                {
                    Id = item.Id,
                    Extras = item,
                    MediaUri = item.Url
                });
                Debug.WriteLine($"Added {item.Id} to queue");
            }

            await model.MediaManager.Play(queue);//PlayMusic(model._selectedMusic);
            if (prev != null)
            {
                if (prev.Url==null) prev.Url = await model._loader.GetTrackUrl(prev.Base as YTrack);

                var tsc = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                try
                {
                    await Task.Run(async () =>
                    {
                        while (!model._isPlaying)
                        {
                            await Task.Delay(100, tsc.Token);
                        }
                    });
                }
                catch (TaskCanceledException e)
                {
                    
                }

                model.MediaManager.Queue.Insert(0, new MediaItem() { MediaUri = prev.Url, Id = prev.Id });
            }

            model._bLoaded = true;
            return model;
        }

        public PlayerViewModel(TrackListViewModel trackListViewModel, IMusicLoader loader)
        {
            Debug.WriteLine("Creating PlayerViewModel...");
            if (LastPlayerModel != null)
            {
                Debug.WriteLine("Disposing PlayerViewModel...");
                LastPlayerModel.Dispose();
                LastPlayerModel.MediaManager = null;
            }

            LastPlayerModel = this;
            MediaManager.Stop();
            
            _trackListViewModel = trackListViewModel;
            _musicList = trackListViewModel.MusicList;
            _selectedMusic = trackListViewModel.SelectedMusic;
            _loader = loader;
            Duration = default;
            Maximum = 1f;
            Position = 0;
            PositionSpan = default;

            MediaManager.Queue.Clear();
            MediaManager.Queue.CurrentIndex = 0;
            MediaManager.RetryPlayOnFailed = true;
            MediaManager.MaxRetryCount = 3;
            MediaManager.MediaItemChanged+= MediaManagerOnMediaItemChanged;
            MediaManager.PositionChanged+= MediaManagerOnPositionChanged;
            MediaManager.StateChanged+= MediaManagerOnStateChanged;
            
        }
        

        private void MediaManagerOnStateChanged(object sender, StateChangedEventArgs e)
        {
            Debug.WriteLine($"StateChanged: {Enum.GetName(typeof(MediaPlayerState), e.State)}");
            IsPlaying = e.State == MediaPlayerState.Playing || (e.State != MediaPlayerState.Paused && e.State != MediaPlayerState.Stopped && _isPlaying);
        }

        private void MediaManagerOnPositionChanged(object sender, MediaManager.Playback.PositionChangedEventArgs e)
        {
            Duration = TimeSpan.FromSeconds(Math.Round(MediaManager.Duration.TotalSeconds>=1 ? MediaManager.Duration.TotalSeconds : 1, 2));
            Maximum = Math.Round(Duration.TotalSeconds,2);
            var p = Math.Round(e.Position <= Duration ? e.Position.TotalSeconds : Duration.TotalSeconds, 1);
            if (Math.Abs(p - _position) > 1)
            {
                
                Position = p;
                PositionSpan = e.Position;
                OnPropertyChanged(nameof(PositionSpan));
            }
        }
        

        private async void MediaManagerOnMediaItemChanged(object sender, MediaItemEventArgs e)
        {
            Debug.WriteLine($"Music changed event: {e.MediaItem?.Id}");
            
            if (e.MediaItem != null)
            {
                SelectedMusic = _musicList.FirstOrDefault(l => l.Id == e.MediaItem.Id);
                var next = FindNextMusic(SelectedMusic);
                if (next != null)
                {
                    if (MediaManager.Queue.MediaItems.IndexOf(e.MediaItem) == MediaManager.Queue.Count - 1)
                    {
                        if (next.Url == null) next.Url = await _loader.GetTrackUrl(next.Base as YTrack);
                        Debug.WriteLine($"Added {next.Id} to queue");
                        MediaManager.Queue.Add(new MediaItem()
                        {
                            Id = next.Id,
                            MediaUri = next.Url
                        });
                    }
                }
            }
        }

        #region Properties
        ObservableCollection<Music> _musicList;
        public ObservableCollection<Music> MusicList
        {
            get => _musicList;
            set
            {
                _musicList = value;
                OnPropertyChanged();
            }
        }

        private Music _selectedMusic;
        public Music SelectedMusic
        {
            get => _selectedMusic;
            set
            {
                _selectedMusic = value;
                _trackListViewModel.SelectedMusic = _selectedMusic;
                OnPropertyChanged();
            }
        }

        private TimeSpan _duration;
        public TimeSpan Duration
        {
            get { return _duration; }
            set
            {
                if (_duration != value)
                {
                    _duration = value;
                    Debug.WriteLine($"Duration changed: {value}");
                    OnPropertyChanged();
                }
            }
        }

        public TimeSpan PositionSpan { get; set; }

        private double _position;
        public double Position
        {
            get => _position;
            set
            {
                _position = value;
                OnPropertyChanged();
            }
        }

        public void SetPosition(double position)
        {
            MediaManager.MediaPlayer.SeekTo(TimeSpan.FromSeconds(position));
        }

        double _maximum = 100f;
        public double Maximum
        {
            get { return _maximum; }
            set
            {
                if ((value > 0) && (_maximum!=value))
                {
                    _maximum = value;
                    Debug.WriteLine($"Maximum changed: {value}");
                    OnPropertyChanged(); 
                }
            }
        }


        private bool _isPlaying;

        public PlayerViewModel()
        {
            Debug.WriteLine("Creating EMPTY PlayerViewModel...");

            Maximum = 100;
            Position = 50;
            PositionSpan = TimeSpan.FromSeconds(50);
            Duration = TimeSpan.FromSeconds(125);
            _selectedMusic = LastPlayerModel?._selectedMusic;

        }

        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                _isPlaying = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PlayIcon));
            }
        }

        public string PlayIcon { get => _isPlaying ? "pause.png" : "play.png"; }

        #endregion

        public ICommand PlayCommand => new Command(Play);
        public ICommand ChangeCommand => new Command(ChangeMusic);
        public ICommand BackCommand => new Command(() => Application.Current.MainPage.Navigation.PopAsync());
        public ICommand ShareCommand => new Command(() => Share.RequestAsync(_selectedMusic.Url, _selectedMusic.Title));

        public ICommand PositionsCommand => new Command(() => {  });

        public ICommand LikeCommand => new Command(Like);

        public ICommand ArtistTapCommand => new Command(OpenArtist);


        private async void OpenArtist()
        {
            Stop();
            await Application.Current.MainPage.Navigation.PopAsync();
            _trackListViewModel.OpenArtist(_selectedMusic.Base as YTrack);
        }
        private async void Like()
        {
            if (_selectedMusic.IsLiked)
            {
                await _loader.DislikeTrack(_selectedMusic.Base as YTrack);
            }
            else
            {
                await _loader.LikeTrack(_selectedMusic.Base as YTrack);
            }
            
            _selectedMusic.IsLiked = !_selectedMusic.IsLiked;
        }

        private async void Play()
        {
                if (_isPlaying)
                {
                    await MediaManager.Pause();
                    IsPlaying = false;
                }
                else
                {
                    await MediaManager.Play();
                    IsPlaying = true;
                }
        }
        
        private void ChangeMusic(object obj)
        {
            if (_bLoaded)
            {

                if ((string)obj == "P")
                    PreviousMusic();
                else if ((string)obj == "N")
                    NextMusic();
            }
        }

        private async Task PlayMusic(Music music)
        {
            Debug.WriteLine($"Start playing music {music.Id}");
            
            try
            {
                if (music.Url == null)
                    music.Url = await _loader.GetTrackUrl(music?.Base as YTrack);

                if (MediaManager != null) await MediaManager.Pause();
                if (MediaManager != null) 
                    if (!await MediaManager.PlayQueueItem(MediaManager.Queue.FirstOrDefault(i => i.Id == music.Id)))
                        Debug.WriteLine($"PlayQueueItem {music.Id} returns false");
                //if (MediaManager != null) await MediaManager.Play();
                IsPlaying = true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error play: "+e.Message);
            }
            /*
            Device.StartTimer(TimeSpan.FromMilliseconds(500), () => 
            {
                Duration = mediaInfo.Duration;
                Maximum = duration.TotalSeconds;
                Position = mediaInfo.Position;
                return true;
            });*/
        }

        private void NextMusic()
        {
            /*
            var currentIndex = musicList.IndexOf(selectedMusic);

            if (currentIndex < musicList.Count - 1)
            {
                SelectedMusic = musicList[currentIndex + 1];
                PlayMusic(selectedMusic);
            }*/
            if (_bLoaded)
                MediaManager.PlayNext();
        }

        private void PreviousMusic()
        {
            /*
            var currentIndex = musicList.IndexOf(selectedMusic);

            if (currentIndex > 0)
            {
                SelectedMusic = musicList[currentIndex - 1];
                PlayMusic(selectedMusic);
            } */

            if (_bLoaded)
            {
                var prev = FindPrevMusic(_selectedMusic);
                if (prev != null)
                {
                    if (prev.Url == null) prev.Url = _loader.GetTrackUrl(prev.Base as YTrack).Result;
                    
                    if (MediaManager.Queue.Previous == null)
                    {
                        var pprev = FindPrevMusic(prev);
                        if (pprev is { Url: null })
                            pprev.Url = _loader.GetTrackUrl(pprev.Base as YTrack).Result;
                        Debug.WriteLine($"Insert {prev.Id} in queue");
                        MediaManager.Queue.Insert(0,new MediaItem()
                        {
                            Id = prev.Id,
                            MediaUri = prev.Url
                        });
                    }
                    MediaManager.PlayPrevious();
                }
            }
        }

        public void Dispose()
        {
            MediaManager.MediaItemChanged-= MediaManagerOnMediaItemChanged;
            MediaManager.PositionChanged-= MediaManagerOnPositionChanged;
            MediaManager.StateChanged -= MediaManagerOnStateChanged;
            _tokenSource.Cancel();
        }

        public async ValueTask DisposeAsync()
        {
            MediaManager.MediaItemChanged-= MediaManagerOnMediaItemChanged;
            MediaManager.PositionChanged-= MediaManagerOnPositionChanged;
            MediaManager.StateChanged -= MediaManagerOnStateChanged;
            _tokenSource.Cancel();
            try
            {
                await _queueTask;
            }
            catch (TaskCanceledException e)
            {
                
            }
        }
    }
}
