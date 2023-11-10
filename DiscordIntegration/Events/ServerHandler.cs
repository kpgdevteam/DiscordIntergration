// -----------------------------------------------------------------------
// <copyright file="ServerHandler.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;

namespace DiscordIntegration.Events
{
    using Dependency;
    using Respawning;
    using static DiscordIntegration;

    /// <summary>
    /// Handles server-related events.
    /// </summary>
    internal sealed class ServerHandler
    {
        public ServerHandler(DiscordIntegration plugin) => Plugin = plugin;

        public static DiscordIntegration Plugin;
        
#pragma warning disable SA1600 // Elements should be documented

        [PluginEvent(ServerEventType.PlayerCheaterReport)]
        public async void OnReportingCheater(Player player, Player target, string reason)
        {
            if (Plugin.Config.EventsToLog.ReportingCheater)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.Reports, string.Format(Language.CheaterReportFilled, player.Nickname, player.UserId, player.Role, target.Nickname, target.UserId, target.Role, reason))).ConfigureAwait(false);
        }
        
        [PluginEvent(ServerEventType.PlayerReport)]
        public async void OnLocalReporting(Player player, Player target, string reason)
        {
            if (Plugin.Config.EventsToLog.ReportingCheater)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.Reports, string.Format(Language.CheaterReportFilled, player.Nickname, player.UserId, player.Role, target.Nickname, target.UserId, target.Role, reason))).ConfigureAwait(false);
        }

        [PluginEvent(ServerEventType.WaitingForPlayers)]
        public async void OnWaitingForPlayers()
        {
            if (Plugin.Config.EventsToLog.WaitingForPlayers)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, Language.WaitingForPlayers)).ConfigureAwait(false);
            if (Plugin.Config.StaffOnlyEventsToLog.WaitingForPlayers)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, Language.WaitingForPlayers)).ConfigureAwait(false);
        }

        [PluginEvent(ServerEventType.RoundRestart)]
        public async void OnRoundStarted()
        {
            if (Plugin.Config.EventsToLog.RoundStarted)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.RoundStarting, Player.GetPlayers().Count))).ConfigureAwait(false);
        }

        [PluginEvent(ServerEventType.RoundEnd)]
        public async void OnRoundEnded(RoundSummary.LeadingTeam leadingTeam)
        {
            if (Plugin.Config.EventsToLog.RoundEnded)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.RoundEnded, leadingTeam, Player.GetPlayers().Count, Plugin.Slots))).ConfigureAwait(false);
        }

        [PluginEvent(ServerEventType.TeamRespawn)]
        public async void OnRespawningTeam(SpawnableTeamType team)
        {
            if (Plugin.Config.EventsToLog.RespawningTeam)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(team == SpawnableTeamType.ChaosInsurgency ? Language.ChaosInsurgencyHaveSpawned : Language.NineTailedFoxHaveSpawned, "Unknown"))).ConfigureAwait(false);
        }
    }
}