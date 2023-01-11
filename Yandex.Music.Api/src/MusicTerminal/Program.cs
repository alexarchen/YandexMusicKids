// See https://aka.ms/new-console-template for more information

using Yandex.Music.Api;
using Yandex.Music.Api.Common;

Console.WriteLine("Ya music client!");
var api = new YandexMusicApi();
var storage = new AuthStorage();

while (!storage.IsAuthorized)
{
    Console.WriteLine("Type login:");
    var login = Console.ReadLine();
    Console.WriteLine("Type password:");
    var pass = Console.ReadLine();
   
    api.User.Authorize(storage, login, pass);
}

var trackids = (await api.Library.GetLikedTracksAsync(storage)).Result.Library.Tracks.Select(t=>t.Id);
var tracks = (await api.Track.GetAsync(storage, trackids)).Result;
var artists = tracks.SelectMany(t => t.Artists).ToArray();
var albums = tracks.SelectMany(t => t.Albums).ToArray();
Console.WriteLine("Favorite artists:");
foreach (var artist in artists)
{
    Console.WriteLine($"{artist.Name}");
}
Console.WriteLine("Favorite albums:");
foreach (var album in albums)
{
    Console.WriteLine($"{album.Title} {string.Join(",",album.Artists?.Select(a=>a.Name)??Array.Empty<string>())}");
}

Console.WriteLine("Favorite tracks:");
foreach (var track in tracks)
{
    var url = api.Track.GetFileLinkAsync(storage, track.GetKey().ToString()).Result;
   Console.WriteLine($"{track.Id} {track.Title} {string.Join(",",track.Artists?.Select(a=>a.Name)??Array.Empty<string>())} {url}");
}

var aalbums = (await api.Artist.GetAlbumsAsync(storage, "781568")).Result.Albums;
foreach (var a in aalbums)
{
    Console.WriteLine($"Album: {a.Id} {a.Title}");
}


do
{
    Console.WriteLine("Enter command:");
    var s = Console.ReadLine();
    Console.WriteLine(await api.Custom.GetAsync(storage, s));

} while (true);



