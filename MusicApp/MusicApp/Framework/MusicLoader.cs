using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MusicApp.Interfaces;
using Yandex.Music.Api;
using Yandex.Music.Api.Common;
using Yandex.Music.Api.Models.Album;
using Yandex.Music.Api.Models.Artist;
using Yandex.Music.Api.Models.Track;

namespace MusicApp.Framework;

public class MusicLoader: IMusicLoader
{
    private readonly YandexMusicApi _api;
    private readonly AuthStorage _storage;
    private YArtist[] _artists = Array.Empty<YArtist>();
    private YAlbum[] _albums = Array.Empty<YAlbum>();
    private YTrack[] _liked = Array.Empty<YTrack>();

    public MusicLoader(YandexMusicApi api, Yandex.Music.Api.Common.AuthStorage storage)
    {
        _api = api;
        _storage = storage;
    }
    
    public string GetTrackUrl(string id)
    {
        return _api.Track.GetFileLink(_storage, id);
    }

    public IEnumerable<YAlbum> GetAlbums()
    {
        return _albums;
    }

    public IEnumerable<YTrack> GetLikedList()
    {
        return _liked;
    }

    public async Task Reload()
    {
        var trackids = (await _api.Library.GetLikedTracksAsync(_storage)).Result.Library.Tracks.Select(t=>t.Id).ToArray();
        _liked = (await _api.Track.GetAsync(_storage, trackids)).Result.ToArray();
        _artists = _liked.SelectMany(t => t.Artists).ToArray();
        var albumIds = (await _api.Library.GetLikedAlbumsAsync(_storage)).Result.Select(a => a.Id).ToArray();
        _albums = _liked.SelectMany(t => t.Albums).Concat((await _api.Album.GetAsync(_storage, albumIds)).Result).ToArray();

    }
}