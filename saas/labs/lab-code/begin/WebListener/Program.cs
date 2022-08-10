using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebListener.Models;
using System.Text;
using WebListener;

var builder = WebApplication.CreateBuilder(args);



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
        string json = await reader.ReadToEndAsync();
        try
        {
            
            Console.WriteLine("recordBackup");
            System.Diagnostics.Trace.TraceWarning("!! /v2/recordBackup !!");
            //Post to service bus
            await new WebWorker().RecordBackupAsync(json);

            
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
            //throw;// Console.WriteLine(ex.Message);
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

            Console.WriteLine("requestRestore");
            System.Diagnostics.Trace.TraceWarning("!! /v2/requestRestore !!");
            //Post to service bus
            await new WebWorker().RequestRestoreAsync(json);

            
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
            //throw;// Console.WriteLine(ex.Message);
        }
        return json;
    }
});

app.Run();
