﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.IO.Compression;
using System.Threading;
using System.Windows.Forms;
using System.Net.Http;
using System.Threading.Tasks;

namespace LunaManager
{
    class MainMenu
    {
        private static Thread thread;
        public const string ApiUrl = "https://github.com/LunaMultiplayer/LunaMultiplayerUpdater/releases/download/1.0.0";
        public static string FolderToDecompress = Path.Combine(Path.GetTempPath(), "LMPClientUpdater");
        public const string FileName = "LunaMultiplayerUpdater-Release.zip";
        public static string ProjectUrl = $"{ApiUrl}/LunaMultiplayerUpdater-Release.zip";
        public static object Downloader { get; private set; }
        public static object product { get; private set; }

        [STAThread]
        static void Main()
        {
            
     
            thread = new Thread(BusyWorkThread);
            processCheck();
            LunaCheck();
            Menu();

        }

        public enum ProductToDownload
        {
            Client
        }

        private static void processCheck()
        {
            try
            {
                foreach (Process proc in Process.GetProcessesByName("KSP_x64"))
                {
                    proc.Kill();
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            try
            {
                foreach (Process proc in Process.GetProcessesByName("KSP"))
                {
                    proc.Kill();
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            try
            {
                foreach (Process proc in Process.GetProcessesByName("Updater"))
                {
                    proc.Kill();
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }

        }
        
        private static void kerbalSafeLaunch()
        {
            clearScreen();
            processCheck();
            LunaCheck();
            kerbalLaunch();
        }
        private static void lunaSafeUpdate()
        {
            clearScreen();
            processCheck();
            LunaCheck();
            lunaMultiplayerUpdate();
        }
        private static void Menu()
        {
            Console.WriteLine("Welcome to a Kerbal Space Program CLI. This is for actively updating Luna Multiplayer during beta testing. \nBelow are some options that will hopefully make things a lot more simpler.");
            Console.WriteLine("Here are your options:");
            showCommands();
            Console.WriteLine("Enter a number to choose:");
            var input = double.Parse(Console.ReadLine());
            if (input == 1)
            {
                kerbalSafeLaunch();

            }
            if (input == 2)
            {
               lunaSafeUpdate();
            }
            else
                clearScreen();
            Console.WriteLine("Invalid Option\n");
            Menu();
        }


        private static void CopyFilesFromTempToDestination(ProductToDownload product)
        {
            var productFolderName = ("LMPClientUpdater");
            foreach (var dirPath in Directory.GetDirectories(Path.Combine(FolderToDecompress, productFolderName), "*", SearchOption.AllDirectories))
            {
                var destFolder = dirPath.Replace(Path.Combine(FolderToDecompress, productFolderName), Directory.GetCurrentDirectory());
                Console.WriteLine($"Creating dest folder: {destFolder}");
                Directory.CreateDirectory(destFolder);
            }

            foreach (var newPath in Directory.GetFiles(Path.Combine(FolderToDecompress, productFolderName), "*.*", SearchOption.AllDirectories))
            {
                var destPath = newPath.Replace(Path.Combine(FolderToDecompress, productFolderName), Directory.GetCurrentDirectory());
                Console.WriteLine($"Copying {Path.GetFileName(newPath)} to {destPath}");
                File.Copy(newPath, destPath, true);
            }
        }


        private static void CleanTempFiles()
        {
            try
            {
                if (Directory.Exists(FolderToDecompress))
                    Directory.Delete(FolderToDecompress, true);
            }
            catch (Exception)
            {
                // ignored
            }

            File.Delete(Path.Combine(Path.GetTempPath(), FileName));
        }


        private static async Task<string> GetDownloadUrl(HttpClient client)
        {
            using (var response = await client.GetAsync(ProjectUrl))
            {
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
            }

            return null;
        }

        public static void DownloadAndReplaceFiles(ProductToDownload product)
        {
            string downloadUrl;
            using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
            {
                downloadUrl = GetDownloadUrl(client).Result;
            }

            if (!string.IsNullOrEmpty(downloadUrl))
            {
                Console.WriteLine($"Downloading LMP from: {downloadUrl} Please wait...");
                try
                {
                    CleanTempFiles();
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(downloadUrl, Path.Combine(Path.GetTempPath(), FileName));
                        Console.WriteLine($"Downloading succeeded! Path: {Path.Combine(Path.GetTempPath(), FileName)}");
                    }

                    Console.WriteLine($"Decompressing file to {FolderToDecompress}");
                    ZipFile.ExtractToDirectory(Path.Combine(Path.GetTempPath(), FileName), FolderToDecompress);

                    CopyFilesFromTempToDestination(product);

                    Console.WriteLine("-----------------===========FINISHED===========-----------------");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                finally
                {
                    CleanTempFiles();
                }
            }
        }
        public static void BusyWorkThread()
        {


        }

        private static void showCommands()
        {
            Console.WriteLine("1. Start up Kerbal Space Program ");
            Console.WriteLine("2. Install/Update LunaMultiplayer");
        }
        private static void kerbalLaunch()
        {

            processCheck();
            Console.WriteLine("Booting Kerbal Space Program");
            string kerbal64 = @"KSP_x64.exe";
            if (File.Exists(kerbal64))
                Process.Start(kerbal64);
            else
                Console.WriteLine("Can not start Kerbal Space Program. Did you place this in the KSP installation folder?");
            Menu();
        }
        private static void clearScreen()
        {
            Console.Clear();
        }

        private static void lunaMultiplayerUpdate()
        {
            string lunaUpdater = @"ClientUpdater.exe";

            processCheck();
            LunaCheck();
            Process.Start(lunaUpdater);
            Menu();

        }


        private static void LunaCheck()
        {
            string lunaUpdater = @"ClientUpdater.exe";

            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), lunaUpdater)))
            {
                Console.WriteLine(" The \"Updater.exe\" is not in the main KSP folder");
                Console.WriteLine("Installing Luna Updater...."); 
                string zipPath = Path.Combine(Directory.GetCurrentDirectory(), "LunaMultiplayerUpdater-Release.zip");
                string extractPath = Directory.GetCurrentDirectory();
                string downloadUrl;
                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
                {
                    downloadUrl = "https://github.com/LunaMultiplayer/LunaMultiplayerUpdater/releases/download/1.0.0/LunaMultiplayerUpdater-Release.zip";
                    WebClient wb = new WebClient();
                }

                if (!string.IsNullOrEmpty(downloadUrl))
                {
                    Console.WriteLine($"Downloading LMP from: {downloadUrl} Please wait...");
                    try
                    {
                        CleanTempFiles();
                        using (var client = new WebClient())
                        {
                            client.DownloadFile(downloadUrl, Path.Combine(Path.GetTempPath(), FileName));
                            Console.WriteLine($"Downloading succeeded! Path: {Path.Combine(Path.GetTempPath(), FileName)}");
                        }

                        Console.WriteLine($"Decompressing file to {FolderToDecompress}");
                        ZipFile.ExtractToDirectory(Path.Combine(Path.GetTempPath(), FileName), FolderToDecompress);
                        DownloadAndReplaceFiles(ProductToDownload.Client);
                        CopyFilesFromTempToDestination(product);

                        Console.WriteLine("-----------------===========FINISHED===========-----------------");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        CleanTempFiles();
                        throw;
                    }
                    finally
                    {
                        CleanTempFiles();
                    }
                }

            }
            else
            {
                clearScreen();
            }
        
        }

        private static void CopyFilesFromTempToDestination(object product)
        {
            var productFolderName = "LMPClientUpdater";
            foreach (var dirPath in Directory.GetDirectories(Path.Combine(FolderToDecompress, productFolderName), "*", SearchOption.AllDirectories))
            {
                var destFolder = dirPath.Replace(Path.Combine(FolderToDecompress, productFolderName), Directory.GetCurrentDirectory());
                Console.WriteLine($"Creating dest folder: {destFolder}");
                Directory.CreateDirectory(destFolder);
            }

            foreach (var newPath in Directory.GetFiles(Path.Combine(FolderToDecompress, productFolderName), "*.*", SearchOption.AllDirectories))
            {
                var destPath = newPath.Replace(Path.Combine(FolderToDecompress, productFolderName), Directory.GetCurrentDirectory());
                Console.WriteLine($"Copying {Path.GetFileName(newPath)} to {destPath}");
                File.Copy(newPath, destPath, true);
            }
        }
  
        private static object GetProductFolderName(ProductToDownload product)
        {
            switch (product)
            {
                case ProductToDownload.Client:
                    return "LMPClientUpdater";

                default:
                    throw new ArgumentOutOfRangeException(nameof(product), product, null);
            }
        }
    }
}



