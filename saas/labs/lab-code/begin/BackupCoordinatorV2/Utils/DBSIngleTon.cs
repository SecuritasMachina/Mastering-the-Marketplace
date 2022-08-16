using Microsoft.Data.Sqlite;

namespace BackupCoordinatorV2.Utils
{
    public class DBSIngleTon
    {
        private DBSIngleTon()
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
            con = new SqliteConnection(connectionStringBuilder.ToString());
            con.Open();
        }
        public SqliteConnection getCon()
        {
            return con;
        }
        private static SqliteConnection con;
        private static DBSIngleTon instance = null;
       
        public static DBSIngleTon Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DBSIngleTon();
                }
                return instance;
            }
        }

    }
}
