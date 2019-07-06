using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Bot.Data;
using Bot.Data.Models;
using Bot.Logger.Interfaces;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NodaTime;

namespace Bot.Discord.Commands.BotTimezoneCommands
{
    [Name("User Timezone Setter")]
    public class TzSetCommands : ModuleBase<SocketCommandContext>
    {

        private readonly ILogger _logger;
        private readonly BotContext _botContext;
        private readonly DiscordShardedClient _shardedClient;
        private readonly EmbedBuilder _embed;
        public TzSetCommands(ILogger logger, BotContext botContext, DiscordShardedClient shardedClient)
        {
            _logger = logger;
            _botContext = botContext;
            _shardedClient = shardedClient;
            _embed = new EmbedBuilder();
        }

        /// <summary>
        /// Sends bot info about the current client.
        /// </summary>
        [Command("TzSet", RunMode = RunMode.Async)]
        public async Task TzSetCommand(string timezone)
        {
            var zone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timezone);
            if (zone == null)
            {
                await ReplyAsync("Invalid timezone a list of all available timezones can be found here: https://gist.github.com/jrolstad/5ca7d78dbfe182d7c1be", false).ConfigureAwait(false);

                return;
            }

            var user = await _botContext.Users.FindAsync(this.Context.User.Id);
            var oldTimezone = "nothing";
            if (user == null)
            {
                user = new User
                {
                    UserId = this.Context.User.Id,
                    TimezoneId = timezone,
                };
                _botContext.Users.Add(user);
            }
            else
            {
                oldTimezone = user.TimezoneId;
                user.TimezoneId = timezone;
            }

            await _botContext.SaveChangesAsync();
            await ReplyAsync($"Successfully changed your timezone from {oldTimezone} to {timezone}", false).ConfigureAwait(false);
            _logger.LogCommandUsed(Context.Guild.Id, Context.Client.ShardId, Context.Channel.Id, Context.User.Id, "TzSet");
        }

        /// <summary>
        /// Sends bot info about the current client.
        /// </summary>
        [Command("TzGet", RunMode = RunMode.Async)]
        public async Task TzGetCommand()
        {
            var user = await _botContext.Users.FindAsync(this.Context.User.Id);
            var timezone = "nothing";
            if (user != null)
            {
                timezone = user.TimezoneId;
            }
            

            await _botContext.SaveChangesAsync();
            await ReplyAsync($"Your current timezone is: {timezone}", false).ConfigureAwait(false);
            _logger.LogCommandUsed(Context.Guild.Id, Context.Client.ShardId, Context.Channel.Id, Context.User.Id, "TzSet");
        }

        /// <summary>
        /// Sends bot info about the current client.
        /// </summary>
        [Command("Help", RunMode = RunMode.Async)]
        public async Task TzHelpCommand()
        {

            _embed.WithThumbnailUrl(_shardedClient.CurrentUser.GetAvatarUrl());
            _embed.WithTitle("Commands for Timezone bot");
           
            _embed.AddField("!!TzConvert 8am", "Converts a time ex: 8am to a list of timezones (no spaces please)");
            _embed.AddField("!!TzSet Europe/Amsterdam", "Sets the timezone used for converting when you use the command");
            _embed.AddField("!!TzGet", "Gets the timezone used for converting when you use !!tzconvert");
            _embed.AddField("Valid timezones", "https://gist.github.com/jrolstad/5ca7d78dbfe182d7c1be");


            _embed.WithColor(new Color(255, 255, 255));
            _embed.WithCurrentTimestamp();
            await ReplyAsync("", false, _embed.Build()).ConfigureAwait(false);
        }

    }
}
