using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using BackupCoordinator;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
// connection string to your Service Bus namespace

new ListenerWorker().startAsync();
