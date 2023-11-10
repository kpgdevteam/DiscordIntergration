// -----------------------------------------------------------------------
// <copyright file="DiscordIntegration.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;

namespace DiscordIntegration
{
    using System;
    using System.Threading;
    using API;
    using API.Configs;
    using Events;

    public class DiscordIntegration
    {
        private MapHandler mapHandler;

        private ServerHandler serverHandler;

        private PlayerHandler playerHandler;

        private NetworkHandler networkHandler;
        
        private int slots;
        
        /// <summary>
        /// Gets the <see cref="API.Network"/> instance.
        /// </summary>
        public static Network Network { get; private set; }

        /// <summary>
        /// Gets or sets the network <see cref="CancellationTokenSource"/> instance.
        /// </summary>
        public static CancellationTokenSource NetworkCancellationTokenSource { get; internal set; }
        
        /// <summary>
        /// Gets the server slots.
        /// </summary>
        public int Slots
        {
            get
            {
                if (Server.MaxPlayers > 0)
                    slots = Server.MaxPlayers;
                return slots;
            }
        }

        public string Name = "DiscordIntegration";
        
        /// <summary>
        /// Creates an instance of the <see cref="Config"/>.
        /// </summary>
        [PluginConfig] public Config Config;
        
        /// <summary>
        /// Fired when the plugin is enabled.
        /// </summary>
        [PluginEntryPoint("DiscordIntegration", "7.1.0", "Event logging using Discord bots.", "Originally by Joker, Ported by Neil")]
        public void OnEnabled()
        {
            Network = new Network(Config.Bot.IPAddress, Config.Bot.Port, TimeSpan.FromSeconds(Config.Bot.ReconnectionInterval));

            NetworkCancellationTokenSource = new CancellationTokenSource();
            
            RegisterEvents();

            Bot.UpdateActivityCancellationTokenSource = new CancellationTokenSource();
            Bot.UpdateChannelsTopicCancellationTokenSource = new CancellationTokenSource();

            _ = Network.Start(NetworkCancellationTokenSource);

            _ = Bot.UpdateActivity(Bot.UpdateActivityCancellationTokenSource.Token);
            _ = Bot.UpdateChannelsTopic(Bot.UpdateChannelsTopicCancellationTokenSource.Token);
        }

        /// <summary>
        /// Fired when the plugin is disabled.
        /// </summary>
        [PluginUnload]
        public void OnDisabled()
        {
            NetworkCancellationTokenSource.Cancel();
            NetworkCancellationTokenSource.Dispose();

            Network.Close();

            Bot.UpdateActivityCancellationTokenSource.Cancel();
            Bot.UpdateActivityCancellationTokenSource.Dispose();

            Bot.UpdateChannelsTopicCancellationTokenSource.Cancel();
            Bot.UpdateChannelsTopicCancellationTokenSource.Dispose();

            UnregisterEvents();
            
            Network = null;
        }

        private void RegisterEvents()
        {
            mapHandler = new MapHandler(this);
            serverHandler = new ServerHandler(this);
            playerHandler = new PlayerHandler(this);
            networkHandler = new NetworkHandler(this);

            EventManager.RegisterEvents(this, mapHandler);
            EventManager.RegisterEvents(this, serverHandler);
            EventManager.RegisterEvents(this, playerHandler);

            Network.SendingError += networkHandler.OnSendingError;
            Network.ReceivingError += networkHandler.OnReceivingError;
            Network.UpdatingConnectionError += networkHandler.OnUpdatingConnectionError;
            Network.ConnectingError += networkHandler.OnConnectingError;
            Network.Connected += networkHandler.OnConnected;
            Network.Connecting += networkHandler.OnConnecting;
            Network.ReceivedFull += networkHandler.OnReceivedFull;
            Network.Sent += networkHandler.OnSent;
            Network.Terminated += networkHandler.OnTerminated;
        }
        
        private void UnregisterEvents()
        {

            EventManager.UnregisterEvents(this, mapHandler);
            EventManager.UnregisterEvents(this, serverHandler);
            EventManager.UnregisterEvents(this, playerHandler);

            Network.SendingError -= networkHandler.OnSendingError;
            Network.ReceivingError -= networkHandler.OnReceivingError;
            Network.UpdatingConnectionError -= networkHandler.OnUpdatingConnectionError;
            Network.ConnectingError -= networkHandler.OnConnectingError;
            Network.Connected -= networkHandler.OnConnected;
            Network.Connecting -= networkHandler.OnConnecting;
            Network.ReceivedFull -= networkHandler.OnReceivedFull;
            Network.Sent -= networkHandler.OnSent;
            Network.Terminated -= networkHandler.OnTerminated;

            mapHandler = null;
            serverHandler = null;
            playerHandler = null;
            networkHandler = null;
        }

    }
}