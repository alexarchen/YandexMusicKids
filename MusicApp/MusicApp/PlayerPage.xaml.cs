using System;
using System.Diagnostics;
using MusicApp.Framework;
using MusicApp.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MusicApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PlayerPage : ContentPage
    {
        private readonly TrackListViewModel _trackListViewModel;
        private readonly IMusicLoader _loader;

        public PlayerPage(TrackListViewModel trackListViewModel,Music music, IMusicLoader loader)
        {
            _trackListViewModel = trackListViewModel;
            _loader = loader;
            
            InitializeComponent();

            Device.InvokeOnMainThreadAsync(async ()=>
            {
                BindingContext = await PlayerViewModel.CreateAsync(trackListViewModel, music, _loader);
            });
           
        }

        public PlayerPage()
        {
            InitializeComponent();
            BindingContext = PlayerViewModel.LastPlayerModel ?? new PlayerViewModel()
            {
            };
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            Debug.WriteLine($"PlayerPage Binding changed");
        }

        private void Slider_OnDragCompleted(object sender, EventArgs e)
        {
            if (BindingContext!=null)
             (BindingContext as PlayerViewModel)!.SetPosition((FindByName("Slider") as Slider)?.Value ?? 0);
        }
    }
}