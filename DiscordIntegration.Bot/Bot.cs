using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Tasks;
using DiscordIntegration.Dependency.Database;
using DiscordIntegration.Bot.ConfigObjects;
using DiscordIntegration.Bot.Services;
using DiscordIntegration.Dependency;
using System.Drawing;
using DiscordIntegration.API.EventArgs.Network;
using Microsoft.Extensions.Logging;

namespace DiscordIntegration.Bot
{
    public class Bot
    {
        private DiscordClient client;
        private DiscordGuild guild;
        private string token;
        private int lastCount = -1;
        private int lastTotal = 0;
        internal readonly ConcurrentDictionary<LogChannel, string> Messages = new();

        public ushort ServerNumber { get; }
        public DiscordClient Client => client ??= new DiscordClient(new DiscordConfiguration
        {
            Token = Program.Config.BotTokens.ToString(),
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged // Specify your intents as needed
        });

        public DiscordGuild Guild => guild ??= Client.GetGuildAsync(Program.Config.DiscordServerIds[ServerNumber]).Result; // Be cautious with .Result in async code
        public SlashCommandsExtension SlashCommands { get; private set; }
        public TcpServer Server { get; private set; }

        public Bot(ushort serverNumber, string token)
        {
            ServerNumber = serverNumber;
            this.token = token;
            Task.Run(Init).ConfigureAwait(false);
        }

        public async Task Destroy() => await Client.DisconnectAsync();

        private async Task Init()
        {
            try
            {
                var discord = new DiscordClient(new DiscordConfiguration()
                {
                    Token = Program.Config.BotTokens[ServerNumber],
                    TokenType = TokenType.Bot,
                    Intents = DiscordIntents.All,
                    MinimumLogLevel = LogLevel.Debug
                });
            }
            catch (Exception e)
            {
                Console.WriteLine($"{ServerNumber}, {nameof(Init)}, {e}");
                return;
            }

            DatabaseHandler.Init();
            Console.WriteLine($"{ServerNumber}, {nameof(Init)}, {"Setting up commands..."}");
            SlashCommands = Client.UseSlashCommands();

            // Logging setup...
            Client.Ready += OnClientReady;
            Server.ReceivedFull += OnReceived;

            await Client.ConnectAsync();

            // TCP Server and other initializations...
        }

        private Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            Console.WriteLine($"{ServerNumber}, {nameof(Init)}, {"Logging in.."}");
            Server = new TcpServer(Program.Config.TcpServers[ServerNumber].IpAddress, Program.Config.TcpServers[ServerNumber].Port, this);
            _ = Server.Start();

            return Task.CompletedTask;
        }

