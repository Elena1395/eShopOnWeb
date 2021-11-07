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
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos;
using OrderItemsReserver.Models;

namespace OrderItemsReserver
{
    public class AddOrderToCosmos
    {
        private const string databaseId = "eShopDatabase";
        private const string containerId = "OrderContainer";

        [FunctionName("AddOrderToCosmos")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var order = JsonConvert.DeserializeObject<Order>(requestBody);

            CalculateTotalPrice(order);

            var cosmosClient = GetCosmosClient(context);
            var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            var container = await database.Database.CreateContainerIfNotExistsAsync(containerId, "/id");

            var cosmosResponse = await container.Container.CreateItemAsync(order, new PartitionKey(order.Id));

            return cosmosResponse.StatusCode==HttpStatusCode.Created ? new OkResult() : (IActionResult)new BadRequestResult();
        }

        private static CosmosClient GetCosmosClient(ExecutionContext executionContext)
        {
            var config = new ConfigurationBuilder()
                            .SetBasePath(executionContext.FunctionAppDirectory)
                            .AddJsonFile("local.settings.json", true, true)
                            .AddEnvironmentVariables().Build();

            CosmosClient cosmosClient = new CosmosClient(config["EndpointUri"], config["PrimaryKey"]);
            return cosmosClient;
        }

        private static void CalculateTotalPrice(Order order)
        {
            double finalPrice = 0;
            foreach (var item in order.OrderItems) {
                finalPrice += item.UnitPrice * item.Units;
            }
            order.FinalPrice = finalPrice;
        }
    }
}
