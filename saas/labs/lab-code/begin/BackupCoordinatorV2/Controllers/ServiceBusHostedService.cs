using Azure.Identity;
using Azure.Messaging.ServiceBus;
using BackupCoordinatorV2.Models;
using BackupCoordinatorV2.Utils;
using Common.DTO.V2;
using Newtonsoft.Json;
using System.Text;

namespace BackupCoordinatorV2.Controllers
{
    public class ServiceBusHostedService : IHostedService
    {
        private readonly ILogger logger;
        private readonly IConfiguration configuration;
        // private QueueClient queueClient;
        ServiceBusClient client;

        // the processor that reads and processes messages from the queue
        ServiceBusProcessor processor;
        public ServiceBusHostedService(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<ServiceBusHostedService>();
            this.configuration = configuration;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            DefaultAzureCredential credential = new DefaultAzureCredential();
            //"Endpoint=sb://securitasmachinaoffsiteclients.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=z0RU2MtEivO9JGSwhwLkRb8P6fg6v7A9MET5tNuljbQ="
            client = new ServiceBusClient("Endpoint=sb://securitasmachinaoffsiteclients.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=z0RU2MtEivO9JGSwhwLkRb8P6fg6v7A9MET5tNuljbQ=");
            //client = new ServiceBusClient("securitasmachinaoffsiteclients.servicebus.windows.net", credential);
            logger.LogDebug($"BusListenerService starting; registering message handler.");
            processor = client.CreateProcessor("coordinator", new ServiceBusProcessorOptions());

            processor.ProcessMessageAsync += MessageHandler;

            // add handler to process any errors
            processor.ProcessErrorAsync += ErrorHandler;

            // start processing 
            processor.StartProcessingAsync();

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogDebug($"BusListenerService stopping.");
            //await this.queueClient.CloseAsync();
        }
        static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            // logger.LogDebug($"BusListenerService starting; registering message handler.");
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
        protected void ProcessError(Exception e)
        {
            logger.LogError(e, "Error while processing queue item in BusListenerService.");
        }
        static async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();

            try
            {
                GenericMessage genericMessage = JsonConvert.DeserializeObject<GenericMessage>(body);
                string nameSpace = genericMessage.nameSpace;
                string msgType = genericMessage.msgType;
                string guid = genericMessage.guid;
                bool isJson = false;

                if (nameSpace.Equals("agent/logs"))
                {
                    //ClientController clientController = new ClientController();
                    DBSingleTon.Instance.write2Log(guid, msgType, genericMessage.msg);
                }
                else if (nameSpace.Equals("agent/status"))
                {
                    // StatusDTO statusDTO = new StatusDTO();
                    DBSingleTon.Instance.putCache("STATUS" + "-" + guid, JsonConvert.SerializeObject(genericMessage));
                    StatusDTO statucDTO = JsonConvert.DeserializeObject<StatusDTO>(genericMessage.msg);
                    genericMessage = new GenericMessage();
                    genericMessage.guid = guid;
                    genericMessage.nameSpace = nameSpace;
                    genericMessage.msgType = "dirListing";
                    genericMessage.msg = JsonConvert.SerializeObject(statucDTO.AgentFileDTOs);
                    DBSingleTon.Instance.write2SQLLog(guid, "DIRLIST", JsonConvert.SerializeObject(genericMessage));
                    DBSingleTon.Instance.putCache(genericMessage.msgType + "-" + guid, JsonConvert.SerializeObject(genericMessage));
                    /*
                    genericMessage = new GenericMessage();
                    genericMessage.guid = guid;
                    genericMessage.nameSpace = nameSpace;
                    genericMessage.msgType = "stagingListing";
                    genericMessage.msg = JsonConvert.SerializeObject(statucDTO.StagingFileDTOs);

                    DBSingleTon.Instance.putCache(genericMessage.msgType + "-" + guid, JsonConvert.SerializeObject(genericMessage));
                    */
                }
                else if (nameSpace.Equals("agent/putCache"))
                {
                    DBSingleTon.Instance.putCache(msgType, genericMessage.msg);
                }
                else if (nameSpace.Equals("agent/backupHistory"))
                {
                    BackupHistoryDTO backupHistoryDTO = JsonConvert.DeserializeObject<BackupHistoryDTO>(genericMessage.msg);
                    DBSingleTon.Instance.postBackup(guid, backupHistoryDTO.backupFile, backupHistoryDTO.newFileName, backupHistoryDTO.fileLength, backupHistoryDTO.startTimeStamp);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                GenericMessage genericMessage2 = new GenericMessage();

                genericMessage2.msgType = "restoreComplete";
                genericMessage2.msg = ex.Message.ToString();
            }
            // complete the message. message is deleted from the queue. 
            await args.CompleteMessageAsync(args.Message);
        }

    }
}
