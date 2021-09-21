using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Timers;
using Websocket.Client;
using Websocket.Client.Exceptions;

namespace Guilded.NET.Base
{
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using Events;

    /// <summary>
    /// A base for Guilded client.
    /// </summary>
    /// <remarks>
    /// A base type for all Guilded.NET client containing WebSocket and REST things, as well as abstract methods to be overriden.
    /// </remarks>
    public abstract partial class BaseGuildedClient
    {
        internal const int welcome_opcode = 1, error_opcode = 8;
        /// <summary>
        /// The default timespan between each interval in milliseconds.
        /// </summary>
        public const double DefaultHeartbeatInterval = 22500;
        /// <summary>
        /// A dictionary of all websocket clients.
        /// </summary>
        /// <seealso cref="Rest"/>
        /// <value>Websocket dictionary</value>
        public Dictionary<string, WebsocketClient> Websockets
        {
            get; set;
        } = new Dictionary<string, WebsocketClient>();
        /// <summary>
        /// A timer for heartbeats.
        /// </summary>
        /// <remarks>
        /// A timer used for sending WebSocket heartbeats to Guilded.
        /// </remarks>
        /// <value>Timer</value>
        protected Timer HeartbeatTimer
        {
            get; set;
        }
        /// <summary>
        /// An event when WebSocket receives a message.
        /// </summary>
        /// <remarks>
        /// An event when WebSocket receives any kind of message from Guilded.
        /// </remarks>
        private readonly Subject<GuildedEvent> OnWebsocketMessage;
        /// <summary>
        /// An event when WebSocket receives a message.
        /// </summary>
        /// <remarks>
        /// An event when WebSocket receives any kind of message from Guilded.
        /// </remarks>
        protected IObservable<GuildedEvent> WebsocketMessage => OnWebsocketMessage.AsObservable();
        /// <summary>
        /// Initializes a new WebSocket client.
        /// </summary>
        /// <remarks>
        /// <para>Creates a new WebSocket client and adds it to <see cref="Websockets"/>.</para>
        /// <para>If <paramref name="lastMessageId"/> is passed, it gets all of the events that occurred after that message.</para>
        /// </remarks>
        /// <param name="lastMessageId">The identifier of the last event before WebSocket disconnection</param>
        /// <param name="websocketUrl">The URL to which WebSocket will connect</param>
        /// <exception cref="WebsocketException">Either <paramref name="lastMessageId"/> or <see cref="AdditionalHeaders"/> has a bad formatting</exception>
        /// <returns>Created websocket</returns>
        /// <seealso cref="ResumeEvent"/>
        /// <seealso cref="InitWebsocket(GuildedEvent)"/>
        protected virtual async Task<WebsocketClient> InitWebsocket(string lastMessageId = null, Uri websocketUrl = null)
        {
            Func<ClientWebSocket> factory = new Func<ClientWebSocket>(() =>
            {
                ClientWebSocket socket = new ClientWebSocket
                {
                    Options = {
                        Cookies = GuildedCookies
                    }
                };
                // Add additional headers for authentication tokens and such
                foreach (KeyValuePair<string, string> header in AdditionalHeaders)
                    socket.Options.SetRequestHeader(header.Key, header.Value);
                // If event identifier is passed, get every event after the given identifier
                if (!string.IsNullOrWhiteSpace(lastMessageId))
                    socket.Options.SetRequestHeader("guilded-last-message-id", lastMessageId);

                return socket;
            });
            WebsocketClient client = new WebsocketClient
            (
                websocketUrl ?? GuildedUrl.Websocket,
                factory
            );

            client.MessageReceived.Subscribe(WebsocketMessageReceived);
            await client.StartOrFail().ConfigureAwait(false);

            return client;
        }
        /// <summary>
        /// Initializes a new WebSocket client.
        /// </summary>
        /// <remarks>
        /// <para>Creates a new WebSocket client and adds it to <see cref="Websockets"/>.</para>
        /// <para>If <paramref name="event"/> is passed, it gets all of the events that occurred after that message.</para>
        /// </remarks>
        /// <param name="event">The last event before WebSocket disconnection</param>
        /// <exception cref="WebsocketException">Either <paramref name="event"/>'s identifier or <see cref="AdditionalHeaders"/> has a bad formatting</exception>
        /// <returns>Created websocket</returns>
        /// <seealso cref="ResumeEvent"/>
        /// <seealso cref="InitWebsocket(string, Uri)"/>
        protected virtual async Task<WebsocketClient> InitWebsocket(GuildedEvent @event) =>
            await InitWebsocket(@event.MessageId).ConfigureAwait(false);
        /// <summary>
        /// Used for when any WebSocket receives a message.
        /// </summary>
        /// <remarks>
        /// <para>An event handler method that gets called once any message is received from a WebSocket.</para>
        /// <para>Override this if you don't like how Guilded.NET handles events or need any additional changes/features to it.</para>
        /// </remarks>
        /// <param name="msg">Websocket message</param>
        protected virtual void WebsocketMessageReceived(ResponseMessage msg)
        {
            if (msg.MessageType == WebSocketMessageType.Text)
            {
                GuildedEvent @event = Deserialize<GuildedEvent>(msg.Text);
                // Check for a welcome message to change hearbeat interval
                if (@event.Opcode == welcome_opcode)
                {
                    HeartbeatTimer.Interval = @event.RawData.Value<double>("heartbeatIntervalMs");
                }
                else if(@event.Opcode == error_opcode)
                {
                    OnWebsocketMessage.OnError(
                        new GuildedWebsocketException(msg, @event.RawData.Value<string>("message"))
                    );
                    return;
                }
                OnWebsocketMessage.OnNext(@event);
            }
        }
        /// <summary>
        /// Sends a heartbeat.
        /// </summary>
        /// <remarks>
        /// <para>Sends a heartbeat through all WebSocket clients in <see cref="Websockets"/> dictionary.</para>
        /// </remarks>
        /// <param name="sender">Who invoked the event</param>
        /// <param name="args">Arguments of the timer's elapsed event</param>
        protected virtual void SendHeartbeat(object sender, ElapsedEventArgs args)
        {
            // Send ping through each WebSocket to show that it's not dead
            foreach (WebsocketClient client in Websockets.Values)
                client.Send("2");
        }
    }
}