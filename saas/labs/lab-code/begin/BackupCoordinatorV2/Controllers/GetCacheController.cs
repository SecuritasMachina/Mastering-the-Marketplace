using BackupCoordinatorV2.Utils;
using Common.DTO.V2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System.Net.Mime;
using System.Text;
using WebListener.Utils;

namespace BackupCoordinatorV2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GetCacheController : ControllerBase
    {


        private readonly ILogger<GetCacheController> _logger;

        public GetCacheController(ILogger<GetCacheController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes("application/json")]
        [Route("/api/v3/putCache/{itemKey}")]
        public ActionResult<string> PostAsync([FromBody] object value, string itemKey)
        {

            string json = value.ToString();
            try
            {

                _logger.LogInformation("/v3/putCache/" + itemKey + " " + json);
                try
                {
                    string stm = "UPDATE mycache set msg= @myJson where id =@myId";
                    SqliteCommand cmd2 = new SqliteCommand(stm, DBSIngleTon.Instance.getCon());
                    cmd2.Parameters.AddWithValue("@myId", itemKey);
                    cmd2.Parameters.AddWithValue("@myJson", json);
                    cmd2.Prepare();
                    cmd2.ExecuteNonQuery();
                }
                catch (Exception ex) { _logger.LogError(ex.ToString()); }
                try
                {
                    string stm = "INSERT INTO mycache(id, msg) VALUES(@myId, @myJson)";
                    SqliteCommand cmd2 = new SqliteCommand(stm, DBSIngleTon.Instance.getCon());
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

        [HttpGet]
        [Route("/api/v3/getCache/{itemKey}")]
        public string Get(string itemKey)
        {
            string json = "";
            try
            {
                GenericMessage genericMessage = new GenericMessage();

                string stm = "select id,msg from mycache where id=@myId";
                SqliteCommand cmd2 = new SqliteCommand(stm, DBSIngleTon.Instance.getCon());
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