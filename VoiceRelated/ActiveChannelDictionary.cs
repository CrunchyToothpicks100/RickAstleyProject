using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace RickAstleyProject.VoiceRelated
{
    #pragma warning disable CS8618
    public static class ActiveChannelDictionary
    {
        public static DiscordClient Client { get; private set; }
        public static EventId BotEventId { get; private set; }
        public static Dictionary<DiscordGuild, List<DiscordChannel>> Current { get; private set; }

        public static void initialize(DiscordClient client, EventId eventId)
        {
            Client = client;
            client.VoiceStateUpdated += Client_VoiceStateUpdated;
            BotEventId = eventId;
            Current = new Dictionary<DiscordGuild, List<DiscordChannel>>();
        }

        private static Task Client_VoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
        {
            //Client.Logger.LogInformation(BotEventId, String.Format($"Voice state updated in \"{e.Guild.Name}\" at {DateTime.Now:hh:mm:ss}"));
            
            if (e.Channel == null)
            {
                //executes when a player leaves
                if (!Current.ContainsKey(e.Guild))
                    return Task.CompletedTask;

                var channels = Current[e.Guild];
                var newChannels = new List<DiscordChannel>();

                channels.ForEach(chn =>
                {
                    if (chn.Users.Count == 0)
                        Client.Logger.LogInformation(BotEventId, $"\"{chn.Name}\" is empty.");
                    else
                        newChannels.Add(chn);
                });

                if (newChannels.Count == 0)
                {
                    Current.Remove(e.Guild);
                    Client.Logger.LogInformation(BotEventId, $"\"{e.Guild.Name}\" has no voice users!");
                }
                Current[e.Guild] = newChannels;

                return Task.CompletedTask;
            }

            if (!Current.ContainsKey(e.Guild))
            {
                Current.Add(e.Guild, new List<DiscordChannel>());

                Client.Logger.LogInformation(BotEventId, String.Format($"\"{e.Guild.Name}\" has voice users!"));
            }

            if (!Current[e.Guild].Contains(e.Channel))
            {
                Current[e.Guild].Add(e.Channel);

                Client.Logger.LogInformation(BotEventId, String.Format($"\"{e.Channel.Name}\" is ready to get rickrolled"));
            }


            return Task.CompletedTask;
        }
    }
}
