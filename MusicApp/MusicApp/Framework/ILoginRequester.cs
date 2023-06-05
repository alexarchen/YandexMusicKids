using System.Threading.Tasks;
using MusicApp.ViewModel;

namespace MusicApp.Framework;

public interface ILoginRequester
{
    Task<string> RequestLogin(LoginModel model);
}
