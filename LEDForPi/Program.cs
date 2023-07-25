//The default settings uses a frequency of 800000 Hz and the DMA channel 10.

using System.Device.Gpio;
using System.Diagnostics;
using System.Drawing;
using System.Text.Json;
using ComputerUtils.Webserver;
using LEDForPi;
using NetCoreAudio;
using rpi_ws281x;

StripWrapper w = new StripWrapper();
w.Init(30, Pin.Gpio21);

Player p = new Player();
p.Play("audio.wav");

HttpServer server = new HttpServer();
server.AddWSRoute("/", request =>
{
    DataReport r = JsonSerializer.Deserialize<DataReport>(request.bodyString);
    switch (r.type)
    {
        case DataType.UPDATE:
            RBSongPlayer.SetSongTime(r.time);
            RBSongPlayer.SetShipPos(r.shipPos);
            break;
        case DataType.LASER_SHOT:
            RBSongPlayer.LaserShot();
            break;
        case DataType.TARGET_HIT:
            RBSongPlayer.TargetHit(r.targetIndex);
            break;
    }
});
server.AddRoute("POST", "/map", request =>
{
    request.SendString("");
    RBSongPlayer.PlaySong(w, JsonSerializer.Deserialize<MapDifficulty>(request.bodyString));
    return true;
});
server.StartServer(14007);