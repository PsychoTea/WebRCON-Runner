using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using WebSocketSharp;

namespace WebRCON_Runner
{
    class Connection
    {
        private Action<string> outputFunction;
        private Action connectedCallback;

        private IPAddress ipAddress;
        private int port;
        private string password;

        private WebSocket socket;

        public Connection(Action<string> outputFunction, IPAddress ipAddress, int port, string password)
        {
            this.outputFunction = outputFunction;
            this.ipAddress = ipAddress;
            this.port = port;
            this.password = password;
        }

        public void Connect(Action connectedCallback)
        {
            this.connectedCallback = connectedCallback;

            string wsURL = $"ws://{ipAddress}:{port}/{password}";

            socket = new WebSocket(wsURL)
            {
                WaitTime = TimeSpan.FromSeconds(5)
            };

            socket.OnOpen += Socket_OnOpen;
            socket.OnMessage += Socket_OnMessage;
            socket.OnClose += Socket_OnClose;
            socket.OnError += Socket_OnError;

            outputFunction.Invoke($"Connecting to {wsURL}...");

            socket.Connect();
        }

        public void Disconnect()
        {
            socket?.Close();
        }

        public void SendCommand(string command)
        {
            Dictionary<string, object> packet = new Dictionary<string, object>()
            {
                { "Identifier", 1},
                {"Message", command },
                { "Name", "WebRCON-Runner" }
            };

            string contents = JsonConvert.SerializeObject(packet);

            socket.Send(contents);

            outputFunction.Invoke($"Sent command: {command}");
        }

        private void Socket_OnOpen(object sender, EventArgs e)
        {
            outputFunction.Invoke($"Connection opened!");

            connectedCallback.Invoke();
        }

        private void Socket_OnClose(object sender, CloseEventArgs e)
        {
            outputFunction.Invoke($"Connection closed!");
        }

        private void Socket_OnMessage(object sender, MessageEventArgs e)
        {
            outputFunction.Invoke($"Message recieved:");
            outputFunction.Invoke(e.Data);
        }

        private void Socket_OnError(object sender, ErrorEventArgs e)
        {
            outputFunction.Invoke($"Connection error: {e.Message}");
            outputFunction.Invoke(e.Exception.ToString());
        }
    }
}
