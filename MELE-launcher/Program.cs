using System;
using System.Diagnostics;
using System.IO;

namespace MELE_launcher
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.Title = "Legendary Launcher";
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
            if (args == null || args.Length == 0) 
            { 
                Console.WriteLine("What do you wish to launch?");
                Console.WriteLine("ME1 - Mass Effect 1 , ME2 - Mass Effect 2 , ME3- Mass Effect 3");
                Console.Write("INPUT : - ");
                string choice = Console.ReadLine();
                Process me1game = new Process();
                Process me2game = new Process();
                Process me3game = new Process();
                Console.WriteLine();
                string gamePath;
                if (File.Exists(Directory.GetCurrentDirectory() + @"\gamepath.txt")) {
                  TextReader tr = new StreamReader("gamepath.txt");
                  string gamePathread = tr.ReadLine();
                  gamePath = gamePathread;
                  tr.Close();
                  Console.WriteLine(gamePath);
                }
                else
			    {
                  Console.Write("Please enter your gamepath - ");
                  gamePath = Console.ReadLine();
                  TextWriter tw = new StreamWriter("gamepath.txt");
                  tw.WriteLine(gamePath);
                  tw.Close();
			    }
                bool ForceFeedback = false;
                Console.Write("Do you wish to enable Controler Force Feedback? - ");
                string promtw = Console.ReadLine();
                if (promtw.ToLower() == "yes")
                {
                  ForceFeedback = true;
                }
                else
                {
                  ForceFeedback = false;
                }
                if (choice == "ME1")
			    {

                
                  Console.Write("Please enter your locale - ");
                  string locale = Console.ReadLine();
                
                 
                  me1game.StartInfo.FileName = Path.Join(gamePath, "/Game/ME1/Binaries/Win64/MassEffect1.exe");

                  if (ForceFeedback == true)
				  {
                    me1game.StartInfo.Arguments = $"-NoHomeDir -SeekFreeLoadingPCConsole -locale locale -OVERRIDELANGUAGE={locale} -Subtitles 20 -TELEMOPTIN 0";
				  }
                  else {
                    me1game.StartInfo.Arguments = $"-NoHomeDir -SeekFreeLoadingPCConsole -locale locale -OVERRIDELANGUAGE={locale} -Subtitles 20 -NOFORCEFEEDBACK -TELEMOPTIN 0";
                  }
                 
            
                  me1game.Start();

                  Console.ReadLine();
                }
                if (choice == "ME2")
			    {
                
                
                 Console.Write("Please enter your locale - ");
                 string locale = Console.ReadLine();
                
                 
                 me2game.StartInfo.FileName = Path.Join(gamePath, "/Game/ME2/Binaries/Win64/MassEffect2.exe");
                 
                 if (ForceFeedback == true)
				 {
                    me2game.StartInfo.Arguments = $"-NoHomeDir -SeekFreeLoadingPCConsole -locale locale -OVERRIDELANGUAGE={locale} -Subtitles 20 -TELEMOPTIN 0";
				 }
                 else {
                    me2game.StartInfo.Arguments = $"-NoHomeDir -SeekFreeLoadingPCConsole -locale locale -OVERRIDELANGUAGE={locale} -Subtitles 20 -NOFORCEFEEDBACK -TELEMOPTIN 0";
                 }
            
                 me2game.Start();

                 Console.ReadLine();
            
                
			    }
                if (choice == "ME3")
			    {
                
                 Console.Write("Please enter your locale - ");
                 string locale = Console.ReadLine();
                
                 
                 me3game.StartInfo.FileName = Path.Join(gamePath, "/Game/ME3/Binaries/Win64/MassEffect3.exe");

                 if (ForceFeedback == true)
				 {
                    me3game.StartInfo.Arguments = $"-NoHomeDir -SeekFreeLoadingPCConsole -locale locale -language={locale} -Subtitles 20 -TELEMOPTIN 0";
				 }
                 else {
                    me3game.StartInfo.Arguments = $"-NoHomeDir -SeekFreeLoadingPCConsole -locale locale -language={locale} -Subtitles 20 -NOFORCEFEEDBACK -TELEMOPTIN 0";
                 }
                 
            
                 me3game.Start();

                 Console.ReadLine();

                }
            }
            else
			{
                Console.WriteLine("Arguments: Game: {0}, Force Feedback: {1}, Language: {2}",args[0], args[1] , args[2]);
                string choice = args[0];
                Process me1game = new Process();
                Process me2game = new Process();
                Process me3game = new Process();
                string gamePath;
                if (File.Exists(Directory.GetCurrentDirectory() + @"\gamepath.txt")) {
                  TextReader tr = new StreamReader("gamepath.txt");
                  string gamePathread = tr.ReadLine();
                  gamePath = gamePathread;
                  tr.Close();
                  //Console.WriteLine(gamePath);
                }
                else
			    {
                  Console.Write("Please enter your gamepath - ");
                  gamePath = Console.ReadLine();
                  TextWriter tw = new StreamWriter("gamepath.txt");
                  tw.WriteLine(gamePath);
                  tw.Close();
			    }
                bool ForceFeedback = false;
                string promtw = args[1];
                if (promtw.ToLower() == "yes" || promtw.ToLower() == "-yes")
                {
                  ForceFeedback = true;
                }
                else
                {
                  ForceFeedback = false;
                }
                if (choice == "ME1" || choice == "-ME1")
			    {

                
                  
                  string locale = args[2].Replace("-","");
                
                 
                  me1game.StartInfo.FileName = Path.Join(gamePath, "/Game/ME1/Binaries/Win64/MassEffect1.exe");

                  if (ForceFeedback == true)
				  {
                    me1game.StartInfo.Arguments = $"-NoHomeDir -SeekFreeLoadingPCConsole -locale locale -OVERRIDELANGUAGE={locale} -Subtitles 20 -TELEMOPTIN 0";
				  }
                  else {
                    me1game.StartInfo.Arguments = $"-NoHomeDir -SeekFreeLoadingPCConsole -locale locale -OVERRIDELANGUAGE={locale} -Subtitles 20 -NOFORCEFEEDBACK -TELEMOPTIN 0";
                  }
                 
            
                  me1game.Start();

                  Console.ReadLine();
                }
                if (choice == "ME2" || choice == "-ME2")
			    {
                
                
                 
                 string locale = args[2].Replace("-","");
                
                 
                 me2game.StartInfo.FileName = Path.Join(gamePath, "/Game/ME2/Binaries/Win64/MassEffect2.exe");
                 
                 if (ForceFeedback == true)
				 {
                    me2game.StartInfo.Arguments = $"-NoHomeDir -SeekFreeLoadingPCConsole -locale locale -OVERRIDELANGUAGE={locale} -Subtitles 20 -TELEMOPTIN 0";
				 }
                 else {
                    me2game.StartInfo.Arguments = $"-NoHomeDir -SeekFreeLoadingPCConsole -locale locale -OVERRIDELANGUAGE={locale} -Subtitles 20 -NOFORCEFEEDBACK -TELEMOPTIN 0";
                 }
            
                 me2game.Start();

                 Console.ReadLine();
            
                
			    }
                if (choice == "ME3" || choice == "-ME3")
			    {
                
                 
                 string locale = args[2].Replace("-","");
                
                 
                 me3game.StartInfo.FileName = Path.Join(gamePath, "/Game/ME3/Binaries/Win64/MassEffect3.exe");

                 if (ForceFeedback == true)
				 {
                    me3game.StartInfo.Arguments = $"-NoHomeDir -SeekFreeLoadingPCConsole -locale locale -language={locale} -Subtitles 20 -TELEMOPTIN 0";
				 }
                 else {
                    me3game.StartInfo.Arguments = $"-NoHomeDir -SeekFreeLoadingPCConsole -locale locale -language={locale} -Subtitles 20 -NOFORCEFEEDBACK -TELEMOPTIN 0";
                 }
                 
            
                 me3game.Start();

                 Console.ReadLine();

                }
			}
                
		
            
		
            
		}
	}
}
