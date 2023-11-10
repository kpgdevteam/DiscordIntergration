namespace DiscordIntegration.Bot.Services;

using System.Reflection;
using DSharpPlus.Entities;

public class EmbedBuilderService
{
    public static string Footer => $"Discord Integration | {Assembly.GetExecutingAssembly().GetName().Version} | - Joker119";

    public static async Task<DiscordEmbed> CreateBasicEmbed(string title, string description, DiscordColor color) => await Task.Run(() => new DiscordEmbedBuilder().WithTitle(title).WithDescription(description).WithColor(color).WithTimestamp(DateTimeOffset.Now).WithFooter(Footer).Build());
}