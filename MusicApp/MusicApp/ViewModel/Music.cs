using System;
using System.Linq;
using Xamarin.Forms;
using Yandex.Music.Api.Models.Album;
using Yandex.Music.Api.Models.Artist;
using Yandex.Music.Api.Models.Playlist;
using Yandex.Music.Api.Models.Track;

namespace MusicApp.ViewModel
{
    public class Music: BaseViewModel
    {
        public static int MaxTitleLength = 45;
       
        public string Title
        {
            get => _title.Length>MaxTitleLength ? _title.Substring(0, MaxTitleLength)+"..." : _title;
            set => _title = value;
        }

        public static int MaxArtistLength = 50;

        public string Artist
        {
            get => _artist.Length>MaxArtistLength ? _artist.Substring(0, MaxArtistLength)+"..." : _artist;
            set => _artist = value;
        }

        public string Url { get; set; }
        
        public string Id { get; set; }
        public string CoverImage { get; set; }
        public string CoverImageHD { get; set; }

        private bool _isCurrent;
        public bool IsRecent
        {
            get => _isCurrent;
            set
            {
              _isCurrent = value;
              OnPropertyChanged();
              OnPropertyChanged(nameof(PlayImage));
            }
        }

        public bool CanLike => !string.IsNullOrEmpty(Id) && (Base is YAlbum or YTrack);
        public object Base { get; set; }
        public Music()
        {
        }

        private bool _isLiked;
        private string _title;
        private string _artist;

        public bool IsLiked
        {
            get => _isLiked;
            set
            {
                _isLiked = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LikedImage));
            }
        }
        public string LikedImage => IsLiked ? "liked.png" : "like.png";
        public string LikedImageLight => IsLiked ? "liked.png" : "likeWhite.png";
        public string PlayImage => _isCurrent ? "playCurrent.png" : "playBlack.png";

        public Music(YTrack track, bool isLiked)
        {
            Title = track.Title;
            Id = track.GetKey().ToString();
            Artist = string.Join(",", track.Artists?.Select(a => a.Name) ?? Array.Empty<string>());
            CoverImage = "https://" + (track.OgImage ?? track.CoverUri)?.Replace("%%", "100x100");
            CoverImageHD = "https://" + (track.OgImage ?? track.CoverUri)?.Replace("%%", "200x200");
            Base = track;
            _isLiked = isLiked;
        }

        public Music(YAlbum album, bool isLiked)
        {
            Title = album.Title;
            Id = album.Id;
            Artist = string.Join(",", album.Artists?.Select(a => a.Name) ?? Array.Empty<string>());
            CoverImage = "https://" + album.CoverUri?.Replace("%%", "200x200");
            CoverImageHD = "https://" + album.CoverUri?.Replace("%%", "400x400");
            IsLiked = isLiked;
            Base = album;
        }
        
        public Music(YArtist artist, bool isLiked)
        {
            Title = "";
            Id = artist.Id;
            IsLiked = isLiked;
            Artist = artist.Name;
            CoverImage = "https://" + artist.OgImage.Replace("%%", "200x200");
            CoverImageHD = "https://" + artist.OgImage.Replace("%%", "400x400");
            Base = artist;
        }
        
        public Music(YPlaylist playlist)
        {
            Title = playlist.Title;
            Artist = "";
            Id = playlist.PlaylistUuid;
            CoverImage = "https://" + (playlist.OgImage ?? playlist.Image)?.Replace("%%", "200x200");
            CoverImageHD = "https://" + (playlist.OgImage ?? playlist.Image)?.Replace("%%", "400x400");
            Base = playlist;
        }
        
    }
}
