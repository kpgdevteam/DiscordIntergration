﻿// -----------------------------------------------------------------------
// <copyright file="Bot.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

using DiscordIntegration.Events;
using PlayerRoles;
using PluginAPI.Core;

namespace DiscordIntegration.API.Configs
{
    using System;
    using System.ComponentModel;
    using System.Threading;
    using System.Threading.Tasks;
    using Dependency;
    using Mirror;
    using static DiscordIntegration;

    /// <summary>
    /// Represents bot-related configs.
    /// </summary>
    public sealed class Bot
    {
        /// <summary>
        /// Gets the bot update activity cancellation token.
        /// </summary>
        public static CancellationTokenSource UpdateActivityCancellationTokenSource { get; internal set; }

        /// <summary>
        /// Gets the bot update activity cancellation token.
        /// </summary>
        public static CancellationTokenSource UpdateChannelsTopicCancellationTokenSource { get; internal set; }

        /// <summary>
        /// Gets the bot IP address.
        /// </summary>
        [Description("Bot IP address")]
        public string IPAddress { get; private set; } = "127.0.0.1";

        /// <summary>
        /// Gets the bot port.
        /// </summary>
        [Description("Bot port")]
        public ushort Port { get; private set; } = 9000;

        /// <summary>
        /// Gets the bot status update interval.
        /// </summary>
        [Description("Bot status update interval, in seconds")]
        public float StatusUpdateInterval { get; private set; } = 25;

        /// <summary>
        /// Gets the channel topic update interval.
        /// </summary>
        [Description("Channel topic update interval, in seconds")]
        public float ChannelTopicUpdateInterval { get; private set; } = 300;

        /// <summary>
        /// Gets the bot reconnection update interval.
        /// </summary>
        [Description("Time to wait before trying to reconnect again, in seconds")]
        public float ReconnectionInterval { get; private set; } = 5;

        /// <summary>
        /// Updates the bot activity.
        /// </summary>
        /// <param name="cancellationToken">The task cancellation token.</param>
        /// <returns>Returns the <see cref="Task"/>.</returns>
        internal static async Task UpdateActivity(CancellationToken cancellationToken)
        {
            while (true)
            {
                try
                {
                    Log.Debug($"{nameof(UpdateActivity)}: Updating bot activity: {Player.GetPlayers().Count}/{ServerHandler.Plugin.Slots}", ServerHandler.Plugin.Config.IsDebugEnabled);
                    await Network.SendAsync(new RemoteCommand(ActionType.UpdateActivity, $"{Player.GetPlayers().Count}/{ServerHandler.Plugin.Slots}"), cancellationToken);
                    await Task.Delay(TimeSpan.FromSeconds(ServerHandler.Plugin.Config.Bot.StatusUpdateInterval), cancellationToken);
                }
                catch (Exception)
                {
                    await Task.Delay(TimeSpan.FromSeconds(ServerHandler.Plugin.Config.Bot.StatusUpdateInterval), cancellationToken);
                    // ignored
                }
            }
        }

        /// <summary>
        /// Updates channels topic.
        /// </summary>
        /// <param name="cancellationToken">The task cancellation token.</param>
        /// <returns>Returns the task.</returns>
        internal static async Task UpdateChannelsTopic(CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!NetworkClient.ready)
                {
                    await Task.Delay(TimeSpan.FromSeconds(ServerHandler.Plugin.Config.Bot.ChannelTopicUpdateInterval), cancellationToken);

                    continue;
                }

                try
                {
                    int aliveHumans = Player.GetPlayers().Count(player => player.IsAlive && player.IsHuman);
                    int aliveScps = Player.GetPlayers().Count(x => !x.IsHuman);

                    string warheadText = Warhead.IsDetonated ? Language.WarheadHasBeenDetonated : Warhead.IsDetonationInProgress ? Language.WarheadIsCountingToDetonation : Language.WarheadHasntBeenDetonated;

                    await Network.SendAsync(new RemoteCommand(ActionType.UpdateChannelActivity, $"{string.Format(Language.PlayersOnline, Player.GetPlayers().Count, ServerHandler.Plugin.Slots)}. {string.Format(Language.RoundDuration, Statistics.Round.Duration)}. {string.Format(Language.AliveHumans, aliveHumans)}. {string.Format(Language.AliveScps, aliveScps)}. {warheadText} IP: {Server.ServerIpAddress}:{Server.Port} TPS: {Handlers.GetServerTps()}"), cancellationToken);
                }
                catch (Exception exception)
                {
                    Log.Error(string.Format(Language.CouldNotUpdateChannelTopicError, exception));
                }

                await Task.Delay(TimeSpan.FromSeconds(ServerHandler.Plugin.Config.Bot.ChannelTopicUpdateInterval), cancellationToken);
            }
        }
    }
}
