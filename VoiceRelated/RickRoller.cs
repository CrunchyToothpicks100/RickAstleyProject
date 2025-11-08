using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;
using DSharpPlus.VoiceNext.EventArgs;
using Microsoft.Extensions.Logging;
using RickAstleyProject.VoiceRelated;
using System.Diagnostics;

namespace RickAstleyProject
{
    #pragma warning disable CS8602
    public class RickRoller
    {
        public DiscordClient Client { get; }
        public EventId BotEventId { get; }

        private List<Task> musicTasks = new List<Task>();

        public RickRoller(DiscordClient client, EventId eventid)
        {
            Client = client;
            BotEventId = eventid;

            RickRollClockAsync().ConfigureAwait(false);
        }

        private async Task RickRollClockAsync()
        {
            List<DiscordGuild> activeGuilds = ActiveChannelDictionary.Current.Keys.ToList();

            await Task.Run(() =>
            {
                WaitForTheHour();

                Client.Logger.LogInformation(BotEventId, "Rickrolling Guilds...");

                activeGuilds.ForEach(guild => RickRoll(guild));
                Task.WhenAll(musicTasks).Wait();

                RestartRickRollClock();
            });
        }

        public void RickRoll(DiscordGuild guild)
        {
            DiscordChannel activeChannel = PickActiveChannel(guild);

            Task stream = RickRollChannel(activeChannel);
            musicTasks.Add(stream);
        }

        private void WaitForTheHour()
        {
            Task.Delay(3600000 - ((int)DateTime.Now.TimeOfDay.TotalMilliseconds % 3600000)).Wait();
            Client.Logger.LogInformation(BotEventId, $"{DateTime.Now:hh:mm:ss} - It's rickroll time!");
        }

        private void WaitForTheMinute()
        {
            Task.Delay(60000 - ((int)DateTime.Now.TimeOfDay.TotalMilliseconds % 60000)).Wait();
            Client.Logger.LogInformation(BotEventId, $"{DateTime.Now:hh:mm:ss} - It's rickroll time!");
        }

        private DiscordChannel PickActiveChannel(DiscordGuild guild)
        {
            List<DiscordChannel> activeChannels = ActiveChannelDictionary.Current[guild];

            int largest = 0;

            for (int i = 0; i < activeChannels.Count; i++)
            {
                if (activeChannels[i].Users.Count > activeChannels[largest].Users.Count)
                {
                    largest = i;
                }
            }

            return activeChannels[largest];
        }

        private async Task RickRollChannel(DiscordChannel channel)
        {
            VoiceNextExtension VoiceNE = Client.GetVoiceNext();
            var vnc = VoiceNE.ConnectAsync(channel).Result;

            if (vnc.IsPlaying)
            {
                Client.Logger.LogInformation(BotEventId, $"Rick is too busy in {vnc.TargetChannel.Guild.Name}");
                return;
            }
                
            string file = "RICKROLL.mp3";

            if (!File.Exists(file))
            {
                Client.Logger.LogInformation(BotEventId, $"File `{file}` ACTUALLY doesn't exist.");
                return;
            }

            Exception? exc = null;
            Client.Logger.LogInformation(BotEventId, $"Playing `{file}`");

            try
            {
                await vnc.SendSpeakingAsync(true);

                var psi = new ProcessStartInfo
                {
                    FileName = @"C:\Program Files\ffmpeg\bin\ffmpeg.exe",
                    Arguments = $@"-i ""{file}"" -ac 2 -f s16le -ar 48000 pipe:1 -loglevel quiet",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                var ffmpeg = Process.Start(psi);
                var ffout = ffmpeg.StandardOutput.BaseStream;

                var txStream = vnc.GetTransmitSink();
                await ffout.CopyToAsync(txStream);
                await txStream.FlushAsync();
                
                await vnc.WaitForPlaybackFinishAsync();
            }
            catch (Exception ex) { exc = ex; }
            finally
            {
                await vnc.SendSpeakingAsync(false);
                Client.Logger.LogInformation(BotEventId, $"Finished playing `{file}`");
                vnc.Dispose();
            }

            if (exc != null)
                Client.Logger.LogError(BotEventId, $"An exception occured during playback: `{exc.GetType()}: {exc.Message}`");
        }

        private void RestartRickRollClock()
        {
            ActiveChannelDictionary.Current.Clear();

            RickRollClockAsync().ConfigureAwait(false);
        }

    }
}
