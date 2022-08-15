using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebListener.Models;
using System.Text;
using WebListener;
using Common.Statics;
using Newtonsoft.Json;
using Common.DTO.V2;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();
ILogger logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();

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
        string json = await reader.ReadToEndAsync();
        try
        {

            logger.LogInformation("/v2/recordBackup:" + json);

            //System.Diagnostics.Trace.TraceWarning("!! /v2/recordBackup !!");
            //Post to service bus for particular client
            await new WebWorker().RecordBackupAsync(json);


        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);

        }
        return json;
    }
});
app.MapPost("/v2/requestRestore", async delegate (HttpContext context)
{
    using (StreamReader reader = new StreamReader(context.Request.Body, Encoding.UTF8))
    {
        string json = await reader.ReadToEndAsync();
        try
        {

            logger.LogInformation("/v2/requestRestore:" + json);
            //System.Diagnostics.Trace.TraceWarning("!! /v2/requestRestore !!");
            //Post to service bus
            await new WebWorker().RequestRestoreAsync(json);


        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);

        }
        return json;
    }
});

// connection string to your Service Bus namespace
//string connectionString = "<NAMESPACE CONNECTION STRING>";

// name of the Service Bus topic
string topicName = "controller";

// name of the subscription to the topic
string subscriptionName = "client";

// the client that owns the connection and can be used to create senders and receivers
ServiceBusClient client;

// the processor that reads and processes messages from the subscription
ServiceBusProcessor processor;
client = new ServiceBusClient(RunTimeSettings.SBConnectionString);

// create a processor that we can use to process the messages
processor = client.CreateProcessor(topicName, subscriptionName, new ServiceBusProcessorOptions());

try
{
    // add handler to process messages
    processor.ProcessMessageAsync += MessageHandler;

    // add handler to process any errors
    processor.ProcessErrorAsync += ErrorHandler;

    // start processing 
    await processor.StartProcessingAsync();

    logger.LogInformation("Wait for a minute and then press any key to end the processing");
    // Console.ReadKey();

    // stop processing 
    // Console.WriteLine("\nStopping the receiver...");
    // await processor.StopProcessingAsync();
    //Console.WriteLine("Stopped receiving messages");
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}
finally
{
    // Calling DisposeAsync on client types is required to ensure that network
    // resources and other unmanaged objects are properly cleaned up.
    // await processor.DisposeAsync();
    // await client.DisposeAsync();
}
// handle received messages
static async Task MessageHandler(ProcessMessageEventArgs args)
{
    string body = args.Message.Body.ToString();
    Console.WriteLine(body);
    dynamic stuff = JsonConvert.DeserializeObject(body);
    string msgType = stuff.msgType;
    string guid = stuff.guid;
    if (string.Equals(msgType, "dirlisting", StringComparison.OrdinalIgnoreCase) )
    {
        DirListingDTO dirListingDTO = JsonConvert.DeserializeObject<DirListingDTO>(stuff.msg);
    }
    //logger.LogInformation(body);

    // complete the message. messages is deleted from the subscription. 
    await args.CompleteMessageAsync(args.Message);
}

// handle any errors when receiving messages
static Task ErrorHandler(ProcessErrorEventArgs args)
{
    Console.WriteLine(args.Exception.ToString());
    return Task.CompletedTask;
}

app.Run();
