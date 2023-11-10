// -----------------------------------------------------------------------
// <copyright file="PlayerHandler.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

using System.Numerics;
using CommandSystem;
using DiscordIntegration.Dependency.Database;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.ThrowableProjectiles;
using InventorySystem.Items.Usables;
using MapGeneration.Distributors;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079;
using PlayerStatsSystem;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Core.Interfaces;
using PluginAPI.Core.Items;
using PluginAPI.Enums;
using RemoteAdmin;

namespace DiscordIntegration.Events
{
    using System;
    using Dependency;
    using static DiscordIntegration;

    /// <summary>
    /// Handles player-related events.
    /// </summary>
    internal sealed class PlayerHandler
    {
        public PlayerHandler(DiscordIntegration plugin) => this.plugin = plugin;

        private readonly DiscordIntegration plugin;
        
#pragma warning disable SA1600 // Elements should be documented
        [PluginEvent(ServerEventType.PlayerActivateGenerator)]
        public async void OnInsertingGeneratorTablet(Player player, Scp079Generator generator)
        {
            if (plugin.Config.EventsToLog.PlayerInsertingGeneratorTablet && (!player.DoNotTrack || !plugin.Config.ShouldRespectDoNotTrack))
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.GeneratorInserted, player.Nickname, plugin.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.Role))).ConfigureAwait(false);
            if (plugin.Config.StaffOnlyEventsToLog.PlayerInsertingGeneratorTablet)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.GeneratorInserted, player.Nickname, player.UserId, player.Role))).ConfigureAwait(false);
        }

        [PluginEvent(ServerEventType.PlayerOpenGenerator)]
        public async void OnOpeningGenerator(Player player, Scp079Generator generator)
        {
            if (plugin.Config.EventsToLog.PlayerOpeningGenerator && (!player.DoNotTrack || !plugin.Config.ShouldRespectDoNotTrack))
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.GeneratorOpened, player.Nickname, plugin.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.Role))).ConfigureAwait(false);
            if (plugin.Config.StaffOnlyEventsToLog.PlayerOpeningGenerator)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.GeneratorOpened, player.Nickname, player.UserId, player.Role))).ConfigureAwait(false);
        }

        [PluginEvent(ServerEventType.PlayerUnlockGenerator)]
        public async void OnUnlockingGenerator(Player player, Scp079Generator generator)
        {
            if (plugin.Config.EventsToLog.PlayerUnlockingGenerator && (!player.DoNotTrack || !plugin.Config.ShouldRespectDoNotTrack))
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.GeneratorUnlocked, player.Nickname, plugin.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.Role))).ConfigureAwait(false);
            if (plugin.Config.StaffOnlyEventsToLog.PlayerUnlockingGenerator)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.GeneratorUnlocked, player.Nickname, player.UserId, player.Role))).ConfigureAwait(false);
        }

        [PluginEvent(ServerEventType.PlayerDeath)]
        public async void OnContaining(Player player, Player attacker, DamageHandlerBase damageHandler)
        {
            if (player.Role != RoleTypeId.Scp106 || damageHandler is not RecontainmentDamageHandler) return;
            
            if (plugin.Config.EventsToLog.ContainingScp106 && (!player.DoNotTrack || !plugin.Config.ShouldRespectDoNotTrack))
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.Scp106WasContained, player.Nickname, plugin.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.Role))).ConfigureAwait(false);
            if (plugin.Config.StaffOnlyEventsToLog.ContainingScp106)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.Scp106WasContained, player.Nickname, player.UserId, player.Role))).ConfigureAwait(false);
        }

        [PluginEvent(ServerEventType.Scp106Stalking)]
        public async void OnScp106Stalking(Player player, bool activated)
        {
            if (plugin.Config.EventsToLog.Scp106Stalking && (!player.DoNotTrack || !plugin.Config.ShouldRespectDoNotTrack))
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.Scp106Stalking, player.Nickname, plugin.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.Role))).ConfigureAwait(false);
            if (plugin.Config.StaffOnlyEventsToLog.Scp106Stalking)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.Scp106Stalking, player.Nickname, player.UserId, player.Role))).ConfigureAwait(false);
        }

        // [PluginEvent(ServerEventType.PlayerChangeItem)]
        // public async void OnChangingItem(Player player, ushort oldItem, ushort newItem)
        // {
        //     if (!InventorySystem.InventoryExtensions.ServerTryGetItemWithSerial(newItem, out var item))
        //     {
        //         Log.Error($"[OnChangingItem] serial {newItem} did not return a real item. Please contact the plugin developer.");
        //         return;
        //     }
        //     
        //     if (plugin.Config.EventsToLog.ChangingPlayerItem && (!player.DoNotTrack || !plugin.Config.ShouldRespectDoNotTrack))
        //         await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.ItemChanged, player.Nickname, plugin.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.CurrentItem.ItemTypeId, item.ItemTypeId))).ConfigureAwait(false);
        //     if (plugin.Config.StaffOnlyEventsToLog.ChangingPlayerItem)
        //         await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.ItemChanged, player.Nickname, player.UserId, player.CurrentItem.ItemTypeId, item.ItemTypeId))).ConfigureAwait(false);
        // }

        [PluginEvent(ServerEventType.Scp079GainExperience)]
        public async void OnGainingExperience(Player player, int amount, Scp079HudTranslation reason)
        {
            if (plugin.Config.EventsToLog.GainingScp079Experience && (!player.DoNotTrack || !plugin.Config.ShouldRespectDoNotTrack))
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.GainedExperience, player.Nickname, plugin.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.Role, amount, reason))).ConfigureAwait(false);
            if (plugin.Config.StaffOnlyEventsToLog.GainingScp079Experience)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.GainedExperience, player.Nickname, player.UserId, player.Role, amount, reason))).ConfigureAwait(false);
        }

        [PluginEvent(ServerEventType.Scp079LevelUpTier)]
        public async void OnGainingLevel(Player player, int tier)
        {
            if (plugin.Config.EventsToLog.GainingScp079Level && (!player.DoNotTrack || !plugin.Config.ShouldRespectDoNotTrack))
#pragma warning disable CS0618 // Type or member is obsolete
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.GainedLevel, player.Nickname, plugin.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.Role, tier - 1, tier))).ConfigureAwait(false);
            if (plugin.Config.StaffOnlyEventsToLog.GainingScp079Level)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.GainedLevel, player.Nickname, player.UserId, player.Role, tier - 1, tier))).ConfigureAwait(false);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [PluginEvent(ServerEventType.PlayerLeft)]
        public async void OnPlayerLeft(Player player)
        {
            if (plugin.Config.EventsToLog.PlayerLeft && (!player.DoNotTrack || !plugin.Config.ShouldRespectDoNotTrack))
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.LeftServer, player.Nickname, plugin.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.Role))).ConfigureAwait(false);
            if (plugin.Config.StaffOnlyEventsToLog.PlayerLeft)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.LeftServer, player.Nickname, player.UserId, player.Role))).ConfigureAwait(false);
        }

        [PluginEvent(ServerEventType.PlayerReloadWeapon)]
        public async void OnReloadingWeapon(Player player, Firearm firearm)
        {
            if (plugin.Config.EventsToLog.ReloadingPlayerWeapon && (!player.DoNotTrack || !plugin.Config.ShouldRespectDoNotTrack))
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.Reloaded, player.Nickname, plugin.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.CurrentItem.ItemTypeId, player.Role))).ConfigureAwait(false);
            if (plugin.Config.StaffOnlyEventsToLog.ReloadingPlayerWeapon)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.Reloaded, player.Nickname, player.UserId, player.CurrentItem.ItemTypeId, player.Role))).ConfigureAwait(false);
        }
        
        // Not currently supported.
        // public async void OnActivatingWarheadPanel(ActivatingWarheadPanelEventArgs ev)
        // {
        //     if (Instance.Config.EventsToLog.PlayerActivatingWarheadPanel && (!player.DoNotTrack || !Instance.Config.ShouldRespectDoNotTrack))
        //         await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.AccessedWarhead, player.Nickname, Instance.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.Role))).ConfigureAwait(false);
        //     if (Instance.Config.StaffOnlyEventsToLog.PlayerActivatingWarheadPanel)
        //         await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.AccessedWarhead, player.Nickname, player.UserId, player.Role))).ConfigureAwait(false);
        // }

        [PluginEvent(ServerEventType.PlayerInteractElevator)]
        public async void OnInteractingElevator(Player player)
        {
            if (plugin.Config.EventsToLog.PlayerInteractingElevator && (!player.DoNotTrack || !plugin.Config.ShouldRespectDoNotTrack))
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.CalledElevator, player.Nickname, plugin.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.Role))).ConfigureAwait(false);
            if (plugin.Config.StaffOnlyEventsToLog.PlayerInteractingElevator)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.CalledElevator, player.Nickname, player.UserId, player.Role))).ConfigureAwait(false);
        }

        [PluginEvent(ServerEventType.PlayerInteractLocker)]
        public async void OnInteractingLocker(Player player, Locker locker, byte colliderId, bool canOpen)
        {
            if (plugin.Config.EventsToLog.PlayerInteractingLocker && (!player.DoNotTrack || !plugin.Config.ShouldRespectDoNotTrack))
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.UsedLocker, player.Nickname, plugin.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.Role))).ConfigureAwait(false);
            if (plugin.Config.StaffOnlyEventsToLog.PlayerInteractingLocker)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.UsedLocker, player.Nickname, player.UserId, player.Role))).ConfigureAwait(false);
        }

        // Not currently supported
        // public async void OnTriggeringTesla(TriggeringTeslaEventArgs ev)
        // {
        //     if (Instance.Config.EventsToLog.PlayerTriggeringTesla && (!player.DoNotTrack || !Instance.Config.ShouldRespectDoNotTrack))
        //         await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.HasTriggeredATeslaGate, player.Nickname, Instance.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.Role))).ConfigureAwait(false);
        //     if (Instance.Config.StaffOnlyEventsToLog.PlayerTriggeringTesla)
        //         await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.HasTriggeredATeslaGate, player.Nickname, player.UserId, player.Role))).ConfigureAwait(false);
        // }

        [PluginEvent(ServerEventType.PlayerCloseGenerator)]
        public async void OnClosingGenerator(Player player, Scp079Generator generator)
        {
            if (plugin.Config.EventsToLog.PlayerClosingGenerator && (!player.DoNotTrack || !plugin.Config.ShouldRespectDoNotTrack))
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.GeneratorClosed, player.Nickname, plugin.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.Role))).ConfigureAwait(false);
            if (plugin.Config.StaffOnlyEventsToLog.PlayerClosingGenerator)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.GeneratorClosed, player.Nickname, player.UserId, player.Role))).ConfigureAwait(false);
        }
        
        [PluginEvent(ServerEventType.PlayerInteractDoor)]
        public async void OnInteractingDoor(Player player, DoorVariant door, bool canOpen)
        {
            if (plugin.Config.EventsToLog.PlayerInteractingDoor && (!player.DoNotTrack || !plugin.Config.ShouldRespectDoNotTrack))
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(door.IsConsideredOpen() ? Language.HasClosedADoor : Language.HasOpenedADoor, player.Nickname, plugin.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.Role, door.name))).ConfigureAwait(false);
            if (plugin.Config.StaffOnlyEventsToLog.PlayerInteractingDoor)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(door.IsConsideredOpen() ? Language.HasClosedADoor : Language.HasOpenedADoor, player.Nickname, player.UserId, player.Role, door.name))).ConfigureAwait(false);
        }

        [PluginEvent(ServerEventType.Scp914Activate)]
        public async void OnActivatingScp914(Player player)
        {
            if (plugin.Config.EventsToLog.ActivatingScp914 && (!player.DoNotTrack || !plugin.Config.ShouldRespectDoNotTrack))
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.Scp914HasBeenActivated, player.Nickname, plugin.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.Role, Handlers.GetNobSetting()))).ConfigureAwait(false);
            if (plugin.Config.StaffOnlyEventsToLog.ActivatingScp914)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.Scp914HasBeenActivated, player.Nickname, player.UserId, player.Role, Handlers.GetNobSetting()))).ConfigureAwait(false);
        }

        [PluginEvent(ServerEventType.Scp914KnobChange)]
        public async void OnChangingScp914KnobSetting(Player player)
        {
            if (plugin.Config.EventsToLog.ChangingScp914KnobSetting && (!player.DoNotTrack || !plugin.Config.ShouldRespectDoNotTrack))
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.Scp914KnobSettingChanged, player.Nickname, plugin.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.Role, Handlers.GetNobSetting()))).ConfigureAwait(false);
            if (plugin.Config.StaffOnlyEventsToLog.ChangingScp914KnobSetting)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.Scp914KnobSettingChanged, player.Nickname, player.UserId, player.Role, Handlers.GetNobSetting()))).ConfigureAwait(false);
        }

        [PluginEvent(ServerEventType.PlayerEnterPocketDimension)]
        public async void OnEnteringPocketDimension(Player player)
        {
            if (plugin.Config.EventsToLog.PlayerEnteringPocketDimension && (!player.DoNotTrack || !plugin.Config.ShouldRespectDoNotTrack))
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.HasEnteredPocketDimension, player.Nickname, plugin.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.Role))).ConfigureAwait(false);
            if (plugin.Config.StaffOnlyEventsToLog.PlayerEnteringPocketDimension)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.HasEnteredPocketDimension, player.Nickname, player.UserId, player.Role))).ConfigureAwait(false);
        }

        [PluginEvent(ServerEventType.PlayerExitPocketDimension)]
        public async void OnEscapingPocketDimension(Player player, bool isSuccessful)
        {
            if (!isSuccessful) return;
            
            if (plugin.Config.EventsToLog.PlayerEscapingPocketDimension && (!player.DoNotTrack || !plugin.Config.ShouldRespectDoNotTrack))
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.HasEscapedPocketDimension, player.Nickname, plugin.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.Role))).ConfigureAwait(false);
            if (plugin.Config.StaffOnlyEventsToLog.PlayerEscapingPocketDimension)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.HasEscapedPocketDimension, player.Nickname, player.UserId, player.Role))).ConfigureAwait(false);
        }

        [PluginEvent(ServerEventType.Scp106TeleportPlayer)]
        public async void OnTeleporting(Player player, Player target)
        {
            if (plugin.Config.EventsToLog.Scp106Teleporting && (!player.DoNotTrack || !plugin.Config.ShouldRespectDoNotTrack))
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.Scp106Teleported, player.Nickname, plugin.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.Role))).ConfigureAwait(false);
            if (plugin.Config.StaffOnlyEventsToLog.Scp106Teleporting)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.Scp106Teleported, player.Nickname, player.UserId, player.Role))).ConfigureAwait(false);
        }

        [PluginEvent(ServerEventType.Scp079UseTesla)]
        public async void OnInteractingTesla(Player player, TeslaGate tesla)
        {
            if (plugin.Config.EventsToLog.Scp079InteractingTesla && (!player.DoNotTrack || !plugin.Config.ShouldRespectDoNotTrack))
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.HasTriggeredATeslaGate, player.Nickname, plugin.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.Role))).ConfigureAwait(false);
            if (plugin.Config.StaffOnlyEventsToLog.Scp079InteractingTesla)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.HasTriggeredATeslaGate, player.Nickname, player.UserId, player.Role))).ConfigureAwait(false);
        }

        [PluginEvent(ServerEventType.PlayerDamage)]
        public async void OnHurting(Player player, Player target, DamageHandlerBase damageHandler)
        {
            string damageType = damageHandler.ToId().DamageType();
            
            if (plugin.Config.EventsToLog.HurtingPlayer && target.UserId != null && (player.UserId == null || !plugin.Config.ShouldLogFriendlyFireDamageOnly || player.Side() == target.Side()) && (!plugin.Config.ShouldRespectDoNotTrack || player.UserId == null || (!player.DoNotTrack && !target.DoNotTrack)) && !plugin.Config.BlacklistedDamageTypes.Contains(damageType) && (!plugin.Config.OnlyLogPlayerDamage || player.UserId != null))
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.HasDamagedForWith, player.UserId != null ? player.Nickname : "Server", plugin.Config.ShouldLogUserIds ? player.UserId ?? string.Empty : Language.Redacted, player?.Role ?? RoleTypeId.None, target.Nickname, plugin.Config.ShouldLogUserIds ? target.UserId : Language.Redacted, target.Role, damageHandler.Amount(), damageType))).ConfigureAwait(false);

            if (plugin.Config.StaffOnlyEventsToLog.HurtingPlayer && target.UserId != null && (player.UserId == null || !plugin.Config.ShouldLogFriendlyFireDamageOnly || player.Side() == target.Side()) && !plugin.Config.BlacklistedDamageTypes.Contains(damageType) && (!plugin.Config.OnlyLogPlayerDamage || player.UserId != null))
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.HasDamagedForWith, player.UserId != null ? player.Nickname : "Server", player.UserId != null ? player.UserId : string.Empty, player?.Role ?? RoleTypeId.None, target.Nickname, target.UserId, target.Role, damageHandler.Amount(), damageType))).ConfigureAwait(false);
        }

        [PluginEvent(ServerEventType.PlayerDeath)]
        public async void OnDying(Player target, Player attacker, DamageHandlerBase damageHandler)
        {
            string damageType = damageHandler.ToId().DamageType();

            if (plugin.Config.EventsToLog.PlayerDying && target.UserId != null && (attacker.UserId == null || !plugin.Config.ShouldLogFriendlyFireKillsOnly || attacker.Side() == target.Side()) && (!plugin.Config.ShouldRespectDoNotTrack || (attacker.UserId == null || (!attacker.DoNotTrack && !target.DoNotTrack))))
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.HasKilledWith, attacker != null ? attacker.Nickname : "Server", plugin.Config.ShouldLogUserIds ? attacker != null ? attacker.UserId : string.Empty : Language.Redacted, attacker?.Role ?? RoleTypeId.None, target.Nickname, plugin.Config.ShouldLogUserIds ? target.UserId : Language.Redacted, target.Role, damageType))).ConfigureAwait(false);

            if (plugin.Config.StaffOnlyEventsToLog.PlayerDying && target.UserId != null && (attacker.UserId == null || !plugin.Config.ShouldLogFriendlyFireKillsOnly || attacker.Side() == target.Side()))
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.HasKilledWith, attacker.UserId != null ? attacker.Nickname : "Server", attacker.UserId ?? string.Empty, attacker?.Role ?? RoleTypeId.None, target.Nickname, target.UserId, target.Role, damageHandler))).ConfigureAwait(false);
        }

        [PluginEvent(ServerEventType.PlayerThrowProjectile)]
        public async void OnThrowingGrenade(IPlayer player, ThrowableItem item, float forceAmount, float upwardsFactor, Vector3 torque, Vector3 velocity)
        {
            if (player is not Player ply) 
                return;
            if (ply.UserId != null && plugin.Config.EventsToLog.PlayerThrowingGrenade && (!ply.DoNotTrack || !plugin.Config.ShouldRespectDoNotTrack))
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.ThrewAGrenade, ply.Nickname, plugin.Config.ShouldLogUserIds ? ply.UserId : Language.Redacted, ply.Role, item.ItemTypeId))).ConfigureAwait(false);
            if (ply.UserId != null && plugin.Config.StaffOnlyEventsToLog.PlayerThrowingGrenade)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.ThrewAGrenade, ply.Nickname, ply.UserId, ply.Role, item.ItemTypeId))).ConfigureAwait(false);
        }

        [PluginEvent(ServerEventType.PlayerUsedItem)]
        public async void OnUsedMedicalItem(Player player, ItemBase item)
        {
            if (item is not Consumable consumable) return;
            
            if (player.UserId != null && plugin.Config.EventsToLog.PlayerUsedMedicalItem && (!player.DoNotTrack || !plugin.Config.ShouldRespectDoNotTrack))
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.UsedMedicalItem, player.Nickname, plugin.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.Role, consumable.name))).ConfigureAwait(false);
            if (player.UserId != null && plugin.Config.StaffOnlyEventsToLog.PlayerUsedMedicalItem)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.UsedMedicalItem, player.Nickname, player.UserId, player.Role, consumable.name))).ConfigureAwait(false);
        }

        [PluginEvent(ServerEventType.PlayerChangeRole)]
        public async void OnChangingRole(Player player, PlayerRoleBase oldRole, RoleTypeId newRole, RoleChangeReason changeReason)
        {
            if (player.UserId != null && plugin.Config.EventsToLog.ChangingPlayerRole && (!player.DoNotTrack || !plugin.Config.ShouldRespectDoNotTrack))
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.ChangedRole, player.Nickname, plugin.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.Role, newRole))).ConfigureAwait(false);
            if (player.UserId != null && plugin.Config.StaffOnlyEventsToLog.ChangingPlayerRole)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.ChangedRole, player.Nickname, player.UserId, player.Role, newRole))).ConfigureAwait(false);
        }

        [PluginEvent(ServerEventType.PlayerJoined)]
        public async void OnVerified(Player player)
        {
            if (plugin.Config.EventsToLog.PlayerJoined && (!player.DoNotTrack || !plugin.Config.ShouldRespectDoNotTrack))
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.HasJoinedTheGame, player.Nickname, plugin.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, plugin.Config.ShouldLogIPAddresses ? player.IpAddress : Language.Redacted))).ConfigureAwait(false);
            if (plugin.Config.StaffOnlyEventsToLog.PlayerJoined)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.HasJoinedTheGame, player.Nickname, player.UserId, player.IpAddress))).ConfigureAwait(false);
        }

        [PluginEvent(ServerEventType.PlayerRemoveHandcuffs)]
        public async void OnRemovingHandcuffs(Player player, Player target)
        {
            if (plugin.Config.EventsToLog.PlayerRemovingHandcuffs && ((!player.DoNotTrack && !target.DoNotTrack) || !plugin.Config.ShouldRespectDoNotTrack))
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.HasBeenFreedBy, target.Nickname, plugin.Config.ShouldLogUserIds ? target.UserId : Language.Redacted, target.Role, player.Nickname, plugin.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.Role))).ConfigureAwait(false);
            if (plugin.Config.StaffOnlyEventsToLog.PlayerRemovingHandcuffs)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.HasBeenFreedBy, target.Nickname, target.UserId, target.Role, player.Nickname, player.UserId, player.Role))).ConfigureAwait(false);
        }

        [PluginEvent(ServerEventType.PlayerHandcuff)]
        public async void OnHandcuffing(Player player, Player target)
        {
            if (plugin.Config.EventsToLog.HandcuffingPlayer && ((!player.DoNotTrack && !target.DoNotTrack) || !plugin.Config.ShouldRespectDoNotTrack))
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.HasBeenHandcuffedBy, target.Nickname, plugin.Config.ShouldLogUserIds ? target.UserId : Language.Redacted, target.Role, player.Nickname, plugin.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.Role))).ConfigureAwait(false);
            if (plugin.Config.StaffOnlyEventsToLog.HandcuffingPlayer)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.HasBeenHandcuffedBy, target.Nickname, target.UserId, target.Role, player.Nickname, player.UserId, player.Role))).ConfigureAwait(false);
        }

        [PluginEvent(ServerEventType.PlayerKicked)]
        public async void OnKicked(Player player, Player issuer, string reason)
        {
            if (plugin.Config.EventsToLog.PlayerBanned)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, "kicks", string.Format(Language.WasKicked, player?.Nickname ?? Language.NotAuthenticated, player?.UserId ?? Language.NotAuthenticated, reason))).ConfigureAwait(false);
        }

        [PluginEvent(ServerEventType.PlayerBanned)]
        public async void OnBanned(Player player, ICommandSender issuer, string reason, long duration)
        {
            Player playerIssuer = Player.Get(((PlayerCommandSender)issuer).ReferenceHub);
            if (plugin.Config.EventsToLog.PlayerBanned)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.Bans, string.Format(Language.WasBannedBy, player.Nickname, player.UserId, playerIssuer.Nickname, reason, DateTime.Now.AddSeconds(duration).ToString(plugin.Config.DateFormat)))).ConfigureAwait(false);
        }

        //No longer supported
        // public async void OnIntercomSpeaking(IntercomSpeakingEventArgs ev)
        // {
        //     if (player != null && Instance.Config.EventsToLog.PlayerIntercomSpeaking && (!player.DoNotTrack || !Instance.Config.ShouldRespectDoNotTrack))
        //         await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.HasStartedUsingTheIntercom, player.Nickname, Instance.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.Role))).ConfigureAwait(false);
        //     if (player != null && Instance.Config.StaffOnlyEventsToLog.PlayerIntercomSpeaking)
        //         await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.HasStartedUsingTheIntercom, player.Nickname, player.UserId, player.Role))).ConfigureAwait(false);
        // }

        [PluginEvent(ServerEventType.PlayerSearchedPickup)]
        public async void OnPickingUpItem(Player player, ItemPickupBase item)
        {
            if (plugin.Config.EventsToLog.PlayerPickingupItem && (!player.DoNotTrack || !plugin.Config.ShouldRespectDoNotTrack))
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.HasPickedUp, player.Nickname, plugin.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.Role, item.name))).ConfigureAwait(false);
            if (plugin.Config.StaffOnlyEventsToLog.PlayerPickingupItem)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.HasPickedUp, player.Nickname, player.UserId, player.Role, item.name))).ConfigureAwait(false);
        }

        [PluginEvent(ServerEventType.PlayerDropItem)]
        public async void OnItemDropped(Player player, ItemBase item)
        {
            if (plugin.Config.EventsToLog.PlayerItemDropped && (!player.DoNotTrack || !plugin.Config.ShouldRespectDoNotTrack))
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.HasDropped, player.Nickname, plugin.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.Role, item.ItemTypeId))).ConfigureAwait(false);
            if (plugin.Config.StaffOnlyEventsToLog.PlayerItemDropped)
                await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.HasDropped, player.Nickname, player.UserId, player.Role, item.ItemTypeId))).ConfigureAwait(false);
        }

        // Not currently supported
        // public async void OnChangingGroup(ChangingGroupEventArgs ev)
        // {
        //     if (player != null && Instance.Config.EventsToLog.ChangingPlayerGroup && (!player.DoNotTrack || !Instance.Config.ShouldRespectDoNotTrack))
        //         await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, string.Format(Language.GroupSet, player.Nickname, Instance.Config.ShouldLogUserIds ? player.UserId : Language.Redacted, player.Role, ev.NewGroup?.BadgeText ?? Language.None, ev.NewGroup?.BadgeColor ?? Language.None))).ConfigureAwait(false);
        //     if (player != null && Instance.Config.StaffOnlyEventsToLog.ChangingPlayerGroup)
        //         await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.GroupSet, player.Nickname, player.UserId, player.Role, ev.NewGroup?.BadgeText ?? Language.None, ev.NewGroup?.BadgeColor ?? Language.None))).ConfigureAwait(false);
        // }

        [PluginEvent(ServerEventType.PlayerGameConsoleCommand)]
        public void OnPlayerGameConsoleCommand(Player player, string command, string[] arguments)
        {
            if (!plugin.Config.EventsToLog.SendingConsoleCommands && !plugin.Config.StaffOnlyEventsToLog.SendingConsoleCommands)
                return;

            if (command.StartsWith("$"))
                return;

            if (player.UserId == null || (!string.IsNullOrEmpty(player.UserId) && plugin.Config.TrustedAdmins.Contains(player.UserId)))
                return;
            if (plugin.Config.EventsToLog.SendingConsoleCommands)
                _ = Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.Command, string.Format(Language.UsedCommand, player.Nickname, player.UserId ?? Language.DedicatedServer, player.Role, command, string.Join(" ", arguments))));
            if (plugin.Config.StaffOnlyEventsToLog.SendingConsoleCommands)
                _ = Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.UsedCommand, player.Nickname, player.UserId ?? Language.DedicatedServer, player.Role, command, string.Join(" ", arguments))));
        }
        
        [PluginEvent(ServerEventType.RemoteAdminCommand)]
        public void OnPlayerRemoteAdminCommand(ICommandSender sender, string command, string[] arguments)
        {
            Player player = Player.Get(((PlayerCommandSender)sender).ReferenceHub);
            
            if (!plugin.Config.EventsToLog.SendingRemoteAdminCommands && !plugin.Config.StaffOnlyEventsToLog.SendingRemoteAdminCommands)
                return;

            if (command.StartsWith("$"))
                return;

            if (player.UserId == null || (!string.IsNullOrEmpty(player.UserId) && plugin.Config.TrustedAdmins.Contains(player.UserId)))
                return;
            if (plugin.Config.EventsToLog.SendingRemoteAdminCommands)
                _ = Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.Command, string.Format(Language.UsedCommand, player.Nickname, player.UserId ?? Language.DedicatedServer, player.Role, command, string.Join(" ", arguments))));
            if (plugin.Config.StaffOnlyEventsToLog.SendingRemoteAdminCommands)
                _ = Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.StaffCopy, string.Format(Language.UsedCommand, player.Nickname, player.UserId ?? Language.DedicatedServer, player.Role, command, string.Join(" ", arguments))));

        }
    }
}