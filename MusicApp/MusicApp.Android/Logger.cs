using System;
using MusicApp.Framework;

namespace MusicApp.Droid;

public class Logger: ILogger
{
    public void Info(string s)
    {
        Android.Util.Log.Info(nameof(Logger),s);
    }

    public void Warn(string s)
    {
        Android.Util.Log.Warn(nameof(Logger),s);
    }

    public void Error(string s, Exception e)
    {
        Android.Util.Log.Error(nameof(Logger),$"{s} {e}");
    }
}