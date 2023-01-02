using System.Collections.Generic;
using System.Threading.Tasks;
using Yandex.Music.Api.Models.Album;
using Yandex.Music.Api.Models.Track;

namespace MusicApp.Interfaces;

public interface IMusicLoader
{
    string GetTrackUrl(string id);

    IEnumerable<YAlbum> GetAlbums();

    IEnumerable<YTrack> GetLikedList();

    Task Reload();

}