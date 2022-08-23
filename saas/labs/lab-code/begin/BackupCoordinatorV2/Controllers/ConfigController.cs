using BackupCoordinatorV2.Utils;
using Common.DTO.V2;
using Common.Utils.Comm;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Net.Mime;
using System.Text;
using WebListener.Utils;

namespace BackupCoordinatorV2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConfigController : ControllerBase
    {


        private readonly ILogger<ConfigController> _logger;

        public ConfigController(ILogger<ConfigController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [Route("/api/v2/config/{customerGuid}")]
        public AgentConfig Get(string customerGuid)
        {
            _logger.LogInformation("looking up " + customerGuid);


            AgentConfig agentConfig = GetAgentConfig(customerGuid);

           // string jsonPopulated = JsonConvert.SerializeObject(agentConfig);
            DBSingleTon.Instance.write2Log(customerGuid, "DEBUG", "AgentConfig Get");
            return agentConfig;


        }
        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [Route("/api/v2/customerConfig/{customerGuid}")]
        public AgentConfig customerConfig(string customerGuid)
        {
            _logger.LogInformation("customerConfig up " + customerGuid);


            AgentConfig ret = GetAgentConfig(customerGuid);
            ret.serviceBusEndPoint = null;

            return ret;


        }
        public AgentConfig GetAgentConfig(string customerGuid)
        {
            AgentConfig ret = new AgentConfig();
            if (customerGuid.Equals("null") || customerGuid.Length < 4)
            {
                ret.name = "Missing Customer GUID";
                return ret;
            }
            _logger.LogInformation("looking up " + customerGuid);

            using (MySqlConnection connection = new MySqlConnection(DBSingleTon._SQLConnectionString))
            using (MySqlCommand command = new MySqlCommand("select * from customers where customerId = @customerId", connection))
            {

                command.Parameters.Add("@customerId", MySqlDbType.VarChar).Value = customerGuid;
                connection.Open();
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ret.passPhrase = reader["passPhrase"].ToString();
                        ret.name = reader["contactname"].ToString();
                        ret.contactEmail = reader["contactEmail"].ToString();
                        ret.serviceBusEndPoint = "Endpoint=sb://securitasmachinaoffsiteclients.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=z0RU2MtEivO9JGSwhwLkRb8P6fg6v7A9MET5tNuljbQ=";
                        ret.controllerTopicName = "controller";
                        ret.clientSubscriptionName = "client";
                        ret.authKey = Utils.Utils.EncryptString(customerGuid, customerGuid + ":" + customerGuid + customerGuid);

                    }
                }
            }
            string jsonString = JsonConvert.SerializeObject(ret);
            Console.WriteLine(jsonString);
            return ret;
        }
    }
}