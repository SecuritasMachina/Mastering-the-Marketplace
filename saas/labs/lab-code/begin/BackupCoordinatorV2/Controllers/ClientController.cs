using BackupCoordinatorV2.Utils;
using Common.DTO.V2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;


using System.Net.Mime;
using WebListener;
using Microsoft.Azure.ServiceBus.Management;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Azure.Identity;
using Azure.Core;
using MySql.Data.MySqlClient;
using BackupCoordinatorV2.Models;

namespace BackupCoordinatorV2.Controllers
{


    [ApiController]
    [Route("[controller]")]
    public class ClientController : ControllerBase
    {
        private static string? _connectionString = System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_OffSiteServiceBusConnection");// "Endpoint =sb://securitasmachina.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=IOC5nIXihyX3eKDzmvzzH20PdUnr/hyt3wydgtNe5z8=";

        private readonly ILogger<ClientController> _logger;

        public ClientController(ILogger<ClientController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Consumes("application/json")]
        [Produces(MediaTypeNames.Application.Json)]
        [Route("/api/v3/agentDir/{customerGUID}")]
        public async Task<ActionResult> agentDir([FromBody] DirListingDTO pDirListingDTO, string customerGUID)
        {

            PutCache(JsonConvert.SerializeObject(pDirListingDTO), "AgentStagingList-" + customerGUID);
            //dynamic stuff = JsonConvert.DeserializeObject(pFormBody.ToString());
            //DBSingleTon.Instance.write2Log(customerGUID, "INFO", pFormBody.ToString());
            return Ok();
        }
        [HttpGet]
        [Consumes("application/json")]
        [Produces(MediaTypeNames.Application.Json)]
        [Route("/api/v3/agentDir/{customerGUID}")]
        public async Task<ActionResult> agentDirGet(string customerGUID)
        {

            return Ok(getCache("AgentStagingList-" + customerGUID));
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


            _logger.LogInformation("/v3/putCache/" + itemKey + " " + json);
            DBSingleTon.Instance.putCache(itemKey, json);


            return Ok();

        }
        //https://localhost:7074/api/v3/postBackupHistory/ab50c41e-3814-4533-8f68-a691b4da9043/Charles+Havranek+Resume+-+LongD9.pdf/Charles+Havranek+Resume+-+LongD9.pdf/147024
        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        //[Consumes("application/json")]
        [Route("/api/v3/postBackupHistory/{itemKey}/{backupFileName}/{pNewFileName}/{fileLength}/{startTimeStamp}")]
        public ActionResult<string> postBackupHistory(string itemKey, string backupFileName, string pNewFileName, long fileLength, long startTimeStamp)
        {

            //string json = pFormBody.ToString();
            _logger.LogInformation("/apt/v1/postBackupHistory");
            DBSingleTon.Instance.postBackup(itemKey, backupFileName, pNewFileName, fileLength, startTimeStamp);
            try
            {
                DBSingleTon.Instance.postBackup(itemKey, backupFileName, pNewFileName, fileLength, startTimeStamp);


                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                DBSingleTon.Instance.write2Log(itemKey, "ERROR", ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, ex.ToString());
            }
            //return Ok();

        }



