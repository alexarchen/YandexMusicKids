using System;
using System.Linq;
using Xamarin.Forms;
using Yandex.Music.Api.Models.Album;
using Yandex.Music.Api.Models.Track;

namespace MusicApp.ViewModel
{
    public class Music: BaseViewModel
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Url { get; set; }
        
        public string Id { get; set; }
        public string CoverImage { get; set; }

        private bool _isRecent;
        public bool IsRecent
        {
            get => _isRecent;
            set
            {
              _isRecent = value;
              OnPropertyChanged();
              OnPropertyChanged(nameof(PlayColor1));
              OnPropertyChanged(nameof(PlayColor2));
            }
        }

        public object Base { get; set; }
        public Music()
        {
        }

        public Color PlayColor1 => Color.FromHex(_isRecent ? "E3A7AE" : "E3E7EE");
        public Color PlayColor2 => Color.FromHex(_isRecent ? "FBBBBB" : "FBFBFB");
        
        public Music(YTrack track)
        {
            Title = track.Title;
            Id = track.Id;
            Artist = string.Join(",", track.Artists.Select(a => a.Name) ?? Array.Empty<string>());
            CoverImage = "https://" + track.CoverUri.Replace("%%", "400x400");
            Base = track;
        }

        public Music(YAlbum album)
        {
            Title = album.Title;
            Id = album.Id;
            Artist = string.Join(",", album.Artists.Select(a => a.Name) ?? Array.Empty<string>());
            CoverImage = "https://" + album.CoverUri.Replace("%%", "400x400");
            Base = album;
        }
    }
}
