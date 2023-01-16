using System;
using Retroherz.Managers;

namespace Retroherz
{
    public static class Program
    {
		private static void ListDisplayModes()
		{
			Console.WriteLine("Display Modes:");
			foreach (var mode in GraphicsManager.DisplayModes.Values)
				Console.WriteLine($"{mode.Resolution} - {mode.ArgumentsLiteral}");
		}

        [STAThread]
        static void Main(string[] args)
        {
            bool isFullScreen = true;
			bool scale = false;
			GraphicsMode displayMode = GraphicsMode.Default;

            if (args.Length > 0)
            {
                foreach (string argument in args)
                {
                    switch (argument.ToLower())
                    {
						case "--list":
							ListDisplayModes();
							return;
                        case "--windowed":
                                isFullScreen = false;
                            break;
						case "--scale":
								scale = true;
							break;
                        case "--nes":
                            displayMode = GraphicsMode.NES;
                            break;
                        case "--snes":
                            displayMode = GraphicsMode.SNES;
                            break;
                        case "--genesis":
                            displayMode = GraphicsMode.Genesis;
                            break;
						case "--180p":
							displayMode = GraphicsMode.OneEightyP;
							break;
                        case "--360p":
                            displayMode = GraphicsMode.ThreeSixtyP;
                            break;
                        case "--450p":
                            displayMode = GraphicsMode.FourFiftyP;
                            break;
                        case "--720p":
                           	displayMode = GraphicsMode.SevenTwentyP;
                            break;
                        case "--1080p":
                            displayMode = GraphicsMode.ThousandEightyP;
                            break;
                        case "--4K":
                            displayMode = GraphicsMode.FourK;
                            break;
                        case "--8K":
                            displayMode = GraphicsMode.EightK;
                            break;
                        default:
                            displayMode = GraphicsMode.Default;
                            break;
                    }
                }
            }

			ListDisplayModes();

			GraphicsManager.DisplayModes[0].ToString();

            using (var game = new Retroherz(displayMode, isFullScreen, scale))
                game.Run();
        }
    }
}
