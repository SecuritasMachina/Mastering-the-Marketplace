using Common.DTO.V2;
using Microsoft.Data.Sqlite;
using System.Collections;
using System.Collections.Generic;

namespace BackupCoordinatorV2.Utils
{
    public class DBSingleTon
    {
        private static SqliteConnection? con;
        private static DBSingleTon? instance;
        private DBSingleTon()
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
            con = new SqliteConnection(connectionStringBuilder.ToString());
            con.Open();
        }
        public SqliteConnection getCon()
        {
            return con;
        }
        public void write2Log(string custGuid,  string msg)
        {
            this.write2Log(custGuid, "INFO", msg);
        }
            public void write2Log(string custGuid, string pLogType,string msg)
        {
            DateTime now = DateTime.UtcNow;
            long unixTimeMilliseconds = new DateTimeOffset(now).ToUnixTimeMilliseconds();

            string stm = "INSERT INTO mylog(id, logTime,logType, msg) VALUES(@myId, @logTime,@logType,@myJson)";
            SqliteCommand cmd2 = new SqliteCommand(stm, DBSingleTon.Instance.getCon());
            cmd2.Parameters.AddWithValue("@myId", custGuid);
            cmd2.Parameters.AddWithValue("@logTime", unixTimeMilliseconds);
            cmd2.Parameters.AddWithValue("@logType", pLogType);
            cmd2.Parameters.AddWithValue("@myJson", msg);
            cmd2.Prepare();
            cmd2.ExecuteNonQuery();
            
        }
        public List<LogMsgDTO> getLogs(string custGuid)
        {
            List<LogMsgDTO> ret = new List<LogMsgDTO>();
            DateTime now = DateTime.UtcNow;
            long unixTimeMilliseconds = new DateTimeOffset(now).ToUnixTimeMilliseconds();

            string stm = "select logTime,msg from mylog where id=@myId order by logTime";
            SqliteCommand cmd2 = new SqliteCommand(stm, DBSingleTon.Instance.getCon());
            cmd2.Parameters.AddWithValue("@myId", custGuid);
           
            cmd2.Prepare();
            
            using SqliteDataReader rdr = cmd2.ExecuteReader();

            while (rdr.Read())
            {
                LogMsgDTO logMsgDTO = new LogMsgDTO();
                logMsgDTO.logTime = rdr.GetInt32(0);
                logMsgDTO.msg = rdr.GetString(1);
                logMsgDTO.id = custGuid;
                ret.Add(logMsgDTO);
               // Console.WriteLine($"{rdr.GetInt32(0)} {rdr.GetString(1)} {rdr.GetInt32(2)}");
            }
            return ret;

        }
        
       
        public static DBSingleTon Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DBSingleTon();
                }
                return instance;
            }
        }

        
    }
}
