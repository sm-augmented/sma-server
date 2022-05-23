using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Options;
using SMAServer.Config;
using SMAServer.Managers;

namespace SMAServer.Workers
{
    public class DiscordBotWorker : BackgroundService
    {
        private readonly PersistenceManager persistence;
        private readonly DiscordClient discord;

        public DiscordBotWorker(PersistenceManager persistence, IOptions<SecretsConfig> secrets)
        {
            this.persistence = persistence;

            discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = secrets.Value.DiscordToken,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged
            });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await discord.ConnectAsync();

            var server = await discord.GetGuildAsync(972860193727729714);
            var channel = server.GetChannel(978216409727385670);

            while (!stoppingToken.IsCancellationRequested)
            {
                var players = persistence.Players;

                var embed = new DiscordEmbedBuilder()
                    .WithTitle($"Online: {players.Count}");

                var groups = players.GroupBy(x => x.Value.Branch);
                var msgContent = "";                

                if (groups.Count() > 0)
                {
                    foreach (var group in groups)
                    {
                        var grPlayers = string.Join(", ", group.Select(x => x.Key));
                        msgContent += $"{group.Key}: {group.Count()} ({grPlayers})\n";
                    }
                }
                else
                {
                    msgContent = "No one is currently playing.";
                }

                embed.WithDescription(msgContent);

                var msgs = await channel.GetMessagesAsync();
                var msg = new DiscordMessageBuilder()
                    .WithEmbed(embed);

                if (msgs.Count > 0)
                {
                    var existingMsg = msgs.FirstOrDefault();
                    await msg.ModifyAsync(existingMsg);
                }
                else
                {
                    await msg.SendAsync(channel);
                }

                await Task.Delay(60000, stoppingToken);

            }

            await discord.DisconnectAsync();
        }
    }
}
