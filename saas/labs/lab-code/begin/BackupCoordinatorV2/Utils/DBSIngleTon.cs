using Common.DTO.V2;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using System.Collections;
using System.Collections.Generic;


namespace BackupCoordinatorV2.Utils
{
    public class DBSingleTon
    {
        public static string _SQLConnectionString = System.Environment.GetEnvironmentVariable("MYSQLCONNSTR_OffSiteBackupSQLConnection");


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
        public void postBackup(string itemKey, string backupFileName, string pNewFileName, long fileLength, long startTimeStamp)
        {
            
                string sql = @"insert into backupHistory(startTimeStamp,endTimeStamp,customerGUID,backupFile,
                newFileName,fileLength) 
                values(@timeStamp,@endTimeStamp,@customerGUID,@backupFile,@pNewFileName,@fileLength)";
                using (MySqlConnection connection = new MySqlConnection(DBSingleTon._SQLConnectionString))
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    long timeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();

                    command.Parameters.AddWithValue("@timeStamp", startTimeStamp);
                    command.Parameters.AddWithValue("@endTimeStamp", timeStamp);
                    command.Parameters.AddWithValue("@fileLength", fileLength);

                    command.Parameters.AddWithValue("@customerGUID", itemKey);
                    command.Parameters.AddWithValue("@backupFile", backupFileName);
                    command.Parameters.AddWithValue("@pNewFileName", pNewFileName);

                    command.ExecuteNonQuery();

                }

               // return Ok();
            
        }
        public void putCache(string itemKey,string json)
        {
            bool updateSuccess = false;
            try
            {
                string stm = "UPDATE mycache set msg= @myJson where id =@myId";
                SqliteCommand cmd2 = new SqliteCommand(stm, DBSingleTon.Instance.getCon());
                cmd2.Parameters.AddWithValue("@myId", itemKey);
                cmd2.Parameters.AddWithValue("@myJson", json);
                cmd2.Prepare();
                int rows = cmd2.ExecuteNonQuery();
                if (rows > 0) updateSuccess = true;
            }
            catch (Exception ex)
            {
                DBSingleTon.Instance.write2Log(itemKey, "ERROR", ex.ToString());

            }
            if (!updateSuccess)
            {
                try
                {
                    string stm = "INSERT INTO mycache(id, msg,timeEntered, source) VALUES(@myId, @myJson, @timeEntered, @source)";
                    SqliteCommand cmd2 = new SqliteCommand(stm, DBSingleTon.Instance.getCon());
                    long unixTimeMilliseconds = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                    cmd2.Parameters.AddWithValue("@myId", itemKey);
                    cmd2.Parameters.AddWithValue("@source", "agent");
                    cmd2.Parameters.AddWithValue("@timeEntered", unixTimeMilliseconds);
                    cmd2.Parameters.AddWithValue("@myJson", json);
                    cmd2.Prepare();
                    cmd2.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    DBSingleTon.Instance.write2Log(itemKey, "ERROR", ex.ToString());
                }
            }
        }
        public void write2Log(string custGuid, string msg)
        {
            this.write2Log(custGuid, "INFO", msg);
        }
        public void write2Log(string custGuid, string pLogType, string msg)
        {

            long unixTimeMilliseconds = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();

            string stm = "INSERT INTO mylog(id, logTime,logType, msg) VALUES(@myId, @logTime,@logType,@myJson)";
            SqliteCommand cmd2 = new SqliteCommand(stm, DBSingleTon.Instance.getCon());
            cmd2.Parameters.AddWithValue("@myId", custGuid);
            cmd2.Parameters.AddWithValue("@logTime", unixTimeMilliseconds);
            cmd2.Parameters.AddWithValue("@logType", pLogType);
            cmd2.Parameters.AddWithValue("@myJson", msg);
            cmd2.Prepare();
            cmd2.ExecuteNonQuery();
            write2SQLLog(custGuid,pLogType,msg);
        }
       
        public void write2SQLLog(string custGuid, string pLogType, string msg)
        {
            if (pLogType.Equals("TRACE"))
                return;
            if (pLogType.Equals("DEBUG"))
                return;
            //write2Log(custGuid, pLogType, msg);
            long unixTimeMilliseconds = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();

            string stm = "INSERT INTO CustomerLogs(customerguid, dateEnteredtimestamp,msgType, msg) VALUES(@myId, @logTime,@logType,@myJson)";
           
            using (MySqlConnection connection = new MySqlConnection(_SQLConnectionString))
            using (MySqlCommand cmd2 = new MySqlCommand(stm, connection))
            {
                connection.Open();
                cmd2.Parameters.AddWithValue("@myId", custGuid);
                cmd2.Parameters.AddWithValue("@logTime", unixTimeMilliseconds);
                cmd2.Parameters.AddWithValue("@logType", pLogType);
                cmd2.Parameters.AddWithValue("@myJson", msg);
               // cmd2.Prepare();
                cmd2.ExecuteNonQuery();
            }

        }
        public List<LogMsgDTO> getLogs(string custGuid)
        {
            List<LogMsgDTO> ret = new List<LogMsgDTO>();
           // DateTime now = DateTime.UtcNow;
           // long unixTimeMilliseconds = new DateTimeOffset(now).ToUnixTimeMilliseconds();

            string stm = "select logTime,msg,logType from mylog where id=@myId order by logTime desc";
            SqliteCommand cmd2 = new SqliteCommand(stm, DBSingleTon.Instance.getCon());
            cmd2.Parameters.AddWithValue("@myId", custGuid);

            cmd2.Prepare();

            using SqliteDataReader rdr = cmd2.ExecuteReader();

            while (rdr.Read())
            {
                LogMsgDTO logMsgDTO = new LogMsgDTO();
                logMsgDTO.logTime = rdr.GetInt64(0);
                if (!rdr.IsDBNull(1))
                    logMsgDTO.msg = rdr.GetString(1);
                if (!rdr.IsDBNull(2))
                    logMsgDTO.logType = rdr.GetString(2);

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
