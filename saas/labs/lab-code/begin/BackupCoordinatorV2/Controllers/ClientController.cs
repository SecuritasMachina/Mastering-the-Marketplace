using BackupCoordinatorV2.Utils;
using Common.DTO.V2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mime;
using WebListener;

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

                    string connectionString = System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_OffSiteServiceBusConnection");
                    string SQLConnectionString = System.Environment.GetEnvironmentVariable("SQLAZURECONNSTR_OffSiteBackupSQLConnection");



                    string sql = "select * from backupHistory where customerGUID='1'";
                    using (SqlConnection connection = new SqlConnection(SQLConnectionString))
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {

                        connection.Open();
                        long timeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();

                        command.Prepare();
                        command.ExecuteReader();// .BeginExecuteNonQuery();

                    }
                    // string jsonPopulated = JsonConvert.SerializeObject(agentConfig);
                    //DBSingleTon.Instance.write2Log(customerGuid, "DEBUG", jsonPopulated);
                    return Ok();
                }
                catch (Exception ex)
                {
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
                _logger.LogError(ex.ToString());
                throw new Exception(ex.Message);

            }
            return json;
        }
    }
}