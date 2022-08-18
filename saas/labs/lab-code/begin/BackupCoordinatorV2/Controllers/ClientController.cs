using BackupCoordinatorV2.Utils;
using Common.DTO.V2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
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
                await new WebWorker().RecordBackupAsync(value.ToString());


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
        [Route("/api/v2/requestRestore/{itemKey}")]
        public async Task<ActionResult<string>> RequestRestoreAsync([FromBody] object value, string itemKey)
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


        [HttpPost]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes("application/json")]
        [Route("/api/v3/putCache/{itemKey}")]
        public ActionResult<string> PutCache([FromBody] object value, string itemKey)
        {

            string json = value.ToString();
            try
            {

                _logger.LogInformation("/v3/putCache/" + itemKey + " " + json);
                try
                {
                    string stm = "UPDATE mycache set msg= @myJson where id =@myId";
                    SqliteCommand cmd2 = new SqliteCommand(stm, DBSingleTon.Instance.getCon());
                    cmd2.Parameters.AddWithValue("@myId", itemKey);
                    cmd2.Parameters.AddWithValue("@myJson", json);
                    cmd2.Prepare();
                    cmd2.ExecuteNonQuery();
                }
                catch (Exception ex) { _logger.LogError(ex.ToString()); }
                try
                {
                    string stm = "INSERT INTO mycache(id, msg) VALUES(@myId, @myJson)";
                    SqliteCommand cmd2 = new SqliteCommand(stm, DBSingleTon.Instance.getCon());
                    cmd2.Parameters.AddWithValue("@myId", itemKey);
                    cmd2.Parameters.AddWithValue("@myJson", json);
                    cmd2.Prepare();
                    cmd2.ExecuteNonQuery();
                }
                catch (Exception ex) { _logger.LogError(ex.ToString()); }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw new Exception(ex.Message);

            }
            return json;

        }
        [HttpPost]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes("application/json")]
        [Route("/api/v3/putLog/{customerGuid}")]
        public ActionResult<string> putLog([FromBody] object value, string customerGuid)
        {
            DBSingleTon.Instance.write2Log(customerGuid, value.ToString());
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
        public string getCache(string itemKey)
        {
            string json = "";
            try
            {
                GenericMessage genericMessage = new GenericMessage();

                string stm = "select id,msg from mycache where id=@myId";
                SqliteCommand cmd2 = new SqliteCommand(stm, DBSingleTon.Instance.getCon());
                cmd2.Parameters.AddWithValue("@myId", itemKey);

                cmd2.Prepare();
                genericMessage.msgType = itemKey.Substring(0, itemKey.IndexOf("-"));
                genericMessage.guid = itemKey;
                SqliteDataReader myReader = cmd2.ExecuteReader();
                while (myReader.Read())
                {
                    json = myReader.GetString(1);
                    genericMessage = JsonConvert.DeserializeObject<GenericMessage>(json);
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