        [HttpPost]
        [Consumes("application/json")]
        [Produces(MediaTypeNames.Application.Json)]
        [Route("/api/v3/provisionUser")]
        public async Task<ActionResult> provisionUserAsync([FromBody] object pFormBody)
        {
            Guid newGuid = Guid.NewGuid();

            //DBSingleTon.Instance.write2Log(newGuid.ToString(), "INFO", pFormBody.ToString());
            DBSingleTon.Instance.write2SQLLog(newGuid.ToString(), "INFO", pFormBody.ToString());
            dynamic stuff = JsonConvert.DeserializeObject(pFormBody.ToString());

            string contactName = stuff.DisplayName == null ? " stuff.DisplayName" : stuff.DisplayName;
            string contactEmail = stuff.Email == null ? "" : stuff.Email;
            string SubscriptionName = stuff.SubscriptionName == null ? " stuff.SubscriptionName" : stuff.SubscriptionName;
            string FulfillmentStatus = stuff.FulfillmentStatus == null ? " stuff.FulfillmentStatus" : stuff.FulfillmentStatus;
            string SubscriptionId = stuff.SubscriptionId == null ? " stuff.SubscriptionId" : stuff.SubscriptionId;
            string TenantId = stuff.TenantId == null ? " stuff.TenantId" : stuff.TenantId;
            string PurchaseIdToken = stuff.PurchaseIdToken == null ? " stuff.PurchaseIdToken" : stuff.PurchaseIdToken;
            if (contactEmail.Equals(""))
            {
                DBSingleTon.Instance.write2SQLLog(newGuid.ToString(), "ERROR", "provisionUser Missing valid email");
                return StatusCode(StatusCodes.Status500InternalServerError, "Missing valid email");
            }

            bool foundCustomer = false;
            string sql = "select customerID from customers where contactEmail=@contactEmail";
            using (MySqlConnection connection = new MySqlConnection(DBSingleTon._SQLConnectionString))
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                connection.Open();

                command.Parameters.Add("@contactEmail", MySqlDbType.VarChar).Value = contactEmail;
                using MySqlDataReader rdr = command.ExecuteReader();
                while (rdr.Read())
                {
                    newGuid = new Guid(rdr.GetString(0));
                    foundCustomer = true;
                }
            }

            if (!foundCustomer)
            {
                try
                {
                    sql = @"INSERT INTO customers
           (customerID,contactName,contactEmail,SubscriptionName,FulfillmentStatus,SubscriptionId,TenantId,PurchaseIdToken)
     VALUES
           (@customerID,@contactName,@contactEmail,@SubscriptionName,@FulfillmentStatus,@SubscriptionId,@TenantId,@PurchaseIdToken)";

                    using (MySqlConnection connection = new MySqlConnection(DBSingleTon._SQLConnectionString))
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        connection.Open();

                        command.Parameters.Add("@customerID", MySqlDbType.VarString).Value = newGuid.ToString();
                        command.Parameters.Add("@contactName", MySqlDbType.VarString).Value = contactName;
                        command.Parameters.Add("@contactEmail", MySqlDbType.VarString).Value = contactEmail;
                        command.Parameters.Add("@SubscriptionName", MySqlDbType.VarString).Value = SubscriptionName;
                        command.Parameters.Add("@FulfillmentStatus", MySqlDbType.VarString).Value = FulfillmentStatus;
                        command.Parameters.Add("@SubscriptionId", MySqlDbType.VarString).Value = SubscriptionId;
                        command.Parameters.Add("@TenantId", MySqlDbType.VarString).Value = TenantId;
                        command.Parameters.Add("@PurchaseIdToken", MySqlDbType.VarString).Value = PurchaseIdToken;
                        command.ExecuteNonQuery();

                        // return Ok(ret);

                    }
                }
                catch (Exception ex)
                {
                    DBSingleTon.Instance.write2Log(newGuid.ToString(), "ERROR", ex.ToString());

                    _logger.LogError(ex.ToString());
                    return StatusCode(StatusCodes.Status500InternalServerError, ex.ToString());
                }


                try
                {
                    if (false)//permission denied
                    {
                        string userAssignedClientId = "f160bcf9-1269-447e-8976-25dde3c6e6e0";
                        DefaultAzureCredential credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = userAssignedClientId });
                        TokenCredential credential2 = new ClientSecretCredential("db2361da-c4d2-4472-85dc-00969813cbe0", "e837d735-07e0-4adc-9336-80344215b102", "HrS8Q~5RqBHy5G6PE_F9MTXSPnlOfdDypKqO-dbA");
                        ServiceBusAdministrationClientOptions options = new ServiceBusAdministrationClientOptions();
                        ServiceBusAdministrationClient client = new ServiceBusAdministrationClient("SecuritasMachinaOffsiteClients.servicebus.windows.net", credential2, options);
                        string subscriptionsName = "client";

