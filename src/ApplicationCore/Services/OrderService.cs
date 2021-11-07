using Ardalis.GuardClauses;
using Azure.Messaging.ServiceBus;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.ApplicationCore.Services
{
    public class OrderService : IOrderService
    {
        private readonly IAsyncRepository<Order> _orderRepository;
        private readonly IUriComposer _uriComposer;
        private readonly IAsyncRepository<Basket> _basketRepository;
        private readonly IAsyncRepository<CatalogItem> _itemRepository;

        public OrderService(IAsyncRepository<Basket> basketRepository,
            IAsyncRepository<CatalogItem> itemRepository,
            IAsyncRepository<Order> orderRepository,
            IUriComposer uriComposer)
        {
            _orderRepository = orderRepository;
            _uriComposer = uriComposer;
            _basketRepository = basketRepository;
            _itemRepository = itemRepository;
        }

        public async Task<bool> CreateOrderAsyncAndTriggerFunction(int basketId, Address shippingAddress, IConfiguration configuration)
        {
            var basketSpec = new BasketWithItemsSpecification(basketId);
            var basket = await _basketRepository.FirstOrDefaultAsync(basketSpec);

            Guard.Against.NullBasket(basketId, basket);
            Guard.Against.EmptyBasketOnCheckout(basket.Items);

            var catalogItemsSpecification = new CatalogItemsSpecification(basket.Items.Select(item => item.CatalogItemId).ToArray());
            var catalogItems = await _itemRepository.ListAsync(catalogItemsSpecification);

            var items = basket.Items.Select(basketItem =>
            {
                var catalogItem = catalogItems.First(c => c.Id == basketItem.CatalogItemId);
                var itemOrdered = new CatalogItemOrdered(catalogItem.Id, catalogItem.Name, _uriComposer.ComposePicUri(catalogItem.PictureUri));
                var orderItem = new OrderItem(itemOrdered, basketItem.UnitPrice, basketItem.Quantity);
                return orderItem;
            }).ToList();

            var order = new Order(basket.BuyerId, shippingAddress, items);

            await _orderRepository.AddAsync(order);
            //_configuration.GetSection("FuncUrl").Value, _configuration.GetSection("FuncKey").Value
            //return PutOrderToblobStorage(order, configuration.GetSection("BlobFuncUrl").Value, configuration.GetSection("BlobFuncKey").Value);
            //return PutOrderToCosmosStorage(order, configuration.GetSection("CosmosFuncUrl").Value, configuration.GetSection("CosmosFuncKey").Value);
            //PutOrderToblobStorage(order, configuration.GetSection("BlobFuncUrl").Value, configuration.GetSection("BlobFuncKey").Value);
            //PutOrderToblobStorage(order, configuration.GetSection("BlobFuncUrl").Value, configuration.GetSection("BlobFuncKey").Value);
            PutOrderToCosmosStorage(order, configuration.GetSection("CosmosFuncUrl").Value, configuration.GetSection("CosmosFuncKey").Value);
            await PutOrderToQueue(order, configuration.GetSection("ServiceBusCon").Value, configuration.GetSection("QueueName").Value);
            return true;
        }

        private async Task PutOrderToQueue(Order order, string connectionString, string queueName) {
            var client = new ServiceBusClient(connectionString);
            var sender = client.CreateSender(queueName);
            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

            string json = JsonSerializer.Serialize(order);
            messageBatch.TryAddMessage(new ServiceBusMessage(json));

            try
            {
                // Use the producer client to send the batch of messages to the Service Bus queue
                await sender.SendMessagesAsync(messageBatch);
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }
        }

        private bool PutOrderToCosmosStorage(Order order, string funcUrl, string funcKey)
        {

            var httpWebRequest = (HttpWebRequest)WebRequest.Create($"{funcUrl}?code={funcKey}");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = JsonSerializer.Serialize(order);
                streamWriter.Write(json);
            }
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            if (httpResponse.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            return false;
        }
        private bool PutOrderToblobStorage(Order order, string funcUrl, string funcKey) {

            var httpWebRequest = (HttpWebRequest)WebRequest.Create($"{funcUrl}?code={funcKey}");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json =  JsonSerializer.Serialize(order);
                streamWriter.Write(json);
            }
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            if (httpResponse.StatusCode == HttpStatusCode.OK) {
                return true;
            }
            return false;
        }
    }
}
