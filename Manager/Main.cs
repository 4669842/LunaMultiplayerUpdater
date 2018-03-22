using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LunaManager
{
    /// <summary>
    ///   <para>Contains main method. Uses inheritence to build up menu.</para>
    ///   <para>Contains checks and validators for KSP.</para>
    /// </summary>
    class MainMenu
    {
        public const string ApiUrl = "https://github.com/LunaMultiplayer/LunaMultiplayerUpdater/releases/download/1.0.0";
        public const string FileName = "LunaMultiplayerUpdater-Release.zip";

        private static Thread thread;
        public static string ClientFolderToDecompress = Path.Combine(Path.GetTempPath(), "LMPClientUpdater");
        public static string ProjectUrl = ($"{ApiUrl}/LunaMultiplayerUpdater-Release.zip");
        public static string ServerFolderToDecompress = Path.Combine(Path.GetTempPath(), "LMPServerUpdater");

        private static void CleanTempClientFiles()
        {
            try
            {
                if(Directory.Exists(ClientFolderToDecompress))
                    Directory.Delete(ClientFolderToDecompress, true);
            } catch(Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
            }

            File.Delete(Path.Combine(Path.GetTempPath(), FileName));
        }

        private static void CleanTempServerFiles()
        {
            try
            {
                if(Directory.Exists(ServerFolderToDecompress))
                    Directory.Delete(ServerFolderToDecompress, true);
            } catch(Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
            }

            File.Delete(Path.Combine(Path.GetTempPath(), FileName));
        }

        private static void ClearScreen()
        {
            Console.ResetColor();
            Console.Clear();
        }

        private static void ClientMenu()
        {
            Console.WriteLine("Welcome to a Kerbal Space Program CLI. This is for actively updating Luna Multiplayer during beta testing. \nBelow are some options that will hopefully make things a lot more simpler.");

            Console.WriteLine("Options:");
            Console.ResetColor();
            ShowClientCommands();

            Console.WriteLine("Enter a number to choose:");
            var input = int.Parse(Console.ReadLine());
            if(input == 1)
            {
                ClearScreen();
                KerbalSafeLaunch();
            }
            if(input == 2)
            {
                ClearScreen();
                lunaSafeClientUpdate();
            }
            if(input == 3)
            {
                ClearScreen();
                ServerMenu();
            } else
                Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Invalid Option\n");
            Console.ResetColor();
            ClientMenu();
        }

        private static void ConfigureServer()
        {
            ClearScreen();
            Console.WriteLine("Welcome to the luna server configurator! You can either load a pre existing configuration or create a new one!");
            Console.WriteLine("============= Feature Coming Soon =============");
        }

        private static void CopyClientFilesFromTempToDestination()
        {
            var productFolderName = "LMPClientUpdater";
            foreach(var dirPath in Directory.GetDirectories(Path.Combine(ClientFolderToDecompress, productFolderName),
                                                            "*",
                                                            SearchOption.AllDirectories))
            {
                var destFolder = dirPath.Replace(Path.Combine(ClientFolderToDecompress, productFolderName),
                                                 Directory.GetCurrentDirectory());
                Console.WriteLine($"Creating dest folder: {destFolder}");
                Directory.CreateDirectory(destFolder);
            }
            foreach(var newPath in Directory.GetFiles(Path.Combine(ClientFolderToDecompress, productFolderName),
                                                      "*.*",
                                                      SearchOption.AllDirectories))
            {
                var destPath = newPath.Replace(Path.Combine(ClientFolderToDecompress, productFolderName),
                                               Directory.GetCurrentDirectory());
                Console.WriteLine($"Copying {Path.GetFileName(newPath)} to {destPath}");
                File.Copy(newPath, destPath, true);
            }
        }

        private static void CopyServerFilesFromTempToDestination()
        {
            var productFolderName = "LMPServerUpdater";
            var serverFolder = (Directory.GetCurrentDirectory() + "\\Server");
            foreach(var dirPath in Directory.GetDirectories(Path.Combine(ServerFolderToDecompress, productFolderName),
                                                            "*",
                                                            SearchOption.AllDirectories))
            {
                string destFolder = dirPath.Replace(Path.Combine(ServerFolderToDecompress, productFolderName),
                                                    serverFolder);
                Console.WriteLine($"Creating dest folder: {destFolder}");
            }

            foreach(var newPath in Directory.GetFiles(Path.Combine(ServerFolderToDecompress, productFolderName),
                                                      "*.*",
                                                      SearchOption.AllDirectories))
            {
                var destPath = newPath.Replace(Path.Combine(ServerFolderToDecompress, productFolderName), serverFolder);
                Console.WriteLine($"Copying {Path.GetFileName(newPath)} to {destPath}");
                File.Copy(newPath, destPath, true);
            }
        }

        private static async Task<string> GetDownloadUrl(HttpClient client)
        {
            using(HttpResponseMessage response = (await client.GetAsync(ProjectUrl)))
            {
                response.EnsureSuccessStatusCode();

                string content = (await response.Content.ReadAsStringAsync());
            }

            return null;
        }

        private static void InstallDirCheck()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string folder = new DirectoryInfo(path).Name;

            string target = @"Kerbal Space Program";
            if(folder != target)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("HALT\nThis is not the Kerbal Space Program Folder! ");
                Console.WriteLine("The manager will now end until this is resolved.");
                var input = Console.ReadLine();
                Environment.Exit(0);
            }
        }

        private static void KerbalLaunch()
        {
            InstallDirCheck();
            SanityCheck();
            Console.BackgroundColor = ConsoleColor.Green;
            Console.WriteLine("Booting Kerbal Space Program");
            string kerbal64 = @"KSP_x64.exe";
            if(File.Exists(kerbal64))
                Process.Start(kerbal64); else
                Console.WriteLine("Can not start Kerbal Space Program. Did you place this in the KSP installation folder?");
            Console.ResetColor();
            ClientMenu();
        }

        private static void KerbalSafeLaunch()
        {
            ClearScreen();
            SanityCheck();
            InstallDirCheck();
            LunaClientCheck();
            KerbalLaunch();
        }

        private static void LunaClientCheck()
        {
            string lunaUpdater = @"ClientUpdater.exe";

            if(!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), lunaUpdater)))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" The \"ClientUpdater.exe\" is not in the main KSP folder");
                Console.ResetColor();
                Console.WriteLine("Installing Luna Updater....");
                string zipPath = Path.Combine(Directory.GetCurrentDirectory(), "LunaMultiplayerUpdater-Release.zip");
                string extractPath = Directory.GetCurrentDirectory();
                ProjectUrl = "https://github.com/LunaMultiplayer/LunaMultiplayerUpdater/releases/download/1.0.0/LunaMultiplayerUpdater-Release.zip";
                WebClient wb = (new WebClient());

                if (!string.IsNullOrEmpty(ProjectUrl))
                {
                    Console.WriteLine($"Downloading LMP from: {ProjectUrl} Please wait...");
                    try
                    {
                        CleanTempClientFiles();
                        using (var client = (new WebClient()))
                        {
                            client.DownloadFile(ProjectUrl, Path.Combine(Path.GetTempPath(), FileName));
                            Console.WriteLine($"Downloading succeeded! Path: {Path.Combine(Path.GetTempPath(), FileName)}");
                        }

                        Console.WriteLine($"Decompressing file to {ClientFolderToDecompress}");
                        ZipFile.ExtractToDirectory(Path.Combine(Path.GetTempPath(), FileName), ClientFolderToDecompress);
                        DownloadAndReplaceFiles(ProductToDownload.Client);
                        CopyClientFilesFromTempToDestination();
                        Console.BackgroundColor = ConsoleColor.DarkGreen;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("-----------------===========FINISHED===========-----------------");
                        Console.ResetColor();
                    }
                    catch (Exception e)
                    {
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.WriteLine(e);
                        CleanTempClientFiles();
                        throw;
                    }
                    finally
                    {
                        CleanTempClientFiles();
                    }
                }
            }
            else
            {
                ClearScreen();
            }
        }

        private static void LunaClientUpdate()
        {
            string lunaClientUpdater = @"ClientUpdater.exe";
            InstallDirCheck();
            SanityCheck();
            LunaClientCheck();
            var lunaClientProcess = (new Process());
            lunaClientProcess.StartInfo = (new ProcessStartInfo(lunaClientUpdater)
            {
                UseShellExecute = false
            });

            lunaClientProcess.Start();
            lunaClientProcess.WaitForExit();
            ClientMenu();
        }

        private static void lunaSafeClientUpdate()
        {
            ClearScreen();
            SanityCheck();
            InstallDirCheck();
            LunaClientCheck();
            LunaClientUpdate();
        }

        private static void LunaSafeServerUpdate()
        {
            ClearScreen();
            SanityCheck();
            installed++;
            Console.WriteLine(installed);
            LunaServerUpdate();
        }

        private static void LunaServerCheck()
        {
            string lunaUpdater = @"ServerUpdater.exe";
            if(!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Server", lunaUpdater)))
            {
                installed = 0;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" The file \"ServerUpdater.exe\" is not in the Luna Server folder...");
                Console.ResetColor();
                Console.WriteLine("Installing Luna Updater....");

                string zipPath = Path.Combine(Directory.GetCurrentDirectory(), "LunaMultiplayerUpdater-Release.zip");
                string extractPath = (Directory.GetCurrentDirectory() + "\\Server");
                Directory.CreateDirectory(extractPath);
                ProjectUrl = "https://github.com/LunaMultiplayer/LunaMultiplayerUpdater/releases/download/1.0.0/LunaMultiplayerUpdater-Release.zip";
                WebClient wb = (new WebClient());
                Console.WriteLine($"Downloading LMP from: {ProjectUrl} Please wait...");
                try
                {
                    CleanTempServerFiles();
                    using (var server = (new WebClient()))
                    {
                        server.DownloadFile(ProjectUrl, Path.Combine(Path.GetTempPath(), FileName));
                        Console.WriteLine($"Downloading succeeded! Path: {Path.Combine(Path.GetTempPath(), FileName)}");
                    }

                    Console.WriteLine($"Decompressing file to {ServerFolderToDecompress}");
                    ZipFile.ExtractToDirectory(Path.Combine(Path.GetTempPath(), FileName), ServerFolderToDecompress);
                    DownloadAndReplaceFiles(ProductToDownload.Server);
                    CopyServerFilesFromTempToDestination();
                    string ServerExeStub = @"\Server\Server.exe";
                    if (!File.Exists(ServerExeStub))
                    {
                        using (FileStream fs = File.Create(Directory.GetCurrentDirectory() + ServerExeStub))
                        {
                            Byte[] exeStub = new UTF8Encoding(true).GetBytes("Hi, thanks for all the fish! I am a stub until you boot the Luna Manager or run the  Server Updater independently. I do nothing, but tell our updaters that I exist! And should update the folder I am sat in to be filled with Luna server files! If you are reading me then you have not started using the Luna Manager to operate the server which will upset the great god Zeus and he shall rain a eternal damnation of you and your kerbals. Your Pa's will never be perfectly circlular and you will be forced to forget vital parts of your staging causing complete restarts of your launches! Now, I dont think anyone wants to go through that. I personally would hate to see someone fight fruitlessly towards the great gods and be banished from being able to reach the stars. But, This is clearly a warning to be taken seriously before its too late!");
                            // Luna Updater requires Server.exe to be in directory, this creates the application as a stub for update.
                            fs.Write(exeStub, 0, exeStub.Length);
                        }
                    }
                    installed++;
                    Console.BackgroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine("-----------------===========FINISHED===========-----------------");
                    Console.ResetColor();
                }
                catch (Exception e)
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.WriteLine(e);
                    CleanTempServerFiles();
                    throw;
                }
                finally
                {
                    installed = 1;
                    CleanTempServerFiles();
                }
            }
            else
            {
                installed = 1;
                ClearScreen();
            }
        }

        private static void LunaServerUpdate()
        {
            SanityCheck();
            LunaServerCheck();
            string lunaServerUpdater = @"ServerUpdater.exe";
            var lunaServerUpdateProcess = (new Process());
            lunaServerUpdateProcess.StartInfo = (new ProcessStartInfo(lunaServerUpdater)
            {
                WorkingDirectory = @"Server",
                FileName = lunaServerUpdater,
                CreateNoWindow = false,
                UseShellExecute = true
            });
            lunaServerUpdateProcess.Start();
            lunaServerUpdateProcess.WaitForExit();

            ServerMenu();
        }

        [STAThread]
        static void Main()
        {
            String[] arguments = Environment.GetCommandLineArgs();
            foreach(String CommandArgs in arguments)
            {
                if(arguments.Contains("-server"))
                {
                    LunaServerCheck();
                    LunaSafeServerUpdate();
                    RunLunaServer();
                }
                if(arguments.Contains("-client"))
                {
                    LunaClientCheck();
                    lunaSafeClientUpdate();
                }
            }
            InstallDirCheck();
            SanityCheck();
            LunaClientCheck();
            ClientMenu();
        }

        private static void RunLunaServer()
        {
            SanityCheck();

            try
            {
                string lunaServer = @"Server.exe";
                var lunaServerProcess = (new Process());
                lunaServerProcess.StartInfo = (new ProcessStartInfo(lunaServer)
                {
                    WorkingDirectory = @"Server",
                    FileName = lunaServer,
                    CreateNoWindow = false,
                    UseShellExecute = true
                });
                lunaServerProcess.Start();
                lunaServerProcess.WaitForExit();
            } catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            ServerMenu();
        }

        private static void SanityCheck()
        {
            try
            {
                foreach(Process proc in Process.GetProcessesByName("KSP_x64"))
                {
                    proc.Kill();
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.WriteLine("Kerbal Space Program was found running and has been killed.");
                    Console.ResetColor();
                }
            } catch(Exception ex)
            {
                Console.Write(ex.Message);
            }
            try
            {
                foreach(Process proc in Process.GetProcessesByName("KSP"))
                {
                    proc.Kill();
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.WriteLine("Kerbal Space Program was found running and has been killed.");
                    Console.ResetColor();
                }
            } catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(ex.Message);
            }
            try
            {
                foreach(Process proc in Process.GetProcessesByName("Updater"))
                {
                    proc.Kill();
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.WriteLine("Luna Updater was found running and has been killed.");
                    Console.ResetColor();
                }
            } catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(ex.Message);
            }
        }

        private static void ServerMenu()
        {
            thread = (new Thread(ServerMenu));
            LunaServerCheck();
            Console.WriteLine("Here you can operate and manage your Luna Multiplayer servers by choosing one of the options below.");
            Console.WriteLine("Options:");
            Console.ResetColor();
            ShowServerCommands();

            Console.WriteLine("Enter a number to choose:");
            var input = int.Parse(Console.ReadLine());
            if(input == 1)
            {
                LunaSafeServerUpdate();
            }
            if(input == 2)
            {
                thread = (new Thread(RunLunaServer));
                ClearScreen();

                RunLunaServer();
            }
            if(input == 3)
            {
                ConfigureServer();
            }
            if(input == 4)
            {
                ClearScreen();
                ClientMenu();
            }
            if(input == 000)
            {
                ClearScreen();
                UninstallLuna();
            } else
                Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Invalid Option\n");
            Console.ResetColor();
            ServerMenu();
        }

        private static void ShowClientCommands()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("1. Start up Kerbal Space Program ");
            Console.WriteLine("2. Install/Update LunaMultiplayer");
            Console.WriteLine("3. Run LunaMultiplayer Server");
            Console.ResetColor();
        }

        private static void ShowServerCommands()
        {
            int fCount = Directory.GetFiles("Server", "*", SearchOption.AllDirectories).Length;

            ClearScreen();
            Console.ForegroundColor = ConsoleColor.Magenta;
            if(fCount == 3)
            {
                Console.WriteLine("1. Install LunaMultiplayer");
            } else if(fCount > 3)
            {
                Console.WriteLine("1. Update LunaMultiplayer");
                Console.WriteLine("2. Start up Luna Server ");
                Console.WriteLine("3. Configure LunaMultiplayer");
            }
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("4. Return to Luna Client options");
            Console.ResetColor();
        }

        private static void UninstallLuna()
        {
            Console.WriteLine("============= Sad to see you go =============");
            Console.WriteLine("Which would you like to remove?");
            Console.WriteLine("1. Luna Client");
            Console.WriteLine("2. Luna Server");
            var input = int.Parse(Console.ReadLine());
            if(input == 1)
            {
                if(Directory.Exists(@"GameData\LunaMultiplayer"))
                {
                    Directory.Delete(@"GameData\LunaMultiplayer");
                }
                if(File.Exists(@"ClientUpdater.exe"))
                {
                    File.Delete(@"ClientUpdater.exe");
                }
                if(File.Exists(@"CommonUpdater.dll"))
                {
                    File.Delete(@"CommonUpdater.dll");
                }
            }
            if(input == 2)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("This will purge your /Server folder leaving anything not backed up, gone forever! Are you sure you wish to delete?");
                var confirm = Console.ReadLine();
                if((confirm == "y") | (confirm == "Y") | (confirm == "YES") | (confirm == "yes"))
                {
                    try
                    {
                        if(Directory.Exists(Directory.GetCurrentDirectory() + "\\Server"))
                        {
                            foreach(Process proc in Process.GetProcessesByName("LMPServer"))
                            {
                                proc.Kill();
                                Console.BackgroundColor = ConsoleColor.Green;
                                Console.WriteLine("Luna Server was found running and has been killed. Continuing to uninstall");
                                Console.ResetColor();
                            }
                            Directory.Delete(Directory.GetCurrentDirectory() + "\\Server");
                            Console.WriteLine("============= Files have been removed =============");
                        }
                    } catch(Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(ex.Message);
                    }
                } else
                {
                    Console.WriteLine("Removal Cancalled");
                }
            }
        }

        public static void DownloadAndReplaceFiles(ProductToDownload product)
        {
            string downloadUrl;
            using(var client = (new HttpClient { Timeout = TimeSpan.FromSeconds(30) }))
            {
                downloadUrl = GetDownloadUrl(client).Result;
            }

            if(!string.IsNullOrEmpty(downloadUrl))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Downloading LMP from: {downloadUrl} Please wait...");
                try
                {
                    CleanTempClientFiles();
                    CleanTempServerFiles();
                    using(var client = (new WebClient()))
                    {
                        client.DownloadFile(downloadUrl, Path.Combine(Path.GetTempPath(), FileName));
                        Console.WriteLine($"Downloading succeeded! Path: {Path.Combine(Path.GetTempPath(), FileName)}");
                    }

                    Console.WriteLine($"Decompressing file to {ClientFolderToDecompress}");
                    ZipFile.ExtractToDirectory(Path.Combine(Path.GetTempPath(), FileName), ClientFolderToDecompress);

                    CopyClientFilesFromTempToDestination();
                    CopyServerFilesFromTempToDestination();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("-----------------===========FINISHED===========-----------------");
                } catch(Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e);
                    Console.ResetColor();
                    throw;
                } finally
                {
                    CleanTempClientFiles();
                    CleanTempServerFiles();
                }
            }
        }

        public static object Downloader
        {
            get; private set;
        }

        public static int installed
        {
            get; private set;
        }

        public static string lunaUpdater
        {
            get; private set;
        }

        public static object Product
        {
            get; private set;
        }

        public enum ProductToDownload
        {
            Client,
            Server
        }
    }
}


