using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebListener.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
 string connectionString = "Endpoint=sb://securitasmachina.servicebus.windows.net/;SharedAccessKeyName=sbpolicy1;SharedAccessKey=hGQMBNMvG1djKydyi1hCJmtDJN/mgtegm/9rAaDMEGg=;EntityPath=offsitebackup";

// name of your Service Bus queue
 string queueName = "offsitebackup";

// the client that owns the connection and can be used to create senders and receivers
 ServiceBusClient client;

// the sender used to publish messages to the queue
 ServiceBusSender sender;
// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapPost("/v2/recordBackup", async delegate (HttpContext context)
{
    using (StreamReader reader = new StreamReader(context.Request.Body, Encoding.UTF8))
    {
       // string queryKey = context.Request.RouteValues["queryKey"].ToString();
        string json = await reader.ReadToEndAsync();
        Console.WriteLine("recordBackup");
        //Post to service bus

        try
        {
            client = new ServiceBusClient(connectionString);
            sender = client.CreateSender(queueName);
            //OffSiteMessageDTO offSiteMessageDTO = new OffSiteMessageDTO();
            //offSiteMessageDTO.customerGuid = customerGuid;
            //offSiteMessageDTO.backupName = backupName;
            string jsonString = JsonSerializer.Serialize(json);
            //var json = new JavaScriptSerializer().Serialize(new { property = "string" })
            // create a batch 
            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();


            // try adding a message to the batch
            if (!messageBatch.TryAddMessage(new ServiceBusMessage(json)))
            {
                // if it is too large for the batch
                throw new Exception($"The message is too large to fit in the batch.");
            }


            try
            {
                // Use the producer client to send the batch of messages to the Service Bus queue
                await sender.SendMessagesAsync(messageBatch);
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
        catch (Exception ex) { Console.WriteLine(ex.Message); }
        return json;
    }
});

app.MapPost("/recordBackup", async ([FromBody] String json) =>
{
    Console.WriteLine("recordBackup");
    //Post to service bus

    try
    {
        client = new ServiceBusClient(connectionString);
        sender = client.CreateSender(queueName);
        //OffSiteMessageDTO offSiteMessageDTO = new OffSiteMessageDTO();
        //offSiteMessageDTO.customerGuid = customerGuid;
        //offSiteMessageDTO.backupName = backupName;
        string jsonString = JsonSerializer.Serialize(json);
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
    catch(Exception ex) { Console.WriteLine(ex.Message); }
    return Results.NoContent();
});
app.Run();
