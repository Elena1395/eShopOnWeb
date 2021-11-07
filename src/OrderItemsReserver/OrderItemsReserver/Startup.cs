using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

[assembly: FunctionsStartup(typeof(OrderItemsReserver.Startup))]
namespace OrderItemsReserver
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;
            var connectionString = configuration.GetSection("AzureWebJobsStorage").Value;

            builder.Services.AddSingleton<CloudStorageAccount>(x => CloudStorageAccount.Parse(connectionString));

            builder.Services.AddSingleton<CloudBlobClient>(CloudStorageAccount.Parse(connectionString).CreateCloudBlobClient());
        }
    }
}