                        await client.CreateSubscriptionAsync(newGuid.ToString(), subscriptionsName);
                    }
                    if (false)
                    {
                        // TokenCredential tk = new TokenCredential();
                        string userAssignedClientId = "e837d735-07e0-4adc-9336-80344215b102";
                        //DefaultAzureCredential credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = userAssignedClientId });
                        // TokenRequestContext requestContext = default;

                        TokenCredential credential2 = new DefaultAzureCredential();// new ClientSecretCredential("db2361da-c4d2-4472-85dc-00969813cbe0", "e837d735-07e0-4adc-9336-80344215b102", "HrS8Q~5RqBHy5G6PE_F9MTXSPnlOfdDypKqO-dbA");
                                                                                   // var client2 = new ServiceBusClient("SecuritasMachinaOffsiteClients.servicebus.windows.net", credential2);
                                                                                   //AccessToken tk = credential.GetToken(requestContext);
                        AccessToken accessToken;
                        //value: HrS8Q~5RqBHy5G6PE_F9MTXSPnlOfdDypKqO-dbA
                        //Secret: 82b86ae3-eb2a-47fd-8f3b-152e1b50c5a6
                        //e837d735-07e0-4adc-9336-80344215b102
                        //var _managedCredential = new ManagedIdentityCredential("e837d735-07e0-4adc-9336-80344215b102");
                        // accessToken = await credential.GetTokenAsync(
                        //                         new TokenRequestContext(
                        //                              new[] { "https://servicebus.azure.net" })).ConfigureAwait(false);
                        ServiceBusAdministrationClientOptions options = new ServiceBusAdministrationClientOptions();
                        //accessToken.
                        //    ServiceBusAdministrationClient client = new ServiceBusAdministrationClient(credential,"Endpoint=sb://securitasmachinaoffsiteclients.servicebus.windows.net/;SharedAccessKeyName=ProvisioningPolicy;SharedAccessKey=rR1lVUGwoWh+avYBsRmC+AWgnKNcIa9/gUAJ9vf0iHI=;EntityPath=ab50c41e-3814-4533-8f68-a691b4da9043");
                        ServiceBusAdministrationClient client = new ServiceBusAdministrationClient("SecuritasMachinaOffsiteClients.servicebus.windows.net", credential2, options);
                        string subscriptionsName = "client";

                        await client.CreateSubscriptionAsync(newGuid.ToString(), subscriptionsName);
                    }
                }
                catch (Exception ex)
                {
                    DBSingleTon.Instance.write2Log(newGuid.ToString(), "ERROR", ex.ToString());
                    _logger.LogError(ex.ToString());
                    return StatusCode(StatusCodes.Status500InternalServerError, ex.ToString());
                }
            }
            return Ok(newGuid.ToString());
        }

        //
        //

        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [Route("/api/v3/PerfHistory/{itemKey}")]
        public ActionResult<ReportDTO> getPerfHistory(string itemKey)
        {
            ReportDTO ret = new ReportDTO();
            ret.offSiteFileReportItems = new List<ReportItemDTO>();
            ret.dirListFileReportItems = new List<ReportItemDTO>();
            ret.backupItemsFileReportItems = new List<ReportItemDTO>();
            //string json = pFormBody.ToString();
            _logger.LogInformation("/apt/v3/PerfHistory");
            try
            {

                string sql = "SELECT DATE_FORMAT(date(dateEntered),'%Y-%m-%d'),count(msgType) FROM offsite.CustomerLogs where customerguid=@customerGUID and  msgtype='BACKUP-END' group by day(dateEntered) order by dateEntered asc";
                using (MySqlConnection connection = new MySqlConnection(DBSingleTon._SQLConnectionString))
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    connection.Open();

                    command.Parameters.Add("@customerGUID", MySqlDbType.VarChar).Value = itemKey;
                    using MySqlDataReader rdr = command.ExecuteReader();
                    while (rdr.Read())
                    {
                        ReportItemDTO logMsgDTO = new ReportItemDTO();
                        if (!rdr.IsDBNull(0))
                            logMsgDTO.myDate = rdr.GetString(0);
                        if (!rdr.IsDBNull(1))
                            logMsgDTO.myCount = rdr.GetInt64(1);

                        ret.backupItemsFileReportItems.Add(logMsgDTO);

                    }
                }
                ret.totalRestores=DBSingleTon.Instance.getField(itemKey,"SELECT count(1) FROM offsite.CustomerLogs where customerguid=@customerGUID and  msgtype='RESTORE-END' ");
                ret.lastDateEnteredTimestamp = DBSingleTon.Instance.getField(itemKey, "SELECT max(dateEnteredtimestamp) FROM offsite.CustomerLogs where customerguid=@customerGUID and  msgtype='DIRLIST'");

                sql = "SELECT DATE_FORMAT(date(dateEntered),'%Y-%m-%d'),count(msgType) FROM offsite.CustomerLogs where customerguid=@customerGUID and msgtype='DIRLIST' group by day(dateEntered) order by dateEntered asc";
                using (MySqlConnection connection = new MySqlConnection(DBSingleTon._SQLConnectionString))
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    connection.Open();

                    command.Parameters.Add("@customerGUID", MySqlDbType.VarChar).Value = itemKey;
                    using MySqlDataReader rdr = command.ExecuteReader();
                    while (rdr.Read())
                    {
                        ReportItemDTO logMsgDTO = new ReportItemDTO();
                        if (!rdr.IsDBNull(0))
                            logMsgDTO.myDate = rdr.GetString(0);
                        if (!rdr.IsDBNull(1))
                            logMsgDTO.myCount = rdr.GetInt64(1);

                        ret.dirListFileReportItems.Add(logMsgDTO);

                    }
                }

            }
            catch (Exception ex)
            {
                DBSingleTon.Instance.write2Log(itemKey, "ERROR", ex.ToString());
                _logger.LogError(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, ex.ToString());
            }
            return Ok(ret);
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

                string sql = "select starttimeStamp,backupFile,newFileName,fileLength,endTimeStamp from backupHistory where customerGUID=@customerGUID order by starttimeStamp desc";
                using (MySqlConnection connection = new MySqlConnection(DBSingleTon._SQLConnectionString))
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    connection.Open();

                    command.Parameters.Add("@customerGUID", MySqlDbType.VarChar).Value = itemKey;
                    using MySqlDataReader rdr = command.ExecuteReader();
                    while (rdr.Read())
                    {
                        BackupHistoryDTO logMsgDTO = new BackupHistoryDTO();
                        if (!rdr.IsDBNull(0))
                            logMsgDTO.startTimeStamp = rdr.GetInt64(0);
                        if (!rdr.IsDBNull(1))
                            logMsgDTO.backupFile = rdr.GetString(1);
                        if (!rdr.IsDBNull(2))
                            logMsgDTO.newFileName = rdr.GetString(2);
                        if (!rdr.IsDBNull(3))
                            logMsgDTO.fileLength = rdr.GetInt64(3);
                        if (!rdr.IsDBNull(4))
                            logMsgDTO.endTimeStamp = rdr.GetInt64(4);


                        ret.Add(logMsgDTO);

                    }
                }


            }
            catch (Exception ex)
            {
                DBSingleTon.Instance.write2Log(itemKey, "ERROR", ex.ToString());
                _logger.LogError(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, ex.ToString());
            }
            return Ok(ret);

        }

        [HttpGet]
        [Route("/apt/v1/testMyHealth")]
        public ActionResult<string> checkHealth()
        {


            _logger.LogInformation("/apt/v1/testMyHealth");
            try
            {
                string sql = "select * from offsite.customers where customerid='ab5fc41e-3814-4533-8f68-a691b4da9043'";
                using (MySqlConnection connection = new MySqlConnection(DBSingleTon._SQLConnectionString))
                using (MySqlCommand command = new MySqlCommand(sql, connection))
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
                DBSingleTon.Instance.write2Log("HealthCheck", "ERROR", ex.ToString());
                _logger.LogError(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }


        }



        [HttpPost]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes("application/json")]
        [Route("/api/v3/putLog/{customerGuid}/{messageType}")]
        public ActionResult<string> putLog([FromBody] object value, string customerGuid, string messageType)
        {
            DBSingleTon.Instance.write2Log(customerGuid, messageType, value.ToString());
            return Ok();
        }
        [HttpPost]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes("application/json")]
        [Route("/api/v3/putLog/{customerGuid}")]
        public ActionResult<string> putLog([FromBody] object value, string customerGuid)
        {
            DBSingleTon.Instance.write2Log(customerGuid, "TRACE", value.ToString());
            return Ok();
        }
        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]

        [Route("/api/v3/getLogs/{customerGuid}")]
        public ActionResult<List<LogMsgDTO>> GetLogAsync(string customerGuid)
        {
            return Ok(DBSingleTon.Instance.getLogs(customerGuid));

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
                cmd2.Parameters.AddWithValue("@myId", itemKey);

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
            return Ok(json);
        }
    }

}