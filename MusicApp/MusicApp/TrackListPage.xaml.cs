using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicApp.Framework;
using MusicApp.Model;
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
        public TrackListPage(IEnumerable<YTrack> tracks, Music album, IMusicLoader loader)
        {
            InitializeComponent();
            BindingContext = new TrackListViewModel(tracks, album, loader);
        }

        public TrackListPage()
        {
            InitializeComponent();
        }

        private void ImageButton_OnClicked(object sender, EventArgs e)
        {
            var model = (BindingContext as TrackListViewModel);
            var music = (sender as ImageButton).BindingContext as Music;
            if ((model != null && music != null))
            {
                model.SelectionCommand.Execute(music);
            }
        }
    }
}
