using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json;

namespace WebRCON_Runner
{
    class Program
    {
        private static Connection connection;

        private static IPAddress ipAddress;
        private static int port;
        private static string password;
        private static string command;
        private static string listenFor;

        // Kind of band-aid way to stop the program closing
        // beofre we've recieved the 'listenFor' match
        private static Thread thread;

        private static bool ListenForEnabled => !string.IsNullOrEmpty(listenFor);

        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            if (args.Length < 4)
            {
                PrintUsage();
                return;
            }

            if (!IPAddress.TryParse(args[0], out ipAddress))
            {
                PrintUsage();
                return;
            }

            if (!Int32.TryParse(args[1], out port))
            {
                PrintUsage();
                return;
            }

            password = args[2];

            command = args[3];

            if (args.Length > 4)
            {
                listenFor = args[4];

                thread = new Thread(() => RunThread());
                thread.Start();
            }

            Console.WriteLine($"IP: {ipAddress}:{port}");
            Console.WriteLine($"Command: {command}");

            if (ListenForEnabled)
            {
                Console.WriteLine($"Listening For: {listenFor}");
            }

            connection = new Connection((x, y) => ConnectionOutput(x, y), ipAddress, port, password);

            connection.Connect(() => OnConnection());
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string name = new AssemblyName(args.Name).Name;

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"WebRCON_Runner.Dependencies.{name}.dll"))
            {
                byte[] data = new BinaryReader(stream).ReadBytes((int)stream.Length);

                return Assembly.Load(data);
            }
        }

        private static void ConnectionOutput(string message, bool socketMessage)
        {
            RCONMessage rconMessage = null;
            try
            {
                rconMessage = JsonConvert.DeserializeObject<RCONMessage>(message);
            }
            catch (Exception ex) { }

            Console.WriteLine("[Connection] " + (socketMessage ? "Message recieved: " : "") + (rconMessage?.Message ?? message));

            if (ListenForEnabled && rconMessage != null)
            {
                string lowerMessage = rconMessage.Message.ToLower();

                if (lowerMessage.Contains(listenFor.ToLower()))
                {
                    Console.WriteLine($"Recieved listen-for message '{rconMessage.Message}'. Closing...");

                    Thread.Sleep(1000);

                    connection.Disconnect();

                    Console.WriteLine("Closed.");

                    thread.Abort();
                }
            }
        }

        private static void OnConnection()
        {
            connection.SendCommand(command);

            if (ListenForEnabled) return;

            Console.WriteLine($"Finished. Closing...");

            // Wait one second to allow the server to send 
            // back any responses to the command
            Thread.Sleep(1000);

            connection.Disconnect();
        }

        private static void RunThread()
        {
            while (true)
            {
            }
        }

        private static void PrintUsage()
        {
            string programName = AppDomain.CurrentDomain.FriendlyName;

            Console.WriteLine($"Usage: {programName} {{ip}} {{port}} {{password}} \"{{command}}\" \"[listen-for]\"");
        }
    }
}
