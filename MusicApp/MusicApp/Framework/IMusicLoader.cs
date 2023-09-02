using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MusicApp.ViewModel;
using Yandex.Music.Api.Models.Account;
using Yandex.Music.Api.Models.Album;
using Yandex.Music.Api.Models.Artist;
using Yandex.Music.Api.Models.Playlist;
using Yandex.Music.Api.Models.Track;

namespace MusicApp.Framework;

public interface IMusicLoader
{
    Task<string> GetTrackUrl(YTrack track);

    IEnumerable<YAlbum> GetAlbums();

    IEnumerable<YTrack> GetLikedList();

    IEnumerable<YPlaylist> GetPlaylists();

    Task LikeTrack(YTrack t);

    Task DislikeTrack(YTrack t);
    
    Task Reload();
    
    event Action<object, EventArgs> Reloaded;

    Task<IEnumerable<YTrack>> GetTracksAsync(string albumId);
    YAccount GetAccount();

    void Logout();

    IEnumerable<YAlbum> GetAllAlbums();
    IEnumerable<YArtist> GetArtists();

    Task<IEnumerable<YAlbum>> GetAlbumsAsync(YArtist artist);
    Task LikeAlbum(YAlbum albumBase);
    Task DislikeAlbum(YAlbum albumBase);
    Task<IEnumerable<YTrack>> GetTracksAsync(YPlaylist playlist);
    void Download(Music selectedMusic);
}