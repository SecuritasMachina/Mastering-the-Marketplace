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


var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                          //policy.WithOrigins("*", "http://localhost:4200", "https://localhost:44424",
                          //                    "https://securitasmachinacoordinater.azurewebsites.net/");
                      });
});
// Add services to the container.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddControllersWithViews();

var app = builder.Build();
app.UseCors(MyAllowSpecificOrigins);
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
var cmd = new SqliteCommand(stm, DBSingleTon.Instance.getCon());
string version = cmd.ExecuteScalar().ToString();

app.Logger.LogInformation("SELECT SQLITE_VERSION: " + version);
stm = @"CREATE TABLE mycache(id TEXT PRIMARY KEY,
            msg TEXT,timeEntered REAL,source TEXT)";
cmd = new SqliteCommand(stm, DBSingleTon.Instance.getCon());
cmd.ExecuteNonQuery();
stm = @"CREATE TABLE mylog(id TEXT, logType TEXT, logTime REAL,
            msg TEXT, source TEXT)";
cmd = new SqliteCommand(stm, DBSingleTon.Instance.getCon());
cmd.ExecuteNonQuery();


app.Run();
