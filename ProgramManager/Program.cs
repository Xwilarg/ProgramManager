using Nancy.Hosting.Self;
using SimpleCrypto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ProgramManager
{
    public class Program
    {
        public static async Task Main(string[] _)
               => await new Program().InitAsync();

        public Dictionary<string, UserPermission[]> Tokens { private set; get; } // Master token, only used for first connection
        public string Salt { private set; get; } // Token's salt

        public static Program P { private set; get; } // Static reference towards this class
        public Random Rand { private set; get; }
        public Db.Db ProgDb { private set; get; }

        public ICryptoService Crypto { private set; get; } // Crypto service to generate hashes (PBKDF2z)

        private async Task InitAsync()
        {
            P = this;

            // Init authentification service
            if (!Directory.Exists("Keys") || !File.Exists("Keys/salt.txt"))
                throw new FileNotFoundException("Missing Keys/salt.txt");
            Salt = File.ReadAllText("Keys/salt.txt");
            Crypto = new PBKDF2();
            Rand = new Random();

            // Init db
            ProgDb = new Db.Db();
            await ProgDb.InitAsync();
            Tokens = new Dictionary<string, UserPermission[]>();
            string masterToken = await ProgDb.GetMasterToken();
            if (masterToken != null)
            {
                Tokens.Add(masterToken, new[] { UserPermission.ManageUser });
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
