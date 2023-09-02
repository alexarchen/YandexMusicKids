using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Android.Media;
using Android.OS;
using Java.Lang;
using Java.Util;
using MediaManager;
using MediaManager.Library;
using MediaManager.Media;
using MediaManager.Notifications;
using MediaManager.Playback;
using MediaManager.Player;
using MediaManager.Queue;
using MediaManager.Video;
using MediaManager.Volume;
using Android = Xamarin.Forms.PlatformConfiguration.Android;
using Stream = System.IO.Stream;

namespace MusicApp.Droid;

public class Player: IMediaPlayer, IMediaManager
{
    private MediaPlayer _player;
    private MediaPlayer _nextPlayer;
    
    public void Dispose()
    {
       _player?.Dispose();
       _nextPlayer?.Dispose();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private Timer _timer = new();

    class PositionTask : TimerTask
    {
        private readonly Player _player;

        public PositionTask(Player player)
        {
            _player = player;
        }
        public override void Run()
        {
            MainActivity.Self.RunOnUiThread(() => { _player.PositionChanged?.Invoke(_player, new PositionChangedEventArgs(_player.Position));  });
        }
    }

    private PositionTask _task;
    public void Init()
    {
        _player = new MediaPlayer();
        _nextPlayer = new MediaPlayer();
        MediaPlayer = this;
        _player.Completion+= PlayerOnCompletion;
        _nextPlayer.Completion += PlayerOnCompletion;
        Queue = new MediaQueue();
        _player.Error += PlayerOnError;
    }

    private async void PlayerOnError(object sender, MediaPlayer.ErrorEventArgs e)
    {
        switch (e.What)
        {
            case MediaError.ServerDied:
                _player.Dispose();
                _nextPlayer.Dispose();
                Init();
                break;
            case MediaError.Io:
            case MediaError.TimedOut:
                if (PlayNextOnFailed)
                {
                    if (Queue.HasNext) await AutoPlayNext();
                }

                break;
        }
        
        e.Mp?.Reset();
    }

    private async void PlayerOnCompletion(object sender, EventArgs e)
    {
        if (Queue.HasNext) await AutoPlayNext();
    }

    async Task<IMediaItem> IMediaManager.Play(IMediaItem mediaItem)
    {
        var playing = Queue.Current;
        int i = Queue.MediaItems.IndexOf(mediaItem);
        if (i >= 0)
            Queue.CurrentIndex = i;
        else
        {
            Queue.Clear();
            Queue.Add(mediaItem);
            Queue.CurrentIndex = 0;
        }
        await MediaPlayer.Play(mediaItem);
        return playing;
    }

    public Task<IMediaItem> Play(string uri)
    {
        return (this as IMediaManager)!.Play(new MediaItem(uri));
    }

    public Task<IMediaItem> PlayFromAssembly(string resourceName, Assembly assembly = null)
    {
        throw new NotImplementedException();
    }

    public Task<IMediaItem> PlayFromResource(string resourceName)
    {
        throw new NotImplementedException();
    }

    public async Task<IMediaItem> Play(IEnumerable<IMediaItem> mediaItems)
    {
        Queue.Clear();
        foreach (var a in mediaItems)
        {
            Queue.Add(a);
        }

        Queue.CurrentIndex = 0;
        if (mediaItems.FirstOrDefault() != null) 
         return await (this as IMediaManager)?.Play(mediaItems.FirstOrDefault());

        return null;
    }

    public Task<IMediaItem> Play(IEnumerable<string> items)
    {
        throw new NotImplementedException();
    }

    public Task<IMediaItem> Play(FileInfo file)
    {
        throw new NotImplementedException();
    }

    public Task<IMediaItem> Play(DirectoryInfo directoryInfo)
    {
        throw new NotImplementedException();
    }

    public Task<IMediaItem> Play(Stream stream, string cacheName)
    {
        throw new NotImplementedException();
    }

    public IMediaPlayer MediaPlayer { get; set; }
    public IMediaLibrary Library { get; set; }
    public Dictionary<string, string> RequestHeaders { get; set; }
    public INotificationManager Notification { get; set; }
    public IVolumeManager Volume { get; set; }
    public IMediaExtractor Extractor { get; set; }
    public IMediaQueue Queue { get; set; } = new MediaQueue();

    async Task IMediaPlayer.Play(IMediaItem mediaItem)
    {
        StopTimer();
        _player.Reset(); 
        await _player.SetDataSourceAsync(mediaItem.MediaUri);
        await Task.Run(() =>
        {
            _player.Prepare();
            _player.Start();
        });
        
        if (Queue.HasNext)
        {
            _nextPlayer.Reset();
            await _nextPlayer.SetDataSourceAsync(Queue.Next.MediaUri);
            await Task.Run(()=>_nextPlayer.Prepare());
        }
        StartTimer();
        
        MediaItemChanged?.Invoke(this, new MediaItemEventArgs(mediaItem));
        StateChanged?.Invoke(this, new StateChangedEventArgs(MediaPlayerState.Playing));
    }

    public Task Play(IMediaItem mediaItem, TimeSpan startAt, TimeSpan? stopAt = null)
    {
        throw new NotImplementedException();
    }

    public async Task Play()
    {
        StartTimer();
        _player.Start();
        StateChanged?.Invoke(this, new StateChangedEventArgs(MediaPlayerState.Playing));
    }

    public async Task Pause()
    {
        StopTimer();
        _player.Pause();
        StateChanged?.Invoke(this, new StateChangedEventArgs(MediaPlayerState.Paused));
    }

    public async Task Stop()
    {
        StopTimer();
        _player.Stop();
        StateChanged?.Invoke(this, new StateChangedEventArgs(MediaPlayerState.Stopped));
    }

    public async Task<bool> PlayPrevious()
    {
        if (Queue.HasPrevious)
         return await PlayQueueItem(Queue.Previous);

        return false;
    }

    public async Task<bool> PlayNext()
    {
        if (Queue.HasNext)
            return await PlayQueueItem(Queue.Next);

        return false;
    }

    void StopTimer()
    {
        if (_task != null)
        {
            _task.Cancel();
            _timer.Purge();
            _task.Dispose();
            _task = null;
        }
    }

    void StartTimer()
    {
        StopTimer();
        _task = new PositionTask(this);
        _timer.ScheduleAtFixedRate(_task, 0, 1000);
    }
    public async Task AutoPlayNext()
    {
        if (Queue.HasNext)
        {
            StopTimer();
            _nextPlayer.Start();
            
            (_player, _nextPlayer) = (_nextPlayer, _player);

            StartTimer();
            
            Queue.CurrentIndex++;
            MediaItemChanged?.Invoke(this, new MediaItemEventArgs(Queue.Current));

            if (Queue.HasNext)
            {
                // todo await Task Run 
                _nextPlayer.Reset();
                await _nextPlayer.SetDataSourceAsync(Queue.Next.MediaUri);
                _nextPlayer.Prepare();
            }
        }
    }

    public async Task<bool> PlayQueueItem(IMediaItem mediaItem)
    {
        Queue.CurrentIndex = Queue.IndexOf(mediaItem);
        await MediaPlayer.Play(mediaItem);
        
        MediaItemChanged?.Invoke(this, new MediaItemEventArgs(Queue.Current));
        return true;
    }

    public async Task<bool> PlayQueueItem(int index)
    {
        Queue.CurrentIndex = index;
        await MediaPlayer.Play(Queue.Current);
        MediaItemChanged?.Invoke(this, new MediaItemEventArgs(Queue.Current));
        return true;
    }

    public Task StepForward()
    {
        throw new NotImplementedException();
    }

    public Task StepBackward()
    {
        throw new NotImplementedException();
    }

    public async Task SeekTo(TimeSpan position)
    {
        _player.SeekTo((int)position.TotalMilliseconds);
    }

    public TimeSpan StepSize { get; set; }
    public MediaPlayerState State { get; }
    public TimeSpan Position => TimeSpan.FromMilliseconds(_player?.CurrentPosition ?? 0);
    public TimeSpan Duration => TimeSpan.FromMilliseconds(_player.Duration);
    public TimeSpan Buffered { get; }
    public float Speed { get => 1;
        set => throw new NotImplementedException();
    }
    public RepeatMode RepeatMode { get; set; }
    public ShuffleMode ShuffleMode { get; set; }
    public bool ClearQueueOnPlay { get; set; }
    public bool AutoPlay { get; set; }
    public bool KeepScreenOn { get; set; }
    public bool RetryPlayOnFailed { get; set; }
    public bool PlayNextOnFailed { get; set; } = true;
    public int MaxRetryCount { get; set; }
    public event StateChangedEventHandler StateChanged;
    public event BufferedChangedEventHandler BufferedChanged;
    public event PositionChangedEventHandler PositionChanged;
    public event MediaItemFinishedEventHandler MediaItemFinished;
    public event MediaItemChangedEventHandler MediaItemChanged;
    public event MediaItemFailedEventHandler MediaItemFailed;

    public IVideoView VideoView { get; set; }
    public bool AutoAttachVideoView { get; set; }
    public VideoAspectMode VideoAspect { get; set; }
    public bool ShowPlaybackControls { get; set; }
    public int VideoWidth { get; }
    public int VideoHeight { get; }
    public float VideoAspectRatio { get; }
    public object VideoPlaceholder { get; set; }
    public event BeforePlayingEventHandler BeforePlaying;
    public event AfterPlayingEventHandler AfterPlaying;
}