using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RickAstleyProject.VoiceRelated;
using System.Text;

namespace RickAstleyProject
{
    public class Bot
    {
        #pragma warning disable CS8618
        public DiscordClient Client { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        public VoiceNextExtension Voice { get; set; }
        public RickRoller Rick { get; set; } 

        public EventId BotEventId = new EventId(777, "RickRollBot");

        public async Task RunAsync()
        {
            var json = String.Empty;

            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            var config = new DiscordConfiguration
            {
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,
                Intents = DiscordIntents.GuildVoiceStates
                | DiscordIntents.All
            };

            Client = new DiscordClient(config);
            Client.Ready += Client_Ready;
            Client.GuildAvailable += Client_GuildAvailable;
            ActiveChannelDictionary.initialize(Client, BotEventId);

            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(1)
            });

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { configJson.Prefix },
                EnableMentionPrefix = true,
                DmHelp = true
            };

            Commands = Client.UseCommandsNext(commandsConfig);

            Commands.RegisterCommands<SimpleCommands>();

            Commands.CommandExecuted += Commands_CommandExecuted;

            Voice = Client.UseVoiceNext();

            await Client.ConnectAsync();

            await Task.Delay(-1);
        }

        private Task Client_GuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
        {
            sender.Logger.LogInformation(BotEventId, $"Guild available: {e.Guild.Name}");
            return Task.CompletedTask;
        }

        private Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            CommandContext ctx = e.Context;

            if (e.Command.Name == "rickroll")
            {
                if (!ActiveChannelDictionary.Current.ContainsKey(ctx.Guild))
                {
                    ctx.Channel.SendMessageAsync("No voice activity detected.");
                    return Task.CompletedTask;
                }

                Rick.RickRoll(ctx.Guild);
            }

            e.Context.Client.Logger.LogInformation(BotEventId, $"{ctx.User.Username} successfully executed '{e.Command.QualifiedName}'");
            return Task.CompletedTask;
        }

        private Task Client_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            sender.Logger.LogInformation(BotEventId, "Together Forever!");

            Rick = new RickRoller(Client, BotEventId);

            return Task.CompletedTask;
        }
    }
}
