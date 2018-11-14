﻿using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SRBot.Misc;
using SpeedrunComSharp;

namespace SRBot
{
    class Program
    {
        public static Bot bot;
        public static SpeedrunComClient srclient;

        public static string token;
        public static ulong botOwnerId;

        public static DiscordColor embedColor;

        static void Main(string[] args)
        {
            Log("Starting...");

            FileManager.CheckDefaultFiles();
            Log("Checked default files.");

            dynamic json = JsonConvert.DeserializeObject(File.ReadAllText("files/config.json"));
            if (json.token == "")
            {
                Log("Error: You need to fill out the 'token' field in 'files/config.json' before starting.\n\n&7Press any key to continue...", "&c");
                Console.Read();
                return;
            }

            token = (string)json.token;
            botOwnerId = Convert.ToUInt64((string)json.botOwnerId);
            Log("Loaded token and bot owner ID.");

            srclient = new SpeedrunComClient();
            Log("Initialized SpeedrunCom Client.");

            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        
        static async Task MainAsync(string[] args)
        {
            Log("MainAsync started.", "&2");

            Log("Creating bot object.", "&2");

            embedColor = new DiscordColor("#185696");

            bot = new Bot(token, "Speedrun.com", ActivityType.Watching, UserStatus.Online);

            await Task.Delay(-1);
        }

        public static void Log(string message, string color = "&7")
        {
            FConsole.WriteLine($"{color}[{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt")}]%0&f {message}");
        }
    }
}
