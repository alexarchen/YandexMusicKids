using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicApp.Interfaces;
using MusicApp.ViewModel;
using Xamarin.Forms;
using Yandex.Music.Api.Models.Track;

namespace MusicApp
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class TrackListPage : ContentPage
    {
        public TrackListPage(IList<YTrack> tracks, IMusicLoader loader)
        {
            InitializeComponent();
            
            BindingContext = new TrackListViewModel(tracks, loader);
        }
    }
}
