using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Net.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SRBot.CNext;
using SRBot.Misc;

namespace SRBot
{
    class Bot
    {
        public DiscordClient client;
        public CommandsNextExtension cnext;

        public static DateTime startTime;

        public static int totalUsers = 0;
        public static int totalGuilds = 0;

        public static Dictionary<ulong, string> prefixes;

        public Bot(string token, string activity = "", ActivityType activityType = ActivityType.Playing, UserStatus status = UserStatus.Online)
        {
            var config = new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Critical,
                AutoReconnect = true
            };
            Program.Log("Initialized config object.", "&2");

            if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1)
            {
                config.WebSocketClientFactory = WebSocketSharpClient.CreateNew;
                Program.Log("Switched websocket to WebSocketSharp. (Windows 7)", "&a");
            }

            client = new DiscordClient(config);
            Program.Log("Initialized client.", "&2");
            
            cnext = client.UseCommandsNext(new CommandsNextConfiguration
            {
                CaseSensitive = false,
                EnableDms = false,
                EnableDefaultHelp = false,
                PrefixResolver = PrefixPredicateAsync,
                EnableMentionPrefix = true,
                IgnoreExtraArguments = true
            });

            cnext.RegisterCommands<General>();

            Program.Log("Initialized CNext extension.", "&2");

            client.GuildCreated += async e =>
            {
                if (Directory.Exists($"guilds/old/{e.Guild.Id}")) Directory.Move($"guilds/old/{e.Guild.Id}", $"guilds/{e.Guild.Id}");
                FileManager.CheckGuildFiles(e.Guild.Id);
                UpdateGuildPrefix(e.Guild.Id);
                UpdateCount();
            };
            client.GuildDeleted += async e =>
            {
                Directory.Move($"guilds/{e.Guild.Id}", $"guilds/old/{e.Guild.Id}");

                UpdateCount();
            };
            client.GuildAvailable += async e =>
            {
                FileManager.CheckGuildFiles(e.Guild.Id);
                UpdateGuildPrefix(e.Guild.Id);
                UpdateCount();
            };
            client.GuildMemberAdded += async e =>
            {
                if (!Perms.BotHasGuildPerm(e.Guild, Permissions.ManageRoles)) return;
                dynamic json = JsonConvert.DeserializeObject(File.ReadAllText($"guilds/{e.Guild.Id}/config.json"));
                if (UInt64.TryParse((string)json.autorole, out ulong id)) await e.Member.GrantRoleAsync(e.Guild.GetRole(id));
            };
            client.Ready += async e =>
            {
                await client.UpdateStatusAsync(new DiscordActivity(activity, activityType), status);

                prefixes = new Dictionary<ulong, string>();

                foreach (ulong id in e.Client.Guilds.Keys)
                {
                    FileManager.CheckGuildFiles(id);
                    UpdateGuildPrefix(id);
                }

                UpdateCount();

                startTime = DateTime.Now;
                Program.Log("Setup completed.", "%2&0");
            };
            Program.Log("Subscribed to events.", "&2");

            Program.Log("Connecting...", "&2");
            client.ConnectAsync();
            Program.Log("Connected.", "&2");
        }

        private static Task<int> PrefixPredicateAsync(DiscordMessage m)
        {
            string pref = ".";
            if (prefixes.ContainsKey(m.Channel.GuildId)) pref = prefixes[m.Channel.Guild.Id];

            return Task.FromResult(m.GetStringPrefixLength(pref));
        }

        private void UpdateCount()
        {
            totalGuilds = client.Guilds.Count;
            totalUsers = 0;
            foreach (DiscordGuild g in client.Guilds.Values)
                totalUsers += g.MemberCount;
        }

        public static async Task SendError(CommandContext ctx, string title, string message)
        {
            var embed = new DiscordEmbedBuilder()
            {
                Title = title,
                Description = "**ERROR**\n\n" + message,
                Color = DiscordColor.DarkRed
            };

            var msg = await ctx.RespondAsync("", embed: embed);
            await Task.Delay(new TimeSpan(0, 0, 5));
            await msg.DeleteAsync();
        }

        public static void UpdateGuildPrefix(ulong id, string _override = "")
        {
            if (prefixes.ContainsKey(id)) prefixes.Remove(id);
            if (_override == "") prefixes.Add(id, (string)((dynamic)JsonConvert.DeserializeObject(File.ReadAllText($"guilds/{id}/config.json"))).prefix);
            else prefixes.Add(id, _override);
        }
    }
}
