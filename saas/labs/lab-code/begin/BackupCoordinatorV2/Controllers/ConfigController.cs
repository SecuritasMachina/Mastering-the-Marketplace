using BackupCoordinatorV2.Utils;
using Common.DTO.V2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
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
        [Route("/api/v2/config/{customerGuid}")]
        public string Get(string customerGuid)
        {
            string json = "";
            _logger.LogInformation("looking up " + customerGuid);

            string connectionString = System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_OffSiteServiceBusConnection");
            string SQLConnectionString = System.Environment.GetEnvironmentVariable("SQLAZURECONNSTR_OffSiteBackupSQLConnection");


            AgentConfig agentConfig = new AgentConfig();
            using (SqlConnection connection = new SqlConnection(SQLConnectionString))
            using (SqlCommand command = new SqlCommand("select * from customers where customerId = @customerId", connection))
            {
                SqlParameter[] param = new SqlParameter[1];
                param[0] = new SqlParameter("@customerId", customerGuid);
                command.Parameters.Add(param[0]);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())

                    {
                        agentConfig.passPhrase = reader["passPhrase"].ToString();
                        agentConfig.ServiceBusEndPoint = "Endpoint=sb://securitasmachinaoffsiteclients.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=z0RU2MtEivO9JGSwhwLkRb8P6fg6v7A9MET5tNuljbQ=";
                        agentConfig.topicName = "controller";


                    }
                }
            }
            string jsonPopulated = JsonConvert.SerializeObject(agentConfig);
            return jsonPopulated;


        }
    }
}