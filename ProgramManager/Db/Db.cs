using RethinkDb.Driver;
using RethinkDb.Driver.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProgramManager.Db
{
    public class Db
    {
        public Db(string dbName = "ProgramManager")
        {
            this.dbName = dbName;
            R = RethinkDB.R;
            Tokens = new Dictionary<string, string>();
        }

        public async Task InitAsync()
        {
            conn = await R.Connection().ConnectAsync();
            if (!await R.DbList().Contains(dbName).RunAsync<bool>(conn))
                R.DbCreate(dbName).Run(conn);
            if (!await R.Db(dbName).TableList().Contains("Users").RunAsync<bool>(conn))
                R.Db(dbName).TableCreate("Users").Run(conn);
            if (!await R.Db(dbName).TableList().Contains("Programs").RunAsync<bool>(conn))
                R.Db(dbName).TableCreate("Programs").Run(conn);
        }

        /// <summary>
        /// Does username and password leads to a valid account
        /// </summary>
        public bool IsAuthValid(string username, string password)
        {
            if (DoesUserExists(username)) // username exists in db
            {
                var user = R.Db(dbName).Table("Users").Get(username).Run(conn);
                return Program.P.Crypto.Compare(Program.P.Crypto.Compute(password, (string)user.salt), (string)user.password);
            }
            return false;
        }

        /// <summary>
        /// Get token session from username
        /// </summary>
        public string GetTokenFromLogin(string username, string password)
        {
            if (Tokens.ContainsValue(username))
                return Tokens.Where(x => x.Value == username).First().Key;
            if (!IsAuthValid(username, password))
                return null;
            string token;
            do
            {
                token = GetRandomToken();
            } while (Tokens.ContainsValue(token));
            Tokens.Add(token, username);
            return token;
        }

        public void AddUser(string username, string password, int perms)
        {
            string salt = Program.P.Crypto.GenerateSalt();
            string hash = Program.P.Crypto.Compute(password, salt);
            R.Db(dbName).Table("Users").Insert(R.HashMap("id", username)
                .With("password", hash)
                .With("salt", salt)
                .With("perms", perms)
            ).Run(conn);
        }

        public bool DoesUserExists(string username)
            => R.Db(dbName).Table("Users").GetAll(username).Count().Eq(1).Run<bool>(conn);

        public bool IsTokenValid(string token)
            => Tokens.ContainsValue(token);

        /// <summary>
        /// Check if any account are registered
        /// (If not we will need to create master user)
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DoesAccountExists()
            => await R.Db(dbName).Table("Users").Count().Eq(1).RunAsync<bool>(conn);

        private string GetRandomToken()
        {
            string str = "";
            for (int i = 0; i < 30; i++)
                str += (char)Program.P.Rand.Next(33, 127);
            return str;
        }

        private readonly RethinkDB R;
        private Connection conn;
        private readonly string dbName;
        public Dictionary<string, string> Tokens { private set; get; } // Token / Username
    }
}
