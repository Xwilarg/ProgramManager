using Nancy.Hosting.Self;
using SimpleCrypto;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProgramManager
{
    public class Program
    {
        public static async Task Main(string[] _)
               => await new Program().InitAsync();

        public static Program P { private set; get; } // Static reference towards this class
        public Random Rand { private set; get; }
        public Db.Db ProgDb { private set; get; }

        public ICryptoService Crypto { private set; get; } // Crypto service to generate hashes (PBKDF2z)

        public Dictionary<string, Response.SingleUser> UserRequests { private set; get; } // token / user

        private async Task InitAsync()
        {
            P = this;

            // Init authentification service
            Crypto = new PBKDF2();
            Rand = new Random();
            UserRequests = new Dictionary<string, Response.SingleUser>();

            // Init db
            ProgDb = new Db.Db();
            await ProgDb.InitAsync();
            if (!await ProgDb.DoesAccountExists()) // No master account, we need to create one
            {
                Console.WriteLine("Creating master user.");
                Console.WriteLine("Enter your username:");
                string username = Console.ReadLine();
                Console.WriteLine("Enter your password:");
                string password = Console.ReadLine();
                ProgDb.AddUser(username, password, -1);
                Console.WriteLine("Account created. Press enter to continue...");
                Console.ReadKey();
                Console.Clear();
            }

            // Init backend
            AutoResetEvent autoEvent = new AutoResetEvent(false);
            LaunchServer(autoEvent);

            autoEvent.WaitOne();
        }

        private void LaunchServer(AutoResetEvent autoEvent)
        {
            HostConfiguration config = new HostConfiguration()
            {
                UrlReservations = new UrlReservations() { CreateAutomatically = true }
            };
            NancyHost host = new NancyHost(config, new Uri("http://localhost:3030"));
            host.Start();
            Console.WriteLine("Host started... Do ^C to exit.");
            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("^C received, exitting...");
                host.Dispose();
                autoEvent.Set();
            };
        }
    }
}