        private async void OnReceived(object? sender, ReceivedFullEventArgs ev)
        {
            try
            {
                Console.WriteLine($"{ServerNumber}, {nameof(OnReceived)}, {$"Received data {ev.Data}"}");
                RemoteCommand command = JsonConvert.DeserializeObject<RemoteCommand>(ev.Data)!;
                Console.WriteLine($"{ServerNumber}, {nameof(OnReceived)}, {$"Received command {command.Action}."}");
                 
                switch (command.Action)
                {
                    case ActionType.Log:
                        Console.WriteLine($"{ServerNumber}, {nameof(OnReceived)}, {$"{Enum.TryParse(command.Parameters[0].ToString(), true, out Dependency.ChannelType _)}"}");
                        if (Enum.TryParse(command.Parameters[0].ToString(), true, out Dependency.ChannelType type))
                        {
                            foreach (LogChannel channel in Program.Config.Channels[ServerNumber].Logs[type])
                            {
                                Console.WriteLine($"{ServerNumber}, {nameof(OnReceived)}, {"Adding message to queue.."}");
                                if (!Messages.ContainsKey(channel))
                                    Messages.TryAdd(channel, string.Empty);
                                Messages[channel] += $"[{DateTime.Now}] {command.Parameters[1]}\n";
                            }

                            break;
                        }

                        Console.WriteLine($"{ServerNumber}, {nameof(OnReceived)}, {"Failed to add message to queue."}");
                        break;
                    case ActionType.SendMessage:
                        if (ulong.TryParse(command.Parameters[0].ToString(), out ulong chanId))
                        {
                            string[] split = command.Parameters[1].ToString()!.Split("|");
                            await Guild.GetChannel(chanId).SendMessageAsync(embed: await EmbedBuilderService.CreateBasicEmbed(ServerNumber + split[0].TrimEnd('|'), split[1].TrimStart('|'), (bool)command.Parameters[2] ? DiscordColor.Green : DiscordColor.Red));
                        }

                        break;
                    case ActionType.UpdateActivity:
                        string commandMessage = string.Empty;
                        foreach (object obj in command.Parameters)
                            commandMessage += (string)obj + " ";
                        Console.WriteLine($"{ServerNumber}, {nameof(OnReceived)}, {$"Updating activity status.. {commandMessage}"}");
                        try
                        {
                            string[] split = ((string)command.Parameters[0]).Split('/');
                            if (!int.TryParse(split[0], out int count))
                            {
                                Console.WriteLine($"{ServerNumber}, {nameof(ActionType.UpdateActivity)}, {$"Error parsing player count {split[0]}"}");
                                return;
                            }

                            if (!int.TryParse(split[1], out int total))
                            {
                                Console.WriteLine($"{ServerNumber}, {nameof(ActionType.UpdateActivity)}, {$"Error parsing player total {split[1]}"}");
                                return;
                            }

                            switch (count)
                            {
                                case > 0:
                                    await Client.UpdateStatusAsync(new DiscordActivity("Starting...", DSharpPlus.Entities.ActivityType.Playing), DSharpPlus.Entities.UserStatus.Online);
                                    break;
                                case 0:
                                    await Client.UpdateStatusAsync(new DiscordActivity("Offline...", DSharpPlus.Entities.ActivityType.Playing), UserStatus.DoNotDisturb);
                                    break;
                            }

                            Console.WriteLine($"{ServerNumber}, {nameof(OnReceived)}, {$"Status message count: {count}"}");
                            Console.WriteLine($"{ServerNumber}, {nameof(OnReceived)}, {$"Status message total: {total}"}");
                            if (count != lastCount || total != lastTotal)
                            {
                                lastCount = count;
                                if (total > 0)
                                    lastTotal = total;
                                await Client.UpdateStatusAsync(new DiscordActivity($"{lastCount}/{lastTotal}", ActivityType.Playing), UserStatus.Online);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"{ServerNumber}, {nameof(OnReceived)}, {"Error updating bot status. Enable debug for more information."}");
                            Console.WriteLine($"{ServerNumber}, {nameof(OnReceived)}, {e}");
                        }

                        break;
                    case ActionType.UpdateChannelActivity:
                        foreach (ulong channelId in Program.Config.Channels[ServerNumber].TopicInfo)
                        {
                            var channel = Client.GetChannelAsync(channelId).Result as DiscordChannel;
                            if (channel is not null)
                            {
                                _ = channel.ModifyAsync(x => x.Topic = (string)command.Parameters[0]);
                            }
                        }

                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{ServerNumber}, {nameof(OnReceived)}, {e.Message}");
                if (e.StackTrace is not null)
                    Console.WriteLine($"{ServerNumber}, {nameof(OnReceived)}, {e.StackTrace}");
            }
        }
        private async Task DequeueMessages()
        {
            for (; ; )
            {
                Console.WriteLine($"{ServerNumber}, {nameof(DequeueMessages)}, {"Dequeue loop"}");
                List<KeyValuePair<LogChannel, string>> toSend = new();
                lock (Messages)
                {
                    foreach (KeyValuePair<LogChannel, string> message in Messages)
                        toSend.Add(message);

                    Messages.Clear();
                }

                foreach (KeyValuePair<LogChannel, string> message in toSend)
                {
                    try
                    {
                        if (message.Value.Length > 1900)
                        {
                            string msg = string.Empty;
                            string[] split = message.Value.Split('\n');
                            int i = 0;
                            while (msg.Length < 1900)
                            {
                                msg += split[i] + "\n";
                                i++;
                            }

                            switch (message.Key.LogType)
                            {
                                case LogType.Embed:
                                    _ = Guild.GetChannel(message.Key.Id).SendMessageAsync(embed: await EmbedBuilderService.CreateBasicEmbed($"Server {ServerNumber} Logs", msg, DiscordColor.Green));
                                    break;
                                case LogType.Text:
                                    _ = Guild.GetChannel(message.Key.Id).SendMessageAsync($"[{ServerNumber}]: {msg}");
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            Messages.TryAdd(message.Key, message.Value.Substring(msg.Length));
                        }
                        else
                        {
                            Console.WriteLine($"{ServerNumber}, {nameof(DequeueMessages)}, {$"Sending message to {message.Key.Id}: {message.Key.LogType} -- {message.Value}"}");
                            switch (message.Key.LogType)
                            {
                                case LogType.Embed:
                                    await Guild.GetChannel(message.Key.Id).SendMessageAsync(embed: await EmbedBuilderService.CreateBasicEmbed($"Server {ServerNumber} Logs", message.Value, DiscordColor.Green));
                                    break;
                                case LogType.Text:
                                    await Guild.GetChannel(message.Key.Id).SendMessageAsync($"[{ServerNumber}]: {message.Value}");
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            Console.WriteLine($"{ServerNumber}, {nameof(DequeueMessages)}, {"Message sent."}");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"{ServerNumber}, {nameof(DequeueMessages)}, {$"{e.Message}\nThis is likely caused because {message.Key.Id} is not a valid channel ID, or an invalid GuildID: {Program.Config.DiscordServerIds[ServerNumber]}. If the GuildID is correct, to avoid this error, disabling the logging of events targeting channels that you've purposefully set to an invalid channel ID.\nEnable debug mode to show the contents of the messages causing this error."}");
                        Console.WriteLine($"{ServerNumber}, {nameof(DequeueMessages)}, {$"{e.Message}\n{message.Value}"}");
                    }
                }

                Console.WriteLine($"{ServerNumber}, {nameof(DequeueMessages)}, {$"Waiting {Program.Config.MessageDelay} ms"}");
                await Task.Delay(Program.Config.MessageDelay);
            }
        }
    }
}