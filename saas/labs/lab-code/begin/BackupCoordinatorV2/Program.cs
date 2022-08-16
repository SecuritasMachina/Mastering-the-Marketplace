using Azure.Messaging.ServiceBus;
using BackupCoordinatorV2.Utils;
using Common.DTO.V2;
using Common.Statics;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System.Text;
using WebListener;
using WebListener.Utils;
//using System.Data.SqlClient;
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<SimpleMemoryCache>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("*","http://localhost:4200",
                                              "https://securitasmachinacoordinater.azurewebsites.net/");
                      });
});
// Add services to the container.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");




string stm = "SELECT SQLITE_VERSION()";
var cmd = new SqliteCommand(stm, DBSIngleTon.Instance.getCon());
//cmd.CommandText = "DROP TABLE IF EXISTS cars";
string version = cmd.ExecuteScalar().ToString();
//cmd.ExecuteNonQuery();
app.Logger.LogInformation("SELECT SQLITE_VERSION: " + version);
stm = @"CREATE TABLE mycache(id TEXT PRIMARY KEY,
            msg TEXT)"; 
 cmd = new SqliteCommand(stm, DBSIngleTon.Instance.getCon());
cmd.ExecuteNonQuery();

app.MapGet("/v2/config/{customerGuid}", async delegate (HttpContext context, string customerGuid)
{
    app.Logger.LogInformation("looking up "+ customerGuid);
    string json = "";
    string connectionString = System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_OffSiteServiceBusConnection");
    string SQLConnectionString = System.Environment.GetEnvironmentVariable("SQLAZURECONNSTR_OffSiteBackupSQLConnection");

    try
    {
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

    catch (Exception ex)
    {
        throw new Exception(ex.Message);

    }
    return json;

});
app.MapGet("/v2/getCache/{msgID}", async delegate (HttpContext context, string msgID)
{

    string json = "";
    app.Logger.LogInformation("looking up msgID: " + msgID);
    try
    {
        SimpleMemoryCache simpleMemoryCache = new SimpleMemoryCache();
        GenericMessage genericMessage = new GenericMessage();
        genericMessage = simpleMemoryCache.GetOrCreate(msgID, null);
        if (genericMessage == null)
        {
            genericMessage = new GenericMessage();
            genericMessage.msgType = "dirListing";
            genericMessage.msg = msgID;
        }
        else
        {
            genericMessage.guid = msgID;
        }

        json = JsonConvert.SerializeObject(genericMessage);
        return json;
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex.ToString());
        throw new Exception(ex.Message);

    }
    //return json;

});
app.MapPost("/v2/recordBackup", async delegate (HttpContext context)
{
    using (StreamReader reader = new StreamReader(context.Request.Body, Encoding.UTF8))
    {
        string json = await reader.ReadToEndAsync();
        try
        {

           // logger.LogInformation("/v2/recordBackup:" + json);

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

           // logger.LogInformation("/v2/requestRestore:" + json);
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
RunTimeSettings.SBConnectionString = "Endpoint=sb://securitasmachinaoffsiteclients.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=z0RU2MtEivO9JGSwhwLkRb8P6fg6v7A9MET5tNuljbQ=";
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
    if (string.Equals(msgType, "dirlisting", StringComparison.OrdinalIgnoreCase))
    {
        //DirListingDTO dirListingDTO = JsonConvert.DeserializeObject<DirListingDTO>(stuff.msg);
    }

    //cmd.CommandText = "DROP TABLE IF EXISTS cars";
    //string version = cmd.ExecuteScalar().ToString();
    //cmd.ExecuteNonQuery();
    //app.Logger.LogInformation("SELECT SQLITE_VERSION: " + version);
    
    
    string stm = "INSERT INTO mycache(id, msg) VALUES($myId, $myJson)";
    SqliteCommand cmd2 = new SqliteCommand(stm, DBSIngleTon.Instance.getCon());
    SqliteParameter myParm1 = cmd2.CreateParameter();
    myParm1.ParameterName = "$myId";
    myParm1.Value = "dirlisting-" + guid;
    SqliteParameter myParm2 = cmd2.CreateParameter();
    myParm2.ParameterName = "$myJson";
    myParm2.Value = stuff.msg;
    cmd2.ExecuteNonQuery();
    /*SimpleMemoryCache simpleMemoryCache = new SimpleMemoryCache();
    GenericMessage genericMessage = new GenericMessage();
    genericMessage.msgType = "dirlisting";
    genericMessage.msg = stuff.msg;
    simpleMemoryCache.GetOrCreate("dirlisting-" + guid, genericMessage);
    */
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
