using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Extensions.Configuration;

namespace OrderItemsReserver
{
    public class AddOrderToStorage
    {
        private static CloudBlobClient client;
        public AddOrderToStorage(CloudBlobClient test)
        {
            client = test;
        }

        [FunctionName("AddOrderToStorage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            return new BadRequestResult();
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject(requestBody);

            CloudStorageAccount storageAccount = GetCloudStorageAccount(context);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("orders");


            ////var client = storage.CreateCloudBlobClient();
            //CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=eshoporderstorage;AccountKey=RWLGErnUXQTS0A1t0bnTZFFmIZCNz+gwt5trif2kLOJBqKCw3ltlb+LembavTrmrNbcncuEyA1orJjyxEXhGZQ==;BlobEndpoint=https://eshoporderstorage.blob.core.windows.net/;TableEndpoint=https://eshoporderstorage.table.core.windows.net/;QueueEndpoint=https://eshoporderstorage.queue.core.windows.net/;FileEndpoint=https://eshoporderstorage.file.core.windows.net/");
            //// Create the blob client.
            //CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            //client = blobClient;
            //var container = client.GetContainerReference("orders");
            //await container.CreateIfNotExistsAsync();
            //var blob = container.GetBlockBlobReference($"order_{Guid.NewGuid()}.json");
            //await blob.UploadFromStreamAsync(req.Body);
            var blob2 = container.GetBlockBlobReference($"order_{Guid.NewGuid()}.json");
            await blob2.UploadTextAsync(requestBody);
            log.LogInformation("AddOrderToStorage - 200");
            return new OkResult();
        }

        private static CloudStorageAccount GetCloudStorageAccount(ExecutionContext executionContext)
        {
            var config = new ConfigurationBuilder()
                            .SetBasePath(executionContext.FunctionAppDirectory)
                            .AddJsonFile("local.settings.json", true, true)
                            .AddEnvironmentVariables().Build();

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(config["AzureWebJobsStorage"]);
            return storageAccount;
        }
    }
}
