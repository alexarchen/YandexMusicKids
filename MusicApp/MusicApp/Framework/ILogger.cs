using System;

namespace MusicApp.Framework;

public interface ILogger
{
    void Info(string s);
    void Warn(string s);
    void Error(string s, Exception e);
}