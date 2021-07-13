using System;
using System.Diagnostics;

namespace MELE_launcher
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.Title = "XIVLOADER";
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            var arr = new[]
            {
                @"                                             ",
                @"   __  __                   __  __        _     _                         _                 ___    _ _ _   _          ",
                @"  |  \/  |__ _ ______  ___ / _|/ _|___ __| |_  | |   ___ __ _ ___ _ _  __| |__ _ _ _ _  _  | __|__| (_) |_(_)___ _ _  ",
                @"  | |\/| / _` (_-<_-< / -_)  _|  _/ -_) _|  _| | |__/ -_) _` / -_) ' \/ _` / _` | '_| || | | _|/ _` | |  _| / _ \ ' \ ",
                @"  |_|  |_\__,_/__/__/ \___|_| |_| \___\__|\__| |____\___\__, \___|_||_\__,_\__,_|_|  \_, | |___\__,_|_|\__|_\___/_||_|",
                @"  | |   __ _ _  _ _ _  __| |_  ___ _ _                  |___/                        |__/                             ",
                @"  | |__/ _` | || | ' \/ _| ' \/ -_) '_|                                                                               ",
                @"  |____\__,_|\_,_|_||_\__|_||_\___|_|                                                                                 ",
            };
            Console.WindowWidth = 160;
            Console.WriteLine("\n\n");
            foreach (string line in arr)
            {
                Console.WriteLine(line);
            }
            Console.WriteLine("What do you wish to launch?");
            Console.WriteLine("ME1 - Mass Effect 1 , ME2 - Mass Effect 2 , ME3- Mass Effect 3");
            Console.Write("INPUT : - ");
            string choice = Console.ReadLine();
            if (choice == "ME1")
			{
                Console.WriteLine();
                Console.Write("Please enter your gamepath - ");
                string gamePath = Console.ReadLine();
                Console.Write("Please enter your locale - ");
                string locale = Console.ReadLine();
                
                 Process me1game = new Process();
                 me1game.StartInfo.FileName = gamePath + "/Game/ME1/Binaries/Win64/MassEffect1.exe";
                 
                 me1game.StartInfo.Arguments = $"-NoHomeDir -SeekFreeLoadingPCConsole -locale locale -OVERRIDELANGUAGE={locale} -Subtitles 20 -NOFORCEFEEDBACK -TELEMOPTIN 0";
            
                 me1game.Start();
            
                
			}
            if (choice == "ME2")
			{
                Console.WriteLine();
                Console.Write("Please enter your gamepath - ");
                string gamePath = Console.ReadLine();
                Console.Write("Please enter your locale - ");
                string locale = Console.ReadLine();
                
                 Process me2game = new Process();
                 me2game.StartInfo.FileName = gamePath + "/Game/ME2/Binaries/Win64/MassEffect2.exe";
                 
                 me2game.StartInfo.Arguments = $"-NoHomeDir -SeekFreeLoadingPCConsole -locale locale -OVERRIDELANGUAGE={locale} -Subtitles 20 -NOFORCEFEEDBACK -TELEMOPTIN 0";
            
                 me2game.Start();
            
                
			}
            if (choice == "ME3")
			{
                Console.WriteLine();
                Console.Write("Please enter your gamepath - ");
                string gamePath = Console.ReadLine();
                Console.Write("Please enter your locale - ");
                string locale = Console.ReadLine();
                
                 Process me3game = new Process();
                 me3game.StartInfo.FileName = gamePath + "/Game/ME3/Binaries/Win64/MassEffect3.exe";
                 
                 me3game.StartInfo.Arguments = $"-NoHomeDir -SeekFreeLoadingPCConsole -locale locale -language={locale} -Subtitles 20 -NOFORCEFEEDBACK -TELEMOPTIN 0";
            
                 me3game.Start();
            
                
			}
		}
	}
}
