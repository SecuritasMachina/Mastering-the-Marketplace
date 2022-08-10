using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebListener.Models;

namespace WebListener.Controllers
{
    public class BackupListener : Controller
    {
        // connection string to your Service Bus namespace
        static string connectionString = "Endpoint=sb://securitasmachina.servicebus.windows.net/;SharedAccessKeyName=sbpolicy1;SharedAccessKey=hGQMBNMvG1djKydyi1hCJmtDJN/mgtegm/9rAaDMEGg=;EntityPath=offsitebackup";

        // name of your Service Bus queue
        static string queueName = "offsitebackup";

        // the client that owns the connection and can be used to create senders and receivers
        static ServiceBusClient client;

        // the sender used to publish messages to the queue
        static ServiceBusSender sender;

        // number of messages to be sent to the queue
        private const int numOfMessages = 3;

        // GET: HomeController1
        public ActionResult Index()
        {
            return View();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RecordBackupAsync(String customerGuid,String backupName)
        {
            //Post to service bus

            try
            {
                client = new ServiceBusClient(connectionString);
                sender = client.CreateSender(queueName);
                OffSiteMessageDTO offSiteMessageDTO = new OffSiteMessageDTO();
                offSiteMessageDTO.customerGUID  = customerGuid;
                offSiteMessageDTO.backupName = backupName;
                string jsonString = JsonSerializer.Serialize(offSiteMessageDTO);
                //var json = new JavaScriptSerializer().Serialize(new { property = "string" })
                // create a batch 
                using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

                
                    // try adding a message to the batch
                    if (!messageBatch.TryAddMessage(new ServiceBusMessage(jsonString)))
                    {
                        // if it is too large for the batch
                        throw new Exception($"The message is too large to fit in the batch.");
                    }
                

                try
                {
                    // Use the producer client to send the batch of messages to the Service Bus queue
                    await sender.SendMessagesAsync(messageBatch);
                    Console.WriteLine($"A batch of {numOfMessages} messages has been published to the queue.");
                }
                finally
                {
                    // Calling DisposeAsync on client types is required to ensure that network
                    // resources and other unmanaged objects are properly cleaned up.
                    await sender.DisposeAsync();
                    await client.DisposeAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: HomeController1/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: HomeController1/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
