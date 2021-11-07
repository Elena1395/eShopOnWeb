using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Microsoft.eShopWeb.ApplicationCore.Interfaces
{
    public interface IOrderService
    {
        Task<bool> CreateOrderAsyncAndTriggerFunction(int basketId, Address shippingAddress, IConfiguration configuration);
    }
}
