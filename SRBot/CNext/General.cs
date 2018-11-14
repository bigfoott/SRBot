using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeedrunComSharp;
using DSharpPlus.Entities;
using SRBot.Misc;

namespace SRBot.CNext
{
    class General : BaseCommandModule
    {
        List<User> users = new List<User>();

        [Command("user")]
        public async Task Leaderboard(CommandContext ctx, string username = "")
        {
            if (username == "") await Bot.SendError(ctx, "User Lookup", "Missing: `Username`");
            else
            {
                var users = Program.srclient.Users.GetUsers(name: username);
                var user = users.FirstOrDefault(u => u.Name.ToLower() == username.ToLower());

                if (user == null) await Bot.SendError(ctx, "User Lookup", "Couldn't find user.");
                else
                {
                    string runString = "";
                    var runs = user.Runs.ToArray();
                    int count = 0;
                    foreach (Run r in runs.OrderBy(r => r.DateSubmitted.Value))
                    {
                        if (count >= 5) break;
                        
                        int index = r.ToString().LastIndexOf(" in ");
                        string run = r.ToString().Substring(0, index);
                        var timespan = TimeSpan.Parse(r.ToString().Substring(index + 4));

                        runString += $"[**{run}** in ";
                        if (timespan.Days > 0) runString += $"{timespan.Days}d {timespan.Hours}h {timespan.Minutes}m {timespan.Seconds}s\n";
                        else if (timespan.Hours > 0) runString += $"{timespan.Hours}h {timespan.Minutes}m {timespan.Seconds}s\n";
                        else if (timespan.Minutes > 0) runString += $"{timespan.Minutes}m {timespan.Seconds}s\n";
                        else runString += $"{timespan.Seconds}s\n";
                        runString += $"](http://bigft.io/sr?r={r.ID})";
                        count++;
                    }
                    
                    string pbString = "";
                    count = 0;
                    foreach (Record r in user.PersonalBests.OrderBy(r => r.DateSubmitted.Value))
                    {
                        if (count >= 5) break;
                        
                        int index = r.ToString().LastIndexOf(" in ");
                        string run = r.ToString().Substring(0, index);
                        var timespan = TimeSpan.Parse(r.ToString().Substring(index + 4));

                        pbString += "[";
                        if (r.Rank == 1) pbString += "<:1st:422125427809714189> ";
                        else if (r.Rank == 2) pbString += "<:2nd:422125428317356053> ";
                        else if (r.Rank == 3) pbString += "<:3rd:422125465185288192> ";

                        pbString += $"{r.Rank.WithSuffix()} • **{run}** in ";
                        if (timespan.Days > 0) pbString += $"{timespan.Days}d {timespan.Hours}h {timespan.Minutes}m {timespan.Seconds}s\n";
                        else if (timespan.Hours > 0) pbString += $"{timespan.Hours}h {timespan.Minutes}m {timespan.Seconds}s\n";
                        else if (timespan.Minutes > 0) pbString += $"{timespan.Minutes}m {timespan.Seconds}s\n";
                        else pbString += $"{timespan.Seconds}s\n";
                        pbString += $"](http://bigft.io/sr?r={r.ID})";
                        count++;
                    }

                    DiscordColor color = default;
                    if (user.NameStyle.IsGradient) color = new DiscordColor(user.NameStyle.DarkGradientStartColorCode);
                    else color = new DiscordColor(user.NameStyle.DarkSolidColorCode);
                    
                    var embed = new DiscordEmbedBuilder()
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor()
                        {
                            Name = user.Name,
                            Url = user.WebLink.OriginalString
                        },
                        Color = color,
                        Description = $":flag_{user.Location.Country.Code}: "
                    };
                    if (runString.Length > 1024) runString = runString.Substring(0, 1024);
                    embed.AddField("Latest PBs", pbString);
                    embed.AddField("Latest Runs", runString);

                    await ctx.RespondAsync("", embed: embed);
                }
            }
        }
    }
}
