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
using MediaManager.Queue;
using MusicApp.Framework;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Yandex.Music.Api.Models.Track;

namespace MusicApp.ViewModel
{
    public class PlayerViewModel : BaseViewModel, IDisposable, IAsyncDisposable
    {
        private readonly IMusicLoader _loader;
        private IMediaManager MediaManager {get; set; } = CrossMediaManager.Current;
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private bool _bLoaded;

        public static PlayerViewModel LastPlayerModel;
        readonly TrackListViewModel _trackListViewModel;
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

            List<IMediaItem> queue = new List<IMediaItem>();
            
            foreach (var item in tracks)
            {
                if (item.Url == null)
                {
                    item.Url = await model._loader.GetTrackUrl(item.Base as YTrack);
                    if (item.Url==null) return
                    null;
                }
                queue.Add(FromMusic(item));
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
                            await Task.Delay(50, tsc.Token);
                        }
                    }, tsc.Token);
                }
                catch (TaskCanceledException e)
                {
                    
                }

                if (!tsc.IsCancellationRequested)
                {
                    model.MediaManager.Queue.MediaItems.Insert(0,FromMusic(prev));
                    model.MediaManager.Queue.CurrentIndex++;
                    
                    Debug.WriteLine($"Insert media item {prev.Id}");
                }
                
            }

            model._bLoaded = true;
            return model;
        }

        private static IMediaItem FromMusic(Music track)
        {
            return new MediaItem()
            {
                MediaUri = track.Url,
                Id = track.Id,
                Extras = track,
                Artist = track.Artist,
                Title = track.Title,
                ImageUri = track.CoverImage
            };
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
            
//            Debug.WriteLine($"Dump player state: {MediaManager.Queue.CurrentIndex} {MediaManager.Queue.Current?.Id} {MediaManager.Queue.Current?.Title}");
            
            if (IsPlaying && (MediaManager.Queue.Current?.Id != SelectedMusic.Id))
            {
                Debug.WriteLine($"Media item wasn't changed in UI");
                MediaManagerOnMediaItemChanged(this, new MediaItemEventArgs(MediaManager.Queue.Current));
            }
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
                        MediaManager.Queue.MediaItems.Add(FromMusic(next));
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

        public ICommand AlbumTapCommand => new Command(() => { });

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
        /*

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
        }
    */

        private void NextMusic()
        {
            if (_bLoaded)
                MediaManager.PlayNext();
        }

        private async void PreviousMusic()
        {

            if (_bLoaded)
            {
                var prev = FindPrevMusic(_selectedMusic);
                if (prev != null)
                {
                    if (prev.Url == null) prev.Url = await _loader.GetTrackUrl(prev.Base as YTrack);
                    
                    if (MediaManager.Queue.Previous == null)
                    {
                        var pprev = FindPrevMusic(prev);
                        if (pprev is { Url: null })
                            pprev.Url = await _loader.GetTrackUrl(pprev.Base as YTrack);
                        Debug.WriteLine($"Insert {prev.Id} in queue");
                        MediaManager.Queue.Insert(0,FromMusic(prev));
                        MediaManager.Queue.CurrentIndex++;
                        
                    }
                    await MediaManager.PlayPrevious();
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
