using RethinkDb.Driver;
using RethinkDb.Driver.Net;
using System.Threading.Tasks;

namespace ProgramManager.Db
{
    public class Db
    {
        public Db(string dbName = "ProgramManager")
        {
            this.dbName = dbName;
            R = RethinkDB.R;
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
        /// If there is no user, we create a master token so we can create the others users
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetMasterToken()
        {
            if (await R.Db(dbName).Table("Users").Count().Eq(0).RunAsync<bool>(conn))
            {
                return GetRandomToken();
            }
            return null;
        }

        private string GetRandomToken()
        {
            string str = "";
            for (int i = 0; i < 20; i++)
                str += (char)Program.P.Rand.Next(33, 127);
            return str;
        }

        private readonly RethinkDB R;
        private Connection conn;
        private readonly string dbName;
    }
}
