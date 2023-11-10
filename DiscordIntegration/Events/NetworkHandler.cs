// -----------------------------------------------------------------------
// <copyright file="NetworkHandler.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

using PluginAPI.Core;

namespace DiscordIntegration.Events
{
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
    using System;
    using System.Net;
    using API.Commands;
    using API.EventArgs.Network;
    using API.User;
    using Dependency;
    using Newtonsoft.Json;
    using static DiscordIntegration;

    /// <summary>
    /// Handles network-related events.
    /// </summary>
    internal sealed class NetworkHandler
    {
        public NetworkHandler(DiscordIntegration plugin) => this.plugin = plugin;

        private readonly DiscordIntegration plugin;

        /// <inheritdoc cref="API.Network.OnReceivedFull(object, ReceivedFullEventArgs)"/>
        public void OnReceivedFull(object _, ReceivedFullEventArgs ev)
        {
            try
            {
                Log.Debug($"[NET] {string.Format(Language.ReceivedData, ev.Data, ev.Length)}", plugin.Config.IsDebugEnabled);
                if (ev.Data.Contains("heartbeat"))
                    return;

                RemoteCommand remoteCommand = JsonConvert.DeserializeObject<RemoteCommand>(ev.Data, Network.JsonSerializerSettings);

                Log.Debug($"[NET] {string.Format(Language.HandlingRemoteCommand, remoteCommand.Action, remoteCommand.Parameters[0], Network.TcpClient?.Client?.RemoteEndPoint)}", plugin.Config.IsDebugEnabled);

                switch (remoteCommand.Action)
                {
                    case ActionType.ExecuteCommand:
                        GameCommand command = new GameCommand(remoteCommand.Parameters[0].ToString(), remoteCommand.Parameters[1].ToString(), new DiscordUser(remoteCommand.Parameters[2].ToString(), remoteCommand.Parameters[3].ToString(), remoteCommand.Parameters[1].ToString()));
                        command?.Execute();
                        break;
                    case ActionType.CommandReply:
                        JsonConvert.DeserializeObject<CommandReply>(remoteCommand.Parameters[0].ToString(), Network.JsonSerializerSettings)?.Answer();
                        break;
                }
            }
            catch (Exception exception)
            {
                Log.Error($"[NET] {string.Format(Language.HandlingRemoteCommandError, plugin.Config.IsDebugEnabled ? exception.ToString() : exception.Message)}");
            }
        }

        /// <inheritdoc cref="API.Network.OnSendingError(object, SendingErrorEventArgs)"/>
        public void OnSendingError(object _, SendingErrorEventArgs ev)
        {
            Log.Error($"[NET] {string.Format(Language.SendingDataError, plugin.Config.IsDebugEnabled ? ev.Exception.ToString() : ev.Exception.Message)}");
        }

        /// <inheritdoc cref="API.Network.OnReceivingError(object, ReceivingErrorEventArgs)"/>
        public void OnReceivingError(object _, ReceivingErrorEventArgs ev)
        {
            Log.Error($"[NET] {string.Format(Language.ReceivingDataError, plugin.Config.IsDebugEnabled ? ev.Exception.ToString() : ev.Exception.Message)}");
        }

        /// <inheritdoc cref="API.Network.OnSent(object, SentEventArgs)"/>
        public void OnSent(object _, SentEventArgs ev) => Log.Debug(string.Format(Language.SentData, ev.Data, ev.Length), plugin.Config.IsDebugEnabled);

        /// <inheritdoc cref="API.Network.OnConnecting(object, ConnectingEventArgs)"/>
        public void OnConnecting(object _, ConnectingEventArgs ev)
        {
            if (!IPAddress.TryParse(plugin.Config?.Bot?.IPAddress, out IPAddress ipAddress))
            {
                Log.Error($"[NET] {string.Format(Language.InvalidIPAddress, plugin.Config?.Bot?.IPAddress)}");
                return;
            }

            ev.IPAddress = ipAddress;
            ev.Port = plugin.Config.Bot.Port;
            ev.ReconnectionInterval = TimeSpan.FromSeconds(plugin.Config.Bot.ReconnectionInterval);

            Log.Warning($"[NET] {string.Format(Language.ConnectingTo, ev.IPAddress, ev.Port)}");
        }

        /// <inheritdoc cref="API.Network.OnConnected(object, System.EventArgs)"/>
        public async void OnConnected(object _, System.EventArgs ev)
        {
            Log.Info($"[NET] {string.Format(Language.SuccessfullyConnected, Network.IPEndPoint?.Address, Network.IPEndPoint?.Port)}");

            await Network.SendAsync(new RemoteCommand(ActionType.Log, ChannelType.GameEvents, Language.ServerConnected));
        }

        /// <inheritdoc cref="API.Network.OnConnectingError(object, ConnectingErrorEventArgs)"/>
        public void OnConnectingError(object _, ConnectingErrorEventArgs ev)
        {
            Log.Error($"[NET] {string.Format(Language.ConnectingError, plugin.Config.IsDebugEnabled ? ev.Exception.ToString() : ev.Exception.Message)}");
        }

        /// <inheritdoc cref="API.Network.OnConnectingError(object, ConnectingErrorEventArgs)"/>
        public void OnUpdatingConnectionError(object _, UpdatingConnectionErrorEventArgs ev)
        {
            Log.Error($"[NET] {string.Format(Language.UpdatingConnectionError, plugin.Config.IsDebugEnabled ? ev.Exception.ToString() : ev.Exception.Message)}");
        }

        /// <inheritdoc cref="API.Network.OnTerminated(object, TerminatedEventArgs)"/>
        public void OnTerminated(object _, TerminatedEventArgs ev)
        {
            if (ev.Task.IsFaulted)
                Log.Error($"[NET] {string.Format(Language.ServerHasBeenTerminatedWithErrors, plugin.Config.IsDebugEnabled ? ev.Task.Exception.ToString() : ev.Task.Exception.Message)}");
            else
                Log.Warning($"[NET] {Language.ServerHasBeenTerminated}");

            NetworkCancellationTokenSource.Cancel();
            NetworkCancellationTokenSource.Dispose();

            NetworkCancellationTokenSource = new CancellationTokenSource();
            _ = Network.Start(NetworkCancellationTokenSource);
        }
    }
}
