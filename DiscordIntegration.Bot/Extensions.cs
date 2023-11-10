namespace DiscordIntegration.Bot;

using System.Text.RegularExpressions;
using DSharpPlus.Entities;

public static class Extensions
{
    public static string SplitCamelCase(this string str)
    {
        return Regex.Replace( 
            Regex.Replace( 
                str, 
                @"(\P{Ll})(\P{Ll}\p{Ll})", 
                "$1 $2" 
            ), 
            @"(\p{Ll})(\P{Ll})", 
            "$1 $2" 
        );
    }

    public static async Task<string> GetUsernameAsync(this DiscordGuild guild, ulong userId)
    {
        var member = await guild.GetMemberAsync(userId);
        return !string.IsNullOrEmpty(member?.Username) ? member.Username : $"Name unavailable. ({userId})";
    }
}