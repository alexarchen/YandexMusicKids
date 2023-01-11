using System;
using MusicApp.Framework;
using Xamarin.Forms;
using Yandex.Music.Api.Models.Account;

namespace MusicApp.ViewModel;

public class AccountViewModel : BaseViewModel
{
    private readonly IMusicLoader _loader;
    private YAccount _account;

    public AccountViewModel(IMusicLoader loader)
    {
        _loader = loader;
        _loader.Reloaded += LoaderOnReloaded;
        _account = _loader.GetAccount();
    }

    private void LoaderOnReloaded(object arg1, EventArgs arg2)
    {
        _account = _loader.GetAccount();

        Device.InvokeOnMainThreadAsync(() =>
        {
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(FullName));
        });

    }

    public string Name => _account?.Login;

    public string FullName => _account?.FullName;

    public void Logout()
    {
        _account = null;
        _loader.Logout();
    }
   
}