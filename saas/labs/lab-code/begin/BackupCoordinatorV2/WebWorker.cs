using Azure.Messaging.ServiceBus;
using Common.DTO.V2;
using Newtonsoft.Json;
using System.Data.SqlClient;
using WebListener.Models;

namespace WebListener
{
    public class WebWorker
    {
        string connectionString = System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_OffSiteServiceBusConnection");// "Endpoint =sb://securitasmachina.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=IOC5nIXihyX3eKDzmvzzH20PdUnr/hyt3wydgtNe5z8=";
        string SQLConnectionString = System.Environment.GetEnvironmentVariable("SQLAZURECONNSTR_OffSiteBackupSQLConnection"); //"Server=tcp:test-offsite-server.database.windows.net,1433;Initial Catalog=OffSiteBackup;Persist Security Info=False;User ID=appLogon;Password=wSuaA4Q0rtvHqaQ9MRS2;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        // name of your Service Bus queue
        string queueName = "offsitebackup";
        // the client that owns the connection and can be used to create senders and receivers
        ServiceBusClient client;

        // the sender used to publish messages to the queue
        ServiceBusSender sender;
        internal string getConfig(string customerGUID)
        {
            OffSiteMessageDTO _OffSiteMessageDTO = new OffSiteMessageDTO();
            using (SqlConnection connection = new SqlConnection(SQLConnectionString))
            using (SqlCommand command = new SqlCommand("select * from customers where customerId = @customerId", connection))
            {
                SqlParameter[] param = new SqlParameter[1];
                param[0] = new SqlParameter("@customerId", customerGUID);
                command.Parameters.Add(param[0]);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())

                    {
                        _OffSiteMessageDTO.passPhrase = reader["passPhrase"].ToString();


                    }
                }
            }
            string jsonPopulated = JsonConvert.SerializeObject(_OffSiteMessageDTO);
            return jsonPopulated;

        }
        internal async Task RecordBackupAsync(string json)
        {
            string topicEndPoint = "";
            dynamic stuff = JsonConvert.DeserializeObject(json);
            string? msgType = stuff.msgType;
            string? customerGUID = stuff.customerGUID;
            string? backupName = stuff.backupName;
            string? status = stuff.status;
            string? passPhrase = stuff.passPhrase;
            string? errorMsg = stuff.errormsg;

            OffSiteMessageDTO _OffSiteMessageDTO = new OffSiteMessageDTO();
            _OffSiteMessageDTO.customerGUID = customerGUID;
            _OffSiteMessageDTO.msgType = msgType;
            _OffSiteMessageDTO.backupName = backupName;
            _OffSiteMessageDTO.status = status;
            // System.Diagnostics.Trace.TraceError("If you're seeing this, something bad happened");
            using (SqlConnection connection = new SqlConnection(SQLConnectionString))
            using (SqlCommand command = new SqlCommand("select * from customers where customerId = @customerId", connection))
            {
                SqlParameter[] param = new SqlParameter[1];
                param[0] = new SqlParameter("@customerId", customerGUID);
                command.Parameters.Add(param[0]);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        _OffSiteMessageDTO.azureBlobEndpoint = reader["azureBlobEndpoint"].ToString();
                        _OffSiteMessageDTO.BlobContainerName = reader["azureContainerName"].ToString();
                        _OffSiteMessageDTO.passPhrase = reader["passPhrase"].ToString();
                        _OffSiteMessageDTO.RetentionDays = reader.GetInt16(reader.GetOrdinal("retentionDays"));
                        topicEndPoint = reader["topicEndPoint"].ToString();

                    }
                }
            }
            string jsonPopulated = JsonConvert.SerializeObject(_OffSiteMessageDTO);
            // create a batch 
            client = new ServiceBusClient(topicEndPoint);
            sender = client.CreateSender(customerGUID);
            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();


            // try adding a message to the batch
            if (!messageBatch.TryAddMessage(new ServiceBusMessage(jsonPopulated)))
            {
                // if it is too large for the batch
                throw new Exception($"The message is too large to fit in the batch.");
            }


            try
            {
                // Use the producer client to send the batch of messages to the Service Bus queue
                await sender.SendMessagesAsync(messageBatch);
                // System.Diagnostics.Trace.TraceInformation("wrote messageBatch");
                //await sender.CloseAsync();
                //Console.WriteLine($"A batch of {numOfMessages} messages has been published to the queue.");
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }
        }

        internal async Task RequestRestoreAsync(string json)
        {
           
            dynamic stuff = JsonConvert.DeserializeObject(json);
            string? msgType = stuff.msgType;
            string? customerGUID = stuff.customerGUID;
            string? backupName = stuff.backupName;
            string? status = stuff.status;
            string? passPhrase = stuff.passPhrase;
            string? errorMsg = stuff.errormsg;
            GenericMessage genericMessage = new GenericMessage();
            genericMessage.msgType = "restoreFile";
            genericMessage.msg = json;
            genericMessage.guid = customerGUID;
            /*using (SqlConnection connection = new SqlConnection(SQLConnectionString))
            using (SqlCommand command = new SqlCommand("select * from customers where customerId = @customerId", connection))
            {
                SqlParameter[] param = new SqlParameter[1];
                param[0] = new SqlParameter("@customerId", customerGUID);
                command.Parameters.Add(param[0]);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        _OffSiteMessageDTO.azureBlobEndpoint = reader["azureBlobEndpoint"].ToString();
                        _OffSiteMessageDTO.BlobContainerName = reader["azureContainerName"].ToString();
                        _OffSiteMessageDTO.passPhrase = reader["passPhrase"].ToString();
                        _OffSiteMessageDTO.RetentionDays = reader.GetInt16(reader.GetOrdinal("retentionDays"));
                        topicEndPoint = reader["topicEndPoint"].ToString();

                    }
                }
            }
            */
            string jsonPopulated = JsonConvert.SerializeObject(genericMessage);
            try
            {
                //string endpoint = "Endpoint=sb://securitasmachinaoffsiteclients.servicebus.windows.net/;SharedAccessKeyName=defaultSender;SharedAccessKey=vnoGGku4jwmkcYhn/+UNIKngq+CtbJaoZK0uPBMZzk8=;EntityPath=ab50c41e-3814-4533-8f68-a691b4da9043";
               // endpoint = "https://SecuritasMachinaOffsiteClients.servicebus.windows.net/ab50c41e-3814-4533-8f68-a691b4da9043";
                string endpoint= "Endpoint = sb://securitasmachinaoffsiteclients.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=z0RU2MtEivO9JGSwhwLkRb8P6fg6v7A9MET5tNuljbQ=";
                client = new ServiceBusClient(endpoint);
                sender = client.CreateSender(customerGUID);
                ServiceBusMessage message = new ServiceBusMessage(jsonPopulated);
                await sender.SendMessageAsync(message);
                //sender. .CloseAsync();
                System.Diagnostics.Trace.TraceInformation("wrote json:" + jsonPopulated + " to "+ customerGUID);
                //Thread.Sleep(15000);
                
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.TraceError("ex:" + ex.ToString());
            }
            finally
            {
                
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }
        }
    }
}
