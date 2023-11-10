// -----------------------------------------------------------------------
// <copyright file="MapHandler.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

using InventorySystem.Items;
using MapGeneration.Distributors;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;

namespace DiscordIntegration.Events
{
    using System;
    using Dependency;
    using static DiscordIntegration;

    /// <summary>
    /// Handles map-related events.
    /// </summary>
    internal sealed class MapHandler
    {
#pragma warning disable SA1600 // Elements should be documented
        public MapHandler(DiscordIntegration plugin) => this.plugin = plugin;

        private readonly DiscordIntegration plugin;
        
        public async void OnWarheadDetonated()
        {
            if (plugin.Config.EventsToLog.WarheadDetonated)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, Language.WarheadHasDetonated)).ConfigureAwait(false);
        }

        public async void OnGeneratorActivated(Scp079Generator generator)
        {
            if (plugin.Config.EventsToLog.GeneratorActivated)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.GeneratorFinished, "Generator room is unknown", "Number of engaged generators is unknown."))).ConfigureAwait(false);
        }
        
        [PluginEvent(ServerEventType.LczDecontaminationStart)]
        public async void OnDecontaminating()
        {
            if (plugin.Config.EventsToLog.Decontaminating)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, Language.DecontaminationHasBegun)).ConfigureAwait(false);
        }

        [PluginEvent(ServerEventType.WarheadStart)]
        public async void OnStartingWarhead(bool isAutomatic, Player player)
        {
            if (!plugin.Config.EventsToLog.StartingWarhead || (player.UserId != null && player.DoNotTrack && plugin.Config.ShouldRespectDoNotTrack)) return;
            object[] vars = player.UserId == null ?
                new object[] { Warhead.DetonationTime } :
                new object[] { player.Nickname, plugin.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.Role, Warhead.DetonationTime };

            await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(player.UserId == null ? Language.WarheadStarted : Language.PlayerWarheadStarted, vars))).ConfigureAwait(false);
        }
        
        [PluginEvent(ServerEventType.WarheadStop)]
        public async void OnStoppingWarhead(Player player)
        {
            if (plugin.Config.EventsToLog.StoppingWarhead && (player.UserId == null || !player.DoNotTrack || !plugin.Config.ShouldRespectDoNotTrack))
            {
                object[] vars = player.UserId == null ?
                    Array.Empty<object>() :
                    new object[] { player.Nickname, plugin.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.Role };

                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(player.UserId == null ? Language.CanceledWarhead : Language.PlayerCanceledWarhead, vars))).ConfigureAwait(false);
            }

            if (!plugin.Config.StaffOnlyEventsToLog.StoppingWarhead) return;
            
            {
                object[] vars = player.UserId == null
                    ? Array.Empty<object>()
                    : new object[] { player.Nickname, player.UserId, player.Role };

                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(player.UserId == null ? Language.CanceledWarhead : Language.PlayerCanceledWarhead, vars))).ConfigureAwait(false);
            }
        }

        [PluginEvent(ServerEventType.Scp914UpgradeInventory)]
        public async void OnUpgradingItems(Player player, ItemBase item)
        {
            if (plugin.Config.EventsToLog.UpgradingScp914Items)
            {
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.Scp914ProcessedItem, item.ItemTypeId)));
            }
        }
    }
}