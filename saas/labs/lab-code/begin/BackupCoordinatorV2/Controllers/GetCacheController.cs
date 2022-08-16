using BackupCoordinatorV2.Utils;
using Common.DTO.V2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using WebListener.Utils;

namespace BackupCoordinatorV2.Controllers
{
    [ApiController]

    public class GetCacheController : ControllerBase
    {


        private readonly ILogger<GetCacheController> _logger;

        public GetCacheController(ILogger<GetCacheController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("/v3/getCache/{itemKey}")]
        public string Get(string itemKey)
        {
            string json = "";
            try
            {
                GenericMessage genericMessage = new GenericMessage();
                
                string stm = "select id,msg from mycache where id='$myId'";
                SqliteCommand cmd2 = new SqliteCommand(stm, DBSIngleTon.Instance.getCon());
                SqliteParameter myParm1 = cmd2.CreateParameter();
                myParm1.ParameterName = "$myId";
                myParm1.Value = itemKey;
                SqliteDataReader myReader = cmd2.ExecuteReader();
                while (myReader.Read())
                {
                    json = myReader.GetString(1);
                    genericMessage.msg = json;
                }

                //cmd.ExecuteNonQuery();


                //SimpleMemoryCache simpleMemoryCache = new SimpleMemoryCache();
                
                //genericMessage = simpleMemoryCache.GetOrCreate(itemKey, null);
                if (genericMessage == null)
                {
                    genericMessage = new GenericMessage();
                    genericMessage.msgType = "dirListing";
                    genericMessage.msg = itemKey;
                }
                else
                {
                    genericMessage.guid = itemKey;
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