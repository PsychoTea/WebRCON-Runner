using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;

namespace WebRCON_Runner
{
    class Program
    {
        private static Connection connection;

        private static IPAddress ipAddress;
        private static int port;
        private static string password;
        private static string command;

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

            command = string.Join(" ", args.Skip(3).ToArray());
            
            connection = new Connection(x => ConnectionOutput(x), ipAddress, port, password);

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

        private static void ConnectionOutput(string message)
        {
            Console.WriteLine($"[Connection] {message}");
        }

        public static void OnConnection()
        {
            connection.SendCommand(command);

            Console.WriteLine($"Finished. Closing...");

            // Wait one second to allow the server to send 
            // back any responses to the command
            Thread.Sleep(1000);

            connection.Disconnect();
        }

        private static void PrintUsage()
        {
            string programName = AppDomain.CurrentDomain.FriendlyName;

            Console.WriteLine($"Usage: {programName} {{ip}} {{port}} {{password}} {{command...}}");
        }
    }
}
