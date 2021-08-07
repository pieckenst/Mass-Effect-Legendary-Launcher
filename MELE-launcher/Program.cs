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
            
            
            
            // For command line arguments the format is as this
            // -ME(Game Number) -yes or -no ( For force feedback enable or disable) -Language Code and add -silent if you desire no output
            // The above is  valid for legendary edition versions of the games
            // For legacy games use
            // -OLDME(Game Number) add -silent if you desire no output
           
            if (args == null || args.Length == 0) 
            { 
                foreach (string line in arr)
                {
                  Console.WriteLine(line);
                }
                Console.WriteLine("What do you wish to launch?");
                Console.WriteLine("ME1 - Mass Effect 1 , ME2 - Mass Effect 2 , ME3- Mass Effect 3 , OLDME1 - Legacy Mass Effect 1 , OLDME2 - Legacy Mass Effect 2, OLDME3 - Legacy Mass Effect 3");
                Console.Write("INPUT : - ");
                string choice = Console.ReadLine();
                Process me1game = new Process();
                Process me2game = new Process();
                Process me3game = new Process();
                Process lmexgame = new Process();
                Console.WriteLine();
                string gamePath;
                string legacygamePathme1;
                string legacygamePathme2;
                string legacygamePathme3;
                if (File.Exists(Directory.GetCurrentDirectory() + @"\gamepath.txt")) {
                  TextReader tr = new StreamReader("gamepath.txt");
                  string gamePathread = tr.ReadLine();
                  gamePath = gamePathread;
                  tr.Close();
                  Console.WriteLine(gamePath);
                }
                else
			    {
                  Console.Write("Please enter your Mass Effect Legendary Edition gamepath - ");
                  gamePath = Console.ReadLine();
                  TextWriter tw = new StreamWriter("gamepath.txt");
                  tw.WriteLine(gamePath);
                  tw.Close();
			    }
                bool ForceFeedback = false;
                
                if (choice == "ME1")
			    {

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
                if (choice == "OLDME1" || choice == "-OLDME1") 
                {
                    if (File.Exists(Directory.GetCurrentDirectory() + @"\me1gamepath.txt")) {
                        TextReader tr = new StreamReader("me1gamepath.txt");
                        string gamePathreadme1 = tr.ReadLine();
                        legacygamePathme1 = gamePathreadme1;
                        tr.Close();
                    }
                    else
					{
                        Console.Write("Please enter your legacy Mass Effect 1 gamepath - ");
                        legacygamePathme1 = Console.ReadLine();
                        TextWriter tw = new StreamWriter("me1gamepath.txt");
                        tw.WriteLine(gamePath);
                        tw.Close();
					}
                    lmexgame.StartInfo.FileName = Path.Join(legacygamePathme1, "/Binaries/Win32/MassEffect.exe");
                    lmexgame.Start();
                }
                if (choice == "OLDME2" || choice == "-OLDME2") 
                {
                    if (File.Exists(Directory.GetCurrentDirectory() + @"\me2gamepath.txt")) {
                        TextReader tr = new StreamReader("me2gamepath.txt");
                        string gamePathreadme2 = tr.ReadLine();
                        legacygamePathme2 = gamePathreadme2;
                        tr.Close();
                    }
                    else
					{
                        Console.Write("Please enter your legacy Mass Effect 2 gamepath - ");
                        legacygamePathme2 = Console.ReadLine();
                        TextWriter tw = new StreamWriter("me2gamepath.txt");
                        tw.WriteLine(gamePath);
                        tw.Close();
					}
                    lmexgame.StartInfo.FileName = Path.Join(legacygamePathme2, "/Binaries/Win32/MassEffect2.exe");
                    lmexgame.Start();
                }
                if (choice == "OLDME3" || choice == "-OLDME3") 
                {
                    if (File.Exists(Directory.GetCurrentDirectory() + @"\me3gamepath.txt")) {
                        TextReader tr = new StreamReader("me3gamepath.txt");
                        string gamePathreadme3 = tr.ReadLine();
                        legacygamePathme3 = gamePathreadme3;
                        tr.Close();
                    }
                    else
					{
                        Console.Write("Please enter your legacy Mass Effect 3 gamepath - ");
                        legacygamePathme3 = Console.ReadLine();
                        TextWriter tw = new StreamWriter("me3gamepath.txt");
                        tw.WriteLine(gamePath);
                        tw.Close();
                    }
                    lmexgame.StartInfo.FileName = Path.Join(legacygamePathme3, "/Binaries/Win32/MassEffect3.exe");
                    lmexgame.Start();
                }
            }
            else
			{
                if (args[3] == "silent" || args[3] == "-silent" || args[1] == "-silent" )
			    {
                    Console.WriteLine();
                }
                else
				{
                    Console.WriteLine("Arguments: Game: {0}, Force Feedback: {1}, Language: {2}",args[0], args[1] , args[2]);
				}
                
                string choice = args[0];
                Process me1game = new Process();
                Process me2game = new Process();
                Process me3game = new Process();
                Process lmexgame = new Process();
                string gamePath;
                string legacygamePathme1;
                string legacygamePathme2;
                string legacygamePathme3;
                if (File.Exists(Directory.GetCurrentDirectory() + @"\gamepath.txt")) {
                  TextReader tr = new StreamReader("gamepath.txt");
                  string gamePathread = tr.ReadLine();
                  gamePath = gamePathread;
                  tr.Close();
                  //Console.WriteLine(gamePath);
                }
                else
			    {
                  Console.Write("Please enter your Mass Effect Legendary Edition gamepath - ");
                  gamePath = Console.ReadLine();
                  TextWriter tw = new StreamWriter("gamepath.txt");
                  tw.WriteLine(gamePath);
                  tw.Close();
			    }
                bool ForceFeedback = false;
                
                if (choice == "ME1" || choice == "-ME1")
			    {

                
                  string promtw = args[1];
                  if (promtw.ToLower() == "yes" || promtw.ToLower() == "-yes")
                  {
                    ForceFeedback = true;
                  }
                  else
                  {
                    ForceFeedback = false;
                  }
                  string locale = args[2].Replace("-","");
                  if (locale == null || locale.Length == 0) 
                  {
                        Console.Write("Language argument was not passed down! Please enter your language code: ");
                        locale = Console.ReadLine();
                  }
                
                 
                  me1game.StartInfo.FileName = Path.Join(gamePath, "/Game/ME1/Binaries/Win64/MassEffect1.exe");

                  if (ForceFeedback == true)
				  {
                    me1game.StartInfo.Arguments = $"-NoHomeDir -SeekFreeLoadingPCConsole -locale locale -OVERRIDELANGUAGE={locale} -Subtitles 20 -TELEMOPTIN 0";
				  }
                  else {
                    me1game.StartInfo.Arguments = $"-NoHomeDir -SeekFreeLoadingPCConsole -locale locale -OVERRIDELANGUAGE={locale} -Subtitles 20 -NOFORCEFEEDBACK -TELEMOPTIN 0";
                  }
                 
            
                  me1game.Start();

                  
                }
                if (choice == "ME2" || choice == "-ME2")
			    {
                
                
                  string promtw = args[1];
                  if (promtw.ToLower() == "yes" || promtw.ToLower() == "-yes")
                  {
                    ForceFeedback = true;
                  }
                  else
                  {
                    ForceFeedback = false;
                  }
                 string locale = args[2].Replace("-","");
                 if (locale == null || locale.Length == 0) 
                 {
                    Console.Write("Language argument was not passed down! Please enter your language code: ");
                    locale = Console.ReadLine();
                 }
                
                 
                 me2game.StartInfo.FileName = Path.Join(gamePath, "/Game/ME2/Binaries/Win64/MassEffect2.exe");
                 
                 if (ForceFeedback == true)
				 {
                    me2game.StartInfo.Arguments = $"-NoHomeDir -SeekFreeLoadingPCConsole -locale locale -OVERRIDELANGUAGE={locale} -Subtitles 20 -TELEMOPTIN 0";
				 }
                 else {
                    me2game.StartInfo.Arguments = $"-NoHomeDir -SeekFreeLoadingPCConsole -locale locale -OVERRIDELANGUAGE={locale} -Subtitles 20 -NOFORCEFEEDBACK -TELEMOPTIN 0";
                 }
            
                 me2game.Start();

                 
            
                
			    }
                if (choice == "ME3" || choice == "-ME3")
			    {
                
                  string promtw = args[1];
                  if (promtw.ToLower() == "yes" || promtw.ToLower() == "-yes")
                  {
                    ForceFeedback = true;
                  }
                  else
                  {
                    ForceFeedback = false;
                  }
                 string locale = args[2].Replace("-","");
                 if (locale == null || locale.Length == 0) 
                 {
                    Console.Write("Language argument was not passed down! Please enter your language code: ");
                    locale = Console.ReadLine();
                 }
                
                 
                 me3game.StartInfo.FileName = Path.Join(gamePath, "/Game/ME3/Binaries/Win64/MassEffect3.exe");

                 if (ForceFeedback == true)
				 {
                    me3game.StartInfo.Arguments = $"-NoHomeDir -SeekFreeLoadingPCConsole -locale locale -language={locale} -Subtitles 20 -TELEMOPTIN 0";
				 }
                 else {
                    me3game.StartInfo.Arguments = $"-NoHomeDir -SeekFreeLoadingPCConsole -locale locale -language={locale} -Subtitles 20 -NOFORCEFEEDBACK -TELEMOPTIN 0";
                 }
                 
            
                 me3game.Start();

                 

                }
                if (choice == "OLDME1" || choice == "-OLDME1") 
                {
                    if (File.Exists(Directory.GetCurrentDirectory() + @"\me1gamepath.txt")) {
                        TextReader tr = new StreamReader("me1gamepath.txt");
                        string gamePathreadme1 = tr.ReadLine();
                        legacygamePathme1 = gamePathreadme1;
                        tr.Close();
                    }
                    else
					{
                        Console.Write("Please enter your legacy Mass Effect 1 gamepath - ");
                        legacygamePathme1 = Console.ReadLine();
                        TextWriter tw = new StreamWriter("me1gamepath.txt");
                        tw.WriteLine(gamePath);
                        tw.Close();
					}
                    lmexgame.StartInfo.FileName = Path.Join(legacygamePathme1, "/Binaries/Win32/MassEffect.exe");
                    lmexgame.Start();
                }
                if (choice == "OLDME2" || choice == "-OLDME2") 
                {
                    if (File.Exists(Directory.GetCurrentDirectory() + @"\me2gamepath.txt")) {
                        TextReader tr = new StreamReader("me2gamepath.txt");
                        string gamePathreadme2 = tr.ReadLine();
                        legacygamePathme2 = gamePathreadme2;
                        tr.Close();
                    }
                    else
					{
                        Console.Write("Please enter your legacy Mass Effect 2 gamepath - ");
                        legacygamePathme2 = Console.ReadLine();
                        TextWriter tw = new StreamWriter("me2gamepath.txt");
                        tw.WriteLine(gamePath);
                        tw.Close();
					}
                    lmexgame.StartInfo.FileName = Path.Join(legacygamePathme2, "/Binaries/Win32/MassEffect2.exe");
                    lmexgame.Start();
                }
                if (choice == "OLDME3" || choice == "-OLDME3") 
                {
                    if (File.Exists(Directory.GetCurrentDirectory() + @"\me3gamepath.txt")) {
                        TextReader tr = new StreamReader("me3gamepath.txt");
                        string gamePathreadme3 = tr.ReadLine();
                        legacygamePathme3 = gamePathreadme3;
                        tr.Close();
                    }
                    else
					{
                        Console.Write("Please enter your legacy Mass Effect 3 gamepath - ");
                        legacygamePathme3 = Console.ReadLine();
                        TextWriter tw = new StreamWriter("me3gamepath.txt");
                        tw.WriteLine(gamePath);
                        tw.Close();
                    }
                    lmexgame.StartInfo.FileName = Path.Join(legacygamePathme3, "/Binaries/Win32/MassEffect3.exe");
                    lmexgame.Start();
                }
                if (args[3] == "silent" || args[3] == "-silent" || args[1] == "-silent")
			    {
                    Console.WriteLine();
                }
                else 
                {
                    Console.ReadLine();
                }
			}
                
		
            
		
            
		}
	}
}
