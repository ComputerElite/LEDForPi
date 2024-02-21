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
using LEDForPi.RBExtras;
using LEDForPi.Strips;
using NetCoreAudio;
using rpi_ws281x;
using Color = LEDForPi.RBExtras.Color;

//StripWrapper w = new StripWrapper();
//w.Init(120, Pin.Gpio21);

RBSongPlayerConfig.Load();
Logger.displayLogInConsole = true;
VirtualStrip v = new VirtualStrip();
v.Init(new List<StripRepresentation>()
{
    new StripRepresentation()
    {
        isVirtual = false,
        pin = Pin.Gpio21,
        ledCount = 60,
        ledStart = 0
    }
});

Player p = new Player();
//p.Play("audio.wav");

StripControllerManager manager = new StripControllerManager();
manager.StartUpdateThread();

SongManager.LoadAllMaps();

HttpServer server = new HttpServer();
server.logRequests = false;
server.AddWSRoute("/", request =>
{
    DataReport r = JsonSerializer.Deserialize<DataReport>(request.bodyString);
    if(RBSongPlayer.mostRecentStripController == null) return;
    switch (r.type)
    {
        case DataType.UPDATE:
            RBSongPlayer.mostRecentStripController.SetSongTime(r.time);
            RBSongPlayer.mostRecentStripController.SetShipPos(r.shipPos);
            RBSongPlayer.mostRecentStripController.SetSpeed(r.speed);
            break;
        case DataType.LASER_SHOT:
            RBSongPlayer.mostRecentStripController.LaserShot();
            break;
        case DataType.TARGET_HIT:
            RBSongPlayer.mostRecentStripController.TargetHit(r.targetIndex);
            break;
    }
});
server.AddWSRoute("/strip", request =>
{
    Dictionary<int, Color> colors = new Dictionary<int, Color>();
    foreach (int led in v.displayedColors.Keys.OrderBy(x => x))
    {
        colors.Add(led, new Color(v.displayedColors[led].R / 255f, v.displayedColors[led].G / 255f, v.displayedColors[led].B / 255f));
    }
    request.SendString(JsonSerializer.Serialize(new StripInfo()
    {
        colors = colors,
        time = RBSongPlayer.mostRecentStripController.elapsedSeconds,
        songId = RBSongPlayer.songId
    }));
});
server.AddWSRoute("/status", request =>
{
    SongPlayStatus s = new SongPlayStatus();
    s.songTime = RBSongPlayer.mostRecentStripController.elapsedSeconds;
    s.speed = RBSongPlayer.mostRecentStripController.speed;
    s.currentSongId = RBSongPlayer.currentSongId;
    request.SendString(JsonSerializer.Serialize(s));
});
server.AddRoute("POST", "/map", request =>
{
    // Used by RB Game to start game
    request.SendString("");
    RBSongPlayer.useGame = true;
    RBStripController c = new RBStripController();
    c.InitSong(v, JsonSerializer.Deserialize<MapDifficulty>(request.bodyString));
    manager.AddController(c);
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
    request.SendString(JsonSerializer.Serialize(v.displayedColors));
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
    v.Init(new List<StripRepresentation>()
    {
        new StripRepresentation()
        {
            isVirtual = false,
            pin = Pin.Gpio21,
            ledCount = RBSongPlayerConfig.playfieldSize,
            ledStart = 0
        }
    });
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
    // Used by Web UI to play song
    request.SendString("Playing");
    SongPlayRequest r = JsonSerializer.Deserialize<SongPlayRequest>(request.bodyString);
    RBSongPlayer.info = SongManager.GetSongFromLibraryBasedOnId(r.songId);
    RBSongPlayer.songId = r.songId;
    
    RBSongPlayer.useGame = false;

    RBSongPlayer.replay = SongManager.LoadReplay(RBSongPlayer.info, r.replay);
    RBStripController c = new RBStripController();
    c.InitSong(v, SongManager.LoadDifficulty(RBSongPlayer.info, r.diffName));
    manager.AddController(c);
    return true;
});
server.AddRoute("GET", "/audio/", request =>
{
    request.SendData(SongManager.GetAudioFile(request.pathDiff), HttpServer.GetContentTpe(SongManager.GetSongFromLibraryBasedOnId(request.pathDiff).songFileName));
    return true;
}, true, true, true);

server.AddRoute("GET", "/audio/", request =>
{
    request.SendData(SongManager.GetAudioFile(request.pathDiff), HttpServer.GetContentTpe(SongManager.GetSongFromLibraryBasedOnId(request.pathDiff).songFileName));
    return true;
}, true, true, true);
server.AddRoute("GET", "/api/animations", request =>
{
    request.SendString(JsonSerializer.Serialize(Animation.GetAnimations()), "application/json");
    return true;
});
server.AddRoute("POST", "/api/setanimation", request =>
{
    switch (request.bodyString)
    {
        case Animation.RainbowFade:
            manager.AddController(new RainbowController(v).SetID("normal"));
            break;
        case Animation.RainbowStatic:
            manager.AddController(new RainbowStaticController(v).SetID("normal"));
            break;
        case Animation.Static:
            manager.AddController(new StaticController(v).SetID("normal"));
            break;
    }
    request.SendString("Set animation to " + request.bodyString);
    return true;
});
server.AddRoute("POST", "/api/setstep", request =>
{
    AnimationSettings.step = double.Parse(request.bodyString);
    request.SendString("Set animation to " + request.bodyString);
    return true;
});
server.AddRoute("POST", "/api/setcolor0", request =>
{
    AnimationSettings.color0 = int.Parse(request.bodyString);
    request.SendString("Set color0 to " + request.bodyString);
    return true;
});
server.AddRoute("POST", "/api/setbrightness", request =>
{
    AnimationSettings.brightness = double.Parse(request.bodyString);
    request.SendString("Set brightness to " + request.bodyString);
    return true;
});
server.AddRouteFile("/view", "view.html", false, true, true);
server.AddRouteFile("/", "index.html", false, true, true);
server.AddRouteFile("/normal", "normal.html", false, true, true);
server.AddRouteFile("/config", "config.html", false, true, true);
server.StartServer(14007);

public class StripInfo 
{
    public Dictionary<int, Color> colors { get; set; } = new Dictionary<int, Color>();
    public double time { get; set; } = 0f;
    public string songId { get; set; } = "";
}