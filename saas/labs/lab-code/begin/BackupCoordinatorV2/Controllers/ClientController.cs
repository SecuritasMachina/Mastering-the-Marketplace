using BackupCoordinatorV2.Utils;
using Common.DTO.V2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using System.Data;

using System.Net;
using System.Net.Mime;
using WebListener;
using Microsoft.Data.SqlClient;
using Microsoft.Azure.ServiceBus.Management;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Azure.Identity;

namespace BackupCoordinatorV2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly ILogger<ClientController> _logger;

        public ClientController(ILogger<ClientController> logger)
        {
            _logger = logger;
        }
        [HttpPost]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes("application/json")]
        [Route("/api/v2/recordBackup/{itemKey}")]
        public async Task<ActionResult<string>> recordBackup([FromBody] object value, string itemKey)
        {

            try
            {

                // logger.LogInformation("/v2/recordBackup:" + json);

                //System.Diagnostics.Trace.TraceWarning("!! /v2/recordBackup !!");
                //Post to service bus for particular client
                await new WebWorker().RecordBackupAsync(json: value.ToString());

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }
            return value.ToString();
        }

        [HttpPost]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes("application/json")]
        [Route("/api/v2/requestRestore/{customerGuid}")]
        public async Task<ActionResult<string>> RequestRestoreAsync([FromBody] object value, string customerGuid)
        {

            try
            {

                //Post to service bus
                if (value != null)
                    await new WebWorker().RequestRestoreAsync(value.ToString());


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }
            return value.ToString();
        }


        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [Route("/api/v3/deleteCache/{itemKey}")]
        public ActionResult<string> deleteCache(string itemKey)
        {
            string stm = "delete from mycache where id =@myId";
            SqliteCommand cmd2 = new SqliteCommand(stm, DBSingleTon.Instance.getCon());
            cmd2.Parameters.AddWithValue("@myId", itemKey);

            cmd2.Prepare();
            cmd2.ExecuteNonQuery();
            return JsonConvert.SerializeObject($"Deleted {itemKey}");
        }
        [HttpPost]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes("application/json")]
        [Route("/api/v3/putCache/{itemKey}")]
        public ActionResult<string> PutCache([FromBody] object pFormBody, string itemKey)
        {

            string json = pFormBody.ToString();
            try
            {

                _logger.LogInformation("/v3/putCache/" + itemKey + " " + json);
                try
                {
                    string stm = "UPDATE mycache set msg= @myJson where id =@myId";
                    SqliteCommand cmd2 = new SqliteCommand(stm, DBSingleTon.Instance.getCon());
                    cmd2.Parameters.AddWithValue("@myId", Uri.EscapeUriString(itemKey));
                    cmd2.Parameters.AddWithValue("@myJson", json);
                    cmd2.Prepare();
                    cmd2.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
                try
                {
                    string stm = "INSERT INTO mycache(id, msg) VALUES(@myId, @myJson)";
                    SqliteCommand cmd2 = new SqliteCommand(stm, DBSingleTon.Instance.getCon());
                    cmd2.Parameters.AddWithValue("@myId", Uri.EscapeUriString(itemKey));
                    cmd2.Parameters.AddWithValue("@myJson", json);
                    cmd2.Prepare();
                    cmd2.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    if (!ex.Message.ToLower().Contains("unique"))
                        _logger.LogError(ex.ToString());
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw new Exception(ex.Message);

            }
            return json;

        }
        //https://localhost:7074/api/v3/postBackupHistory/ab50c41e-3814-4533-8f68-a691b4da9043/Charles+Havranek+Resume+-+LongD9.pdf/Charles+Havranek+Resume+-+LongD9.pdf/147024
        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        //[Consumes("application/json")]
        [Route("/api/v3/postBackupHistory/{itemKey}/{backupFileName}/{pNewFileName}/{fileLength}")]
        public ActionResult<string> postBackupHistory(string itemKey, string backupFileName, string pNewFileName, long fileLength)
        {

            //string json = pFormBody.ToString();
            _logger.LogInformation("/apt/v1/postBackupHistory");
            try
            {
                //_logger.LogInformation("looking up " + customerGuid);

                //string connectionString = System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_OffSiteServiceBusConnection");
            //    string SQLConnectionString = System.Environment.GetEnvironmentVariable("SQLAZURECONNSTR_OffSiteBackupSQLConnection");



                string sql = "insert into backupHistory([timeStamp2],[customerGUID],[backupFile],[newFileName],[fileLength]) values(@timeStamp,@customerGUID,@backupFile,@pNewFileName,@fileLength)";
                using (SqlConnection connection = new SqlConnection(SQLConnectionString))
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    long timeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                    command.Parameters.Add("@timeStamp", SqlDbType.BigInt).Value = timeStamp;
                    command.Parameters.Add("@fileLength", SqlDbType.BigInt).Value = fileLength;

                    command.Parameters.Add("@customerGUID", SqlDbType.VarChar).Value = itemKey;
                    command.Parameters.Add("@backupFile", SqlDbType.VarChar).Value = backupFileName;
                    command.Parameters.Add("@pNewFileName", SqlDbType.VarChar).Value = pNewFileName;


                    //command.Prepare();
                    command.BeginExecuteNonQuery();

                }
                // string jsonPopulated = JsonConvert.SerializeObject(agentConfig);
                //DBSingleTon.Instance.write2Log(customerGuid, "DEBUG", jsonPopulated);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, ex.ToString());
            }
            return Ok();

        }

        private static string? _connectionString = System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_OffSiteServiceBusConnection");// "Endpoint =sb://securitasmachina.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=IOC5nIXihyX3eKDzmvzzH20PdUnr/hyt3wydgtNe5z8=";
        private static string SQLConnectionString = System.Environment.GetEnvironmentVariable("SQLAZURECONNSTR_OffSiteBackupSQLConnection");

        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [Route("/api/v3/provisionUser/{customerGUID}")]
        public async Task<ActionResult> provisionUserAsync(string customerGUID)
        {
            

            try
            {
                string sql = @"INSERT INTO[dbo].[customers]
           ([customerID],name)
     VALUES
           (@customerID,'Your Name')";

                using (SqlConnection connection = new SqlConnection(SQLConnectionString))
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();

                    command.Parameters.Add("@customerID", SqlDbType.UniqueIdentifier).Value = new Guid(customerGUID);
                    command.ExecuteNonQuery();

                    // return Ok(ret);

                }
            }
            catch (Exception ex)
            {
                DBSingleTon.Instance.write2Log(customerGUID, "ERROR", ex.ToString());

                _logger.LogError(ex.ToString());
                //return StatusCode(StatusCodes.Status500InternalServerError, ex.ToString());
            }

           // _connectionString = System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_OffSiteServiceBusConnection");
            //var client = new ManagementClient(_connectionString);
            //ServiceBusClient client = new ServiceBusClient(_connectionString);
            try
            {
                ServiceBusAdministrationClient client = new ServiceBusAdministrationClient("SecuritasMachinaOffsiteClients.servicebus.windows.net", new DefaultAzureCredential());
                string subscriptionsName = "client";
                if (!await client.SubscriptionExistsAsync(customerGUID, subscriptionsName))
                {
                    await client.CreateSubscriptionAsync(customerGUID, subscriptionsName);
                }
            }
            catch (Exception ex)
            {
                DBSingleTon.Instance.write2Log(customerGUID, "ERROR", ex.ToString());
                _logger.LogError(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, ex.ToString());
            }
            return Ok();
        }

        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [Route("/api/v3/BackupHistory/{itemKey}")]
        public ActionResult<List<BackupHistoryDTO>> getBackupHistory(string itemKey)
        {
            List<BackupHistoryDTO> ret = new List<BackupHistoryDTO>();
            //string json = pFormBody.ToString();
            _logger.LogInformation("/apt/v3/BackupHistory");
            try
            {
                //_logger.LogInformation("looking up " + customerGuid);

                //string connectionString = System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_OffSiteServiceBusConnection");
                


                string sql = "select timeStamp2,backupFile,newFileName,fileLength from backupHistory where customerGUID=@customerGUID order by timeStamp2 desc";
                using (SqlConnection connection = new SqlConnection(SQLConnectionString))
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();

                    command.Parameters.Add("@customerGUID", SqlDbType.VarChar).Value = itemKey;
                    using SqlDataReader rdr = command.ExecuteReader();
                    while (rdr.Read())
                    {
                        BackupHistoryDTO logMsgDTO = new BackupHistoryDTO();
                        logMsgDTO.timeStamp = rdr.GetInt64(0);
                        logMsgDTO.backupFile = rdr.GetString(1);
                        if (!rdr.IsDBNull(2))
                            logMsgDTO.newFileName = rdr.GetString(2);
                        if (!rdr.IsDBNull(3))
                            logMsgDTO.fileLength = rdr.GetInt64(3);

                        ret.Add(logMsgDTO);
                        // Console.WriteLine($"{rdr.GetInt32(0)} {rdr.GetString(1)} {rdr.GetInt32(2)}");
                    }
                    return Ok(ret);

                }
                // string jsonPopulated = JsonConvert.SerializeObject(agentConfig);
                //DBSingleTon.Instance.write2Log(customerGuid, "DEBUG", jsonPopulated);
                return Ok(ret);
            }
            catch (Exception ex)
            {
                DBSingleTon.Instance.write2Log(itemKey, "ERROR", ex.ToString());
                _logger.LogError(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, ex.ToString());
            }
            return Ok();

        }

        [HttpGet]
        [Route("/apt/v1/testMyHealth")]
        public ActionResult<string> checkHealth()
        {

            //string json = pFormBody.ToString();
            try
            {

                _logger.LogInformation("/apt/v1/testMyHealth");
                try
                {
                    //_logger.LogInformation("looking up " + customerGuid);

                   // string connectionString = System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_OffSiteServiceBusConnection");
                    //string SQLConnectionString = System.Environment.GetEnvironmentVariable("SQLAZURECONNSTR_OffSiteBackupSQLConnection");



                    string sql = "select * from backupHistory where customerGUID='ab5fc41e-3814-4533-8f68-a691b4da9043";
                    using (SqlConnection connection = new SqlConnection(SQLConnectionString))
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {

                        connection.Open();


                        command.Prepare();
                        command.ExecuteReader();// .BeginExecuteNonQuery();

                    }
                    // string jsonPopulated = JsonConvert.SerializeObject(agentConfig);
                    //DBSingleTon.Instance.write2Log(customerGuid, "DEBUG", jsonPopulated);
                    return Ok();
                }
                catch (Exception ex)
                {
                    DBSingleTon.Instance.write2Log("ab5fc41e-3814-4533-8f68-a691b4da9043", "ERROR", ex.ToString());
                    _logger.LogError(ex.ToString());
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw new Exception(ex.Message);

            }
            return Ok();

        }
        [HttpPost]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes("application/json")]
        [Route("/api/v3/putLog/{customerGuid}/{messageType}")]
        public ActionResult<string> putLog([FromBody] object value, string customerGuid, string messageType)
        {
            DBSingleTon.Instance.write2Log(customerGuid, messageType, value.ToString());
            return "Log written";
        }
        [HttpPost]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes("application/json")]
        [Route("/api/v3/putLog/{customerGuid}")]
        public ActionResult<string> putLog([FromBody] object value, string customerGuid)
        {
            DBSingleTon.Instance.write2Log(customerGuid, "TRACE", value.ToString());
            return "Log written";
        }
        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]

        [Route("/api/v3/getLogs/{customerGuid}")]
        public ActionResult<List<LogMsgDTO>> GetLogAsync(string customerGuid)
        {
            return DBSingleTon.Instance.getLogs(customerGuid);

        }
        [HttpGet]
        [Route("/api/v3/getCache/{itemKey}")]
        public ActionResult<string> getCache(string itemKey)
        {
            string json = "";
            try
            {
                GenericMessage genericMessage = new GenericMessage();

                string stm = "select id,msg from mycache where id=@myId";
                SqliteCommand cmd2 = new SqliteCommand(stm, DBSingleTon.Instance.getCon());
                cmd2.Parameters.AddWithValue("@myId", Uri.EscapeUriString(itemKey));

                cmd2.Prepare();
                genericMessage.msgType = itemKey.Substring(0, itemKey.IndexOf("-"));
                genericMessage.guid = itemKey;
                SqliteDataReader myReader = cmd2.ExecuteReader();
                bool gotRead = false;
                while (myReader.Read())
                {
                    gotRead = true;
                    json = myReader.GetString(1);
                    genericMessage = JsonConvert.DeserializeObject<GenericMessage>(json);
                }

                if (!gotRead)
                {
                    return NotFound();
                }
                json = JsonConvert.SerializeObject(genericMessage);

            }
            catch (Exception ex)
            {
                DBSingleTon.Instance.write2Log(itemKey, "ERROR", ex.ToString());
                _logger.LogError(ex.ToString());
                throw new Exception(ex.Message);

            }
            return json;
        }
    }
}