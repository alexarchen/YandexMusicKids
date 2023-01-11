using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Yandex.Music.Api;
using Yandex.Music.Api.Common;
using Yandex.Music.Api.Models.Account;
using Yandex.Music.Api.Models.Album;
using Yandex.Music.Api.Models.Artist;
using Yandex.Music.Api.Models.Track;

namespace MusicApp.Framework;

public class MusicLoader: IMusicLoader
{
    private readonly YandexMusicApi _api;
    private readonly AuthStorage _storage;
    private readonly App _app;
    private readonly string _cacheFileName;
    private List<YArtist> _artists = new();
    private List<YAlbum> _albums = new();
    private List<YTrack> _liked = new();
    private YAlbum[] _allAlbums = Array.Empty<YAlbum>();

    public MusicLoader(YandexMusicApi api, AuthStorage storage, App app, string cacheFileName)
    {
        _api = api;
        _storage = storage;
        _app = app;
        _cacheFileName = cacheFileName;
        LoadFromFile();
    }

    internal struct StoreObject
    {
        public YAlbum[] Albums;
        public YTrack[] Liked;
        public YArtist[] Artists;
    }

    public void LoadFromFile()
    {
        if ((_cacheFileName != null) && File.Exists(_cacheFileName))
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<StoreObject>(File.ReadAllText(_cacheFileName));
                _liked = obj.Liked.ToList();
                _albums = obj.Albums.ToList();
                _artists = obj.Artists.ToList();
                
                Debug.WriteLine("Loaded from cache");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error loading cache: {e}");
            }

        }
    }

    public async Task SaveToFileAsync()
    {
        try
        {
            if (_cacheFileName != null)
                await File.WriteAllTextAsync(_cacheFileName,
                    JsonConvert.SerializeObject(new StoreObject()
                    {
                        Artists = _artists.ToArray(),
                        Albums = _albums.ToArray(),
                        Liked = _liked.ToArray()
                    }, new JsonSerializerSettings(){ ContractResolver = new CamelCasePropertyNamesContractResolver()}));
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Error saving cache: {e}");
        }
        
    }
    public async Task<string> GetTrackUrl(YTrack track)
    {
        return await _api.Track.GetFileLinkAsync(_storage, track.GetKey().ToString());
    }

    public IEnumerable<YAlbum> GetAlbums()
    {
        return _albums;
    }

    public IEnumerable<YTrack> GetLikedList()
    {
        return _liked;
    }

    public async Task LikeTrack(YTrack t)
    {
        await _api.Library.AddTrackLikeAsync(_storage, t);
        var was = _liked.FirstOrDefault(tr=>tr.Id == t.Id);
        if (was==null) _liked.Add(t);

        Reloaded?.Invoke(this, new EventArgs() { });
    }

    public async Task DislikeTrack(YTrack t)
    {
        await _api.Library.RemoveTrackLikeAsync(_storage, t);
        var was = _liked.FirstOrDefault(tr=>tr.Id == t.Id);
        if (was!=null) _liked.Remove(was);
        
        Reloaded?.Invoke(this, new EventArgs() { });
    }

    public async Task Reload()
    {
        if (_storage.IsAuthorized)
        {
            try
            {
                var trackIds = (await _api.Library.GetLikedTracksAsync(_storage)).Result.Library.Tracks.Select(t => t.Id).ToArray();
                _liked = (await _api.Track.GetAsync(_storage, trackIds)).Result.ToList();
                _artists = (await _api.Library.GetLikedArtistsAsync(_storage)).Result.ToList();
                var albumIds = (await _api.Library.GetLikedAlbumsAsync(_storage)).Result.Select(a => a.Id).ToArray();
                _albums = (await _api.Album.GetAsync(_storage, albumIds)).Result.ToList();
                var allAlbumIds = (await _api.Library.GetLikedAlbumsAsync(_storage)).Result.Select(a => a.Id)
                    .Concat(_liked.SelectMany(l => l.Albums.Select(a => a.Id))).Distinct().ToArray();
                _allAlbums = (await _api.Album.GetAsync(_storage, allAlbumIds)).Result.ToArray();
            }
            catch (Exception e)
            {
              Debug.WriteLine($"Error reload: {e}");  
            }
        }
        else
        {
            _artists = new();
            _albums = new();
            _liked = new();
            _allAlbums = Array.Empty<YAlbum>();
        }

        await SaveToFileAsync();
        Reloaded?.Invoke(this, new EventArgs() { });
    }

    private async Task ReloadTracks()
    {
        try
        {
            var trackIds = (await _api.Library.GetLikedTracksAsync(_storage)).Result.Library.Tracks.Select(t => t.Id).ToArray();
            _liked = (await _api.Track.GetAsync(_storage, trackIds)).Result.ToList();
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Error reload tracks: {e}");  
        }

        await SaveToFileAsync();
        Reloaded?.Invoke(this, new EventArgs() { });
    }

    private async Task ReloadAlbums()
    {
        try
        {
            var albumIds = (await _api.Library.GetLikedAlbumsAsync(_storage)).Result.Select(a => a.Id).ToArray();
            _albums = (await _api.Album.GetAsync(_storage, albumIds)).Result.ToList();
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Error reload albums: {e}");  
        }

        await SaveToFileAsync();
        Reloaded?.Invoke(this, new EventArgs() { });
    }
    
    public event Action<object, EventArgs> Reloaded;
    public async Task<IEnumerable<YTrack>> GetTracksAsync(string albumId)
    {
        var album =  (await _api.Album.GetAsync(_storage, albumId))?.Result;
        return album?.Volumes.SelectMany(v=>v.AsEnumerable()).GroupBy(t=>t.Id).Select(g=>g.First()) ?? Array.Empty<YTrack>();
    }

    public YAccount GetAccount()
    {
        return _storage.User;
    }

    public void Logout()
    {
        _artists = new();
        _albums = new();
        _liked = new();
        _allAlbums = Array.Empty<YAlbum>();
        _storage.Logout();
        _app.Logout();
        Reloaded?.Invoke(this, new EventArgs() { });
    }

    public IEnumerable<YAlbum> GetAllAlbums()
    {
        return _allAlbums;
    }

    public IEnumerable<YArtist> GetArtists()
    {
        return _artists;
    }

    public async Task<IEnumerable<YAlbum>> GetAlbumsAsync(YArtist artist)
    {
        return (await _api.Artist.GetAlbumsAsync(_storage, artist.Id)).Result.Albums.ToArray();
    }

    public async Task LikeAlbum(YAlbum albumBase)
    {
        await _api.Library.AddAlbumLikeAsync(_storage, albumBase);
        var was = _albums.FirstOrDefault(a=>a.Id == albumBase.Id);
        if (was==null) _albums.Add(albumBase);
        
        Reloaded?.Invoke(this, new EventArgs() { });
    }
    
    public async Task DislikeAlbum(YAlbum albumBase)
    {
        await _api.Library.RemoveAlbumLikeAsync(_storage, albumBase);
        var was = _albums.FirstOrDefault(a=>a.Id == albumBase.Id);
        if (was!=null) _albums.Remove(was);
        
        Reloaded?.Invoke(this, new EventArgs() { });
    }
    
}