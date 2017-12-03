using System;
using System.Net;
using Newtonsoft.Json;
using WebSocketSharp;

namespace WebRCON_Runner
{
    class Connection
    {
        private Action<string, bool> outputFunction;
        private Action connectedCallback;

        private IPAddress ipAddress;
        private int port;
        private string password;

        private WebSocket socket;

        public Connection(Action<string, bool> outputFunction, IPAddress ipAddress, int port, string password)
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

            outputFunction.Invoke($"Connecting to {wsURL}...", false);

            socket.Connect();
        }

        public void Disconnect()
        {
            socket?.Close();
        }

        public bool IsOpen() => socket.ReadyState == WebSocketState.Open;

        public void SendCommand(string command)
        {
            var packet = new
            {
                Identifier = 1,
                Message = command,
                Name = "WebRCON-Runner"
            };

            string contents = JsonConvert.SerializeObject(packet);

            socket.Send(contents);

            outputFunction.Invoke($"Sent command: {command}", false);
        }

        private void Socket_OnOpen(object sender, EventArgs e)
        {
            outputFunction.Invoke($"Connection opened!", false);

            connectedCallback.Invoke();
        }

        private void Socket_OnClose(object sender, CloseEventArgs e)
        {
            outputFunction.Invoke($"Connection closed!", false);
        }

        private void Socket_OnMessage(object sender, MessageEventArgs e)
        {
            outputFunction.Invoke(e.Data, true);
        }

        private void Socket_OnError(object sender, ErrorEventArgs e)
        {
            outputFunction.Invoke($"Connection error: {e.Message}", false);
            outputFunction.Invoke(e.Exception.ToString(), false);
        }
    }
}
