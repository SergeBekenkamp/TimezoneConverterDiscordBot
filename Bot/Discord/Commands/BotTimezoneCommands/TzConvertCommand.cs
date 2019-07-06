using System;
using System.Globalization;
using System.Threading.Tasks;
using Bot.Data;
using Bot.Logger.Interfaces;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NodaTime;

namespace Bot.Discord.Commands.BotTimezoneCommands
{
    [Name("Timezone Converter")]
    public class TzConvertCommand : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger _logger;
        private readonly BotContext _botContext;
        private readonly DiscordShardedClient _shardedClient;
        private readonly EmbedBuilder _embed;

        private string _message = string.Empty;

        public TzConvertCommand(ILogger logger, BotContext botContext, DiscordShardedClient shardedClient)
        {
            _logger = logger;
            _botContext = botContext;
            _shardedClient = shardedClient;
            _embed = new EmbedBuilder();
        }

        /// <summary>
        /// Sends bot info about the current client.
        /// </summary>
        [Command("TzConvert", RunMode = RunMode.Async)]
        public async Task TzConvertAsync(string[] strings)
        {
            Instant? localTime = null;
            int dateLocation;
            var timezone = await GetUserTimeZone();
            for (int i = 0; i < strings.Length; i++)
            {
                if (DateTime.TryParse(strings[i], CultureInfo.CurrentCulture, DateTimeStyles.AdjustToUniversal, out var parsedTime))
                {
                    var lt = LocalDateTime.FromDateTime(parsedTime, CalendarSystem.Iso);
                    localTime = lt.InZone(timezone, x=> x.First()).ToInstant();
                    break;
                };
            }
            if (!localTime.HasValue)
            {
                await ReplyAsync("Failed to find / convert time to timezones", false, null).ConfigureAwait(false);
                return;
            }

           

            _embed.WithThumbnailUrl(_shardedClient.CurrentUser.GetAvatarUrl());
            _embed.WithTitle("Timezone conversions for: " + localTime.Value.ToString("HH:mm", CultureInfo.CurrentCulture) + " UTC");
            foreach (var zone in await TimeZonesForUser())
            {
                var xOffset = localTime.Value.InZone(zone);
                var str = xOffset.ToString("HH:mm", CultureInfo.CurrentCulture);
                _embed.AddField(zone.Id, str);
            }

            _embed.WithColor(new Color(255, 255, 255));
            _embed.WithCurrentTimestamp();
            await ReplyAsync(_message, false, _embed.Build()).ConfigureAwait(false);
            _logger.LogCommandUsed(Context.Guild.Id, Context.Client.ShardId, Context.Channel.Id, Context.User.Id, "BotInfo");
        }

        private async Task<DateTimeZone> GetUserTimeZone()
        {
            var user = await _botContext.Users.FindAsync(this.Context.User.Id);
            if (user != null) return DateTimeZoneProviders.Tzdb[user.TimezoneId];

            _message = "It looks like you havent set a timezone yet use !!tzset to set your local timezone";
            return DateTimeZoneProviders.Tzdb["Europe/London"];
        }

        private Task<DateTimeZone[]> TimeZonesForUser()
        {
            return Task.FromResult(new DateTimeZone[]
            {
                DateTimeZoneProviders.Tzdb["Europe/London"],
                DateTimeZoneProviders.Tzdb["Europe/Amsterdam"],
                DateTimeZoneProviders.Tzdb["Europe/Moscow"],
                DateTimeZoneProviders.Tzdb["Asia/Tokyo"],
                DateTimeZoneProviders.Tzdb["America/New_York"],
                DateTimeZoneProviders.Tzdb["America/Los_Angeles"],
            });
        }

    }
}
