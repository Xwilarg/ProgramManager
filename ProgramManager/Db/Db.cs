﻿using RethinkDb.Driver;
using RethinkDb.Driver.Net;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            Tokens = new Dictionary<string, Response.SingleUser>();
            Processes = new Dictionary<string, LocalProcess>();
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
            else
            {
                foreach (var elem in R.Db(dbName).Table("Programs").Run(conn))
                {
                    Processes.Add((string)elem.id, new LocalProcess((string)elem.path));
                }
            }
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
            if (!IsAuthValid(username, password))
                return null;
            if (Tokens.Any(x => x.Value.Username == username))
                return Tokens.Where(x => x.Value.Username == username).First().Key;
            string token;
            do
            {
                token = GetRandomToken();
            } while (Tokens.ContainsKey(token));
            var user = R.Db(dbName).Table("Users").Get(username).Run(conn);
            Tokens.Add(token, new Response.SingleUser()
            {
                Username = username,
                Permissions = user.perms
            });
            return token;
        }

        public Response.SingleUser GetUserFromToken(string token)
            => Tokens[token];

        public bool HavePermission(string token, UserPermission neededPerm)
        {
            var user = Tokens[token];
            if (user.Permissions == -1)
                return true;
            return (user.Permissions & (int)neededPerm) > 0;
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

        public void AddProgram(string path, string name)
        {
            R.Db(dbName).Table("Programs").Insert(R.HashMap("id", name)
                .With("path", path)
            ).Run(conn);
            Processes.Add(name, new LocalProcess(path));
        }

        public void DeleteUser(string username)
        {
            R.Db(dbName).Table("Users").Filter(R.HashMap("id", username)).Delete().Run(conn);
        }

        public ReadOnlyCollection<Response.SingleUser> GetAllUsers()
        {
            List<Response.SingleUser> users = new List<Response.SingleUser>();
            foreach (var elem in R.Db(dbName).Table("Users").Run(conn))
            {
                users.Add(new Response.SingleUser()
                {
                    Permissions = elem.perms,
                    Username = elem.id
                });
            }
            return users.AsReadOnly();
        }

        public ReadOnlyCollection<Response.SingleProgram> GetAllPrograms()
        {
            List<Response.SingleProgram> programs = new List<Response.SingleProgram>();
            foreach (var elem in Processes)
            {
                programs.Add(new Response.SingleProgram()
                {
                    Name = elem.Key,
                    Stdout = elem.Value.GetStdout(),
                    IsStopped = elem.Value.IsStopped()
                });
            }
            return programs.AsReadOnly();
        }

        public bool DoesUserExists(string username)
            => R.Db(dbName).Table("Users").GetAll(username).Count().Eq(1).Run<bool>(conn);

        public bool IsTokenValid(string token)
            => Tokens.ContainsKey(token);

        /// <summary>
        /// Check if any account are registered
        /// (If not we will need to create master user)
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DoesAccountExists()
            => !await R.Db(dbName).Table("Users").Count().Eq(0).RunAsync<bool>(conn);

        public string GetRandomToken()
        {
            string str = "";
            for (int i = 0; i < 30; i++)
                str += (char)Program.P.Rand.Next(64, 127);
            return str;
        }

        private readonly RethinkDB R;
        private Connection conn;
        private readonly string dbName;
        public Dictionary<string, Response.SingleUser> Tokens { private set; get; } // Token / Username
        public Dictionary<string, LocalProcess> Processes { private set; get; }
    }
}
