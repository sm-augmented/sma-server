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
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildPresences
            });

            //discord.PresenceUpdated += Discord_PresenceUpdated;
        }

        private async Task Discord_PresenceUpdated(DiscordClient sender, DSharpPlus.EventArgs.PresenceUpdateEventArgs e)
        {
            var server = await discord.GetGuildAsync(916366594693816330);
            var playingRole = server.Roles.ContainsKey(978257556428447814) ? server.Roles[978257556428447814] : null;
            var user = e.User ?? e.UserAfter;

            if (user != null && playingRole != null)
            {
                var member = await server.GetMemberAsync(user.Id);

                if (e.PresenceAfter?.Activities?.Any(x => x.Name == "Space Marine Augmented") ?? false)
                {
                    if (!member.Roles.Any(x => x.Id == playingRole.Id))
                    {
                        await member.GrantRoleAsync(playingRole, "From Bot");
                    }
                }
                else
                {
                    if (member.Roles.Any(x => x.Id == playingRole.Id))
                    {
                        await member.RevokeRoleAsync(playingRole, "From Bot");
                    }
                }
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await discord.ConnectAsync();

            var server = await discord.GetGuildAsync(916366594693816330); //SR - 972860193727729714; SMA - 916366594693816330
            var channel = server.GetChannel(978227131316334612); //SR - 978216409727385670; SMA - 978227131316334612

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
                        var grPlayers = string.Join(", ", group.Select(x => x.Value.ID));
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
