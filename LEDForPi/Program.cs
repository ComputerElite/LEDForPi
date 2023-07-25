//The default settings uses a frequency of 800000 Hz and the DMA channel 10.

using System.Device.Gpio;
using System.Diagnostics;
using System.Drawing;
using System.Text.Json;
using LEDForPi;
using NetCoreAudio;
using rpi_ws281x;

StripWrapper w = new StripWrapper();
w.Init(30, Pin.Gpio21);

Player p = new Player();
p.Play("audio.wav");
RBSongPlayer.PlaySong(w, JsonSerializer.Deserialize<MapDifficulty>(File.ReadAllText("map.json")));