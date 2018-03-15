﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace LunaManager
{
    class MainMenu
    {
        static void Main()

        {
            processCheck();
            LunaCheck();
            Menu();

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
        private static void Menu()
        {
            Console.WriteLine("Luna Manager for Kerbal Space Program.");
            Console.WriteLine("Here are your options:");
            showCommands();
            Console.WriteLine("Please enter a number to decide");
            var input = double.Parse(Console.ReadLine());
            if (input == 1)
            {
                processCheck();
                kerbalLaunch();
            }
            if (input == 2)
            {
                processCheck();
                LunaCheck();
                lunaMultiplayerUpdate();
            }
            else
                Console.WriteLine("Invalid Option");
            Menu();
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


        private static void lunaMultiplayerUpdate()
        {
            string lunaUpdater = @"Updater.exe";

            processCheck();
            LunaCheck();
            Process.Start(lunaUpdater);
            Menu();

        }
        private static void LunaCheck()
        {
            string lunaUpdater = @"Updater.exe";

            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), lunaUpdater)))
            {
                Console.WriteLine(" The \"Updater.exe\" is not in the main KSP folder");
                Console.WriteLine("Installing Luna Updater....");
                WebClient wb = new WebClient();
                wb.DownloadFile("https://github.com/LunaMultiplayer/LunaMultiplayerUpdater/releases/download/1.0.0/LunaMultiplayerUpdater-Release.zip", "LunaMultiplayerUpdater-Release.zip");
               
                string zipPath = Path.Combine(Directory.GetCurrentDirectory(), "LunaMultiplayerUpdater-Release.zip");
                string extractPath = Directory.GetCurrentDirectory();

                Console.WriteLine("Download completed. Extraction started.");
                //Get a unzipper class as 

            }
            else
            {
                Console.WriteLine("Luna Updater has been found! Yay");
            }
        
        }
    }
}


