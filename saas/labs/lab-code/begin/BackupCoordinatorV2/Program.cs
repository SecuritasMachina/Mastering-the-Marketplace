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
//builder.Services.AddControllers().AddJsonOptions();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("*", "http://localhost:4200",
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
app.MapControllers();
//app.MapHttpAttributeRoutes();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");




string stm = "SELECT SQLITE_VERSION()";
var cmd = new SqliteCommand(stm, DBSIngleTon.Instance.getCon());
string version = cmd.ExecuteScalar().ToString();

app.Logger.LogInformation("SELECT SQLITE_VERSION: " + version);
stm = @"CREATE TABLE mycache(id TEXT PRIMARY KEY,
            msg TEXT)";
cmd = new SqliteCommand(stm, DBSIngleTon.Instance.getCon());
cmd.ExecuteNonQuery();

app.MapGet("/v2/config/{customerGuid}", async delegate (HttpContext context, string customerGuid)
{
    app.Logger.LogInformation("looking up " + customerGuid);
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


app.Run();
