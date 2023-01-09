using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Retroherz
{
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            bool fullscreen = true;
            Size resolution = new Size(3840, 2160);

            // Base resolution  => 320 x 180
            // Scale factors => 1x, 2x, 4x, 6x, 12x, 24x
            var baseResolution = new Size(320, 180);

            var resolutionList = new List<Tuple<Size, string>>();
            resolutionList.Add(new Tuple<Size, string>(baseResolution * 1, "180p"));
            resolutionList.Add(new Tuple<Size, string>(baseResolution * 2, "360p"));
            resolutionList.Add(new Tuple<Size, string>(baseResolution * 4, "720p"));
            resolutionList.Add(new Tuple<Size, string>(baseResolution * 6, "1080p"));
            resolutionList.Add(new Tuple<Size, string>(baseResolution * 12, "4K"));
            resolutionList.Add(new Tuple<Size, string>(baseResolution * 24, "8K"));

            foreach (var res in resolutionList)
                System.Console.WriteLine("{0}, {1} - {2}", res.Item1.Width, res.Item1.Height, res.Item2);

            // REDO!!
            if (args.Length > 0)
            {
                foreach (string argument in args)
                {
                    switch (argument.ToLower())
                    {
                        case "--fullscreen":
                                fullscreen = true;
                            break;
                        case "--windowed":
                                fullscreen = false;
                            break;
                        case "--nes":
                            resolution = new Size(266, 240);
                            break;
                        case "--snes":
                            resolution = new Size(256,224);
                            break;
                        case "--genesis":
                            resolution = new Size(320, 244);
                            break;
                        case "--360p":
                            resolution = new Size(640, 360);
                            break;
                        case "--450p":
                            resolution = new Size(800, 450);
                            break;
                        case "--720p":
                            resolution = new Size(1280, 720);
                            break;
                        case "--1080p":
                            resolution = new Size(1920, 1080);
                            break;
                        default:
                            resolution = new Size(3840, 2160);
                            break;
                    }
                }
            }

            resolution = resolutionList[3].Item1;

            using (var game = new Retroherz(resolution, fullscreen))
                game.Run();
        }
    }
}
