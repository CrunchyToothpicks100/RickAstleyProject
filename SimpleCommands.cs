using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using RickAstleyProject.Attributes;
using RickAstleyProject.VoiceRelated;

namespace RickAstleyProject
{
    public class SimpleCommands : BaseCommandModule
    {
        [Command("Ping")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Pong").ConfigureAwait(false);
        }

        [Command("Add")]
        [Description("Adds two numbers together")]
        public async Task add(CommandContext ctx,
            [Description("First Number")] int num1,
            [Description("Second Number")] int num2)
        {
            int result;
            if (num1 == 9 && num2 == 10)
                result = 21;
            else
                result = num1 + num2;

            await ctx.Channel
                .SendMessageAsync((result).ToString())
                .ConfigureAwait(false);
        }

        [Command("Subtract")]
        [Description("Subtract a number from another")]
        public async Task subtract(CommandContext ctx,
            [Description("First Number")] int num1,
            [Description("Second Number")] int num2)
        {
            await ctx.Channel
                .SendMessageAsync((num1 - num2).ToString())
                .ConfigureAwait(false);
        }

        [Command("Multiply")]
        [Description("Multiplies two numbers together")]
        public async Task multiply(CommandContext ctx,
            [Description("First Number")] int num1,
            [Description("Second Number")] int num2)
        {
            await ctx.Channel
                .SendMessageAsync((num1 * num2).ToString())
                .ConfigureAwait(false);
        }

        [Command("songlink")]
        [Description("Sends a link to \"Never Gonna Give You Up\"")]
        public async Task songLink(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("https://www.youtube.com/watch?v=dQw4w9WgXcQ").ConfigureAwait(false);
        }

        [Command("lyrics")]
        [Description("Sends a message containing the full lyrics to \"Never Gonna Give You Up\"")]
        public async Task Lyrics(CommandContext ctx)
        {
            var fs = File.OpenRead("../../../lyrics.txt");
            var sr = new StreamReader(fs);
            string lyrics = sr.ReadToEnd();
            fs.Close();

            await ctx.Channel.SendMessageAsync(lyrics).ConfigureAwait(false);
        }

        [Command("Repeat")]
        [Description("Repeats the next message sent")]
        public async Task Repeat(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();

            var message = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(message.Result.Content);
        }

        [Command("Poll")]
        [Description("Run a poll")]
        public async Task Poll(CommandContext ctx, TimeSpan duration, params string[] opinion)
        {
            var interactivity = ctx.Client.GetInteractivity();

            var description = string.Join(" ", opinion);

            var pollEmbed = new DiscordEmbedBuilder
            {
                Title = "An Unofficial Rick Astley Bot(TM) Poll",
                Description = description
            };

            DiscordEmoji[] emojiOptions = new DiscordEmoji[]
            {
                DiscordEmoji.FromName(ctx.Client, ":thumbsup:"),
                DiscordEmoji.FromName(ctx.Client, ":thumbsdown:")
            };

            var pollMessage = await ctx.Channel.SendMessageAsync(embed: pollEmbed);

            var poll_result = await interactivity.DoPollAsync(pollMessage, emojiOptions, PollBehaviour.DeleteEmojis, duration).ConfigureAwait(false);
            var results = poll_result.Select(x => $"{x.Emoji}: {x.Total}");

            var resultsEmbed = new DiscordEmbedBuilder
            {
                Title = $":drum: Poll results for: {description} :drum:",
                Description = string.Join("\n", results)
            };

            await ctx.Channel.SendMessageAsync(embed: resultsEmbed).ConfigureAwait(false);
        }

        [Command("Rickroll")]
        [Description("Executes a rickroll")]
        public async Task Rickroll(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Attempting to rickroll your server...").ConfigureAwait(false);
        }

        [Command("Stop")]
        [Description("Terminates a rickroll")]
        public async Task Stop(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("lol it doesn't work").ConfigureAwait(false);
        }
    }
}
