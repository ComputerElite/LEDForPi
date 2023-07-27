//The default settings uses a frequency of 800000 Hz and the DMA channel 10.

using System.Device.Gpio;
using System.Diagnostics;
using System.Drawing;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text.Json;
using ComputerUtils.Logging;
using ComputerUtils.Webserver;
using LEDForPi;
using NetCoreAudio;
using rpi_ws281x;
using Color = LEDForPi.Color;

StripWrapper w = new StripWrapper();
w.Init(120, Pin.Gpio21);

Player p = new Player();
p.Play("audio.wav");

Logger.displayLogInConsole = true;
RBSongPlayerConfig.Load();
SongManager.LoadAllMaps();

HttpServer server = new HttpServer();
server.logRequests = false;
server.AddWSRoute("/", request =>
{
    DataReport r = JsonSerializer.Deserialize<DataReport>(request.bodyString);
    switch (r.type)
    {
        case DataType.UPDATE:
            RBSongPlayer.SetSongTime(r.time);
            RBSongPlayer.SetShipPos(r.shipPos);
            RBSongPlayer.SetSpeed(r.speed);
            break;
        case DataType.LASER_SHOT:
            RBSongPlayer.LaserShot();
            break;
        case DataType.TARGET_HIT:
            RBSongPlayer.TargetHit(r.targetIndex);
            break;
    }
});
server.AddWSRoute("/strip", request =>
{
    Dictionary<int, Color> colors = new Dictionary<int, Color>();
    foreach (int led in w.displayedColors.Keys.OrderBy(x => x))
    {
        colors.Add(led, new Color(w.displayedColors[led].R / 255f, w.displayedColors[led].G / 255f, w.displayedColors[led].B / 255f));
    }
    request.SendString(JsonSerializer.Serialize(colors));
});
server.AddWSRoute("/status", request =>
{
    SongPlayStatus s = new SongPlayStatus();
    s.songTime = RBSongPlayer.elapsedSeconds;
    s.speed = RBSongPlayer.speed;
    s.currentSongId = RBSongPlayer.currentSongId;
    request.SendString(JsonSerializer.Serialize(s));
});
server.AddRoute("POST", "/map", request =>
{
    request.SendString("");
    RBSongPlayer.useGame = true;
    RBSongPlayer.PlaySong(w, JsonSerializer.Deserialize<MapDifficulty>(request.bodyString));
    return true;
});
server.AddRoute("POST", "/info", request =>
{
    request.SendString("");
    RBSongPlayer.info = JsonSerializer.Deserialize<MapInfo>(request.bodyString);
    return true;
});
server.AddRoute("GET", "/strip", request =>
{
    request.SendString(JsonSerializer.Serialize(w.displayedColors));
    return true;
});
server.AddRoute("GET", "/configjson", request =>
{
    request.SendString(JsonSerializer.Serialize(RBSongPlayerConfig.instance));
    return true;
});
server.AddRoute("POST", "/configjson", request =>
{
    RBSongPlayerConfig.instance = JsonSerializer.Deserialize<RBSongPlayerConfig>(request.bodyString);
    RBSongPlayerConfig.Save();
    request.SendString(JsonSerializer.Serialize(RBSongPlayerConfig.instance));
    return true;
});

// Map
server.AddRoute("POST", "/importmap", request =>
{
    SongManager.ImportZipFile(request.bodyBytes);
    request.SendString("Song imported");
    return true;
});
server.AddRoute("POST", "/importreplay", request =>
{
    request.SendString(SongManager.ImportReplay(request.bodyString));
    return true;
});
server.AddRoute("POST", "/stopsong", request =>
{
    RBSongPlayer.Stop();
    request.SendString("song stopped");
    return true;
});
server.AddRoute("GET", "/songs", request =>
{
    request.SendString(JsonSerializer.Serialize(SongManager.loadedMaps.Values));
    return true;
});
server.AddRoute("GET", "/replays/", request =>
{
    string songId = request.pathDiff.Split('/')[0];
    string diff = request.pathDiff.Split('/')[1];
    request.SendString(JsonSerializer.Serialize(SongManager.GetReplays(songId, diff)));
    return true;
}, true);
server.AddRoute("POST", "/playsong", request =>
{
    request.SendString("Playing");
    SongPlayRequest r = JsonSerializer.Deserialize<SongPlayRequest>(request.bodyString);
    RBSongPlayer.info = SongManager.GetSongFromLibraryBasedOnId(r.songId);
    
    RBSongPlayer.useGame = false;

    RBSongPlayer.replay = SongManager.LoadReplay(RBSongPlayer.info, r.replay);
    RBSongPlayer.PlaySong(w, SongManager.LoadDifficulty(RBSongPlayer.info, r.diffName));
    return true;
});
server.AddRoute("GET", "/audio/", request =>
{
    request.SendData(SongManager.GetAudioFile(request.pathDiff), HttpServer.GetContentTpe(SongManager.GetSongFromLibraryBasedOnId(request.pathDiff).songFileName));
    return true;
}, true, true, true);
server.AddRouteFile("/view", "view.html", false, true, true);
server.AddRouteFile("/", "index.html", false, true, true);
server.AddRouteFile("/config", "config.html", false, true, true);
server.StartServer(14007);