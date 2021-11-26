using LemonMarkets.Models;
using LemonMarkets.Models.Requests.Trading;
using LemonMarkets.Models.Responses;
using System.Threading.Tasks;

namespace LemonMarkets.Interfaces
{
    public interface IOrdersRepo
    {

        LemonResult Activate(RequestActivateOrder request);

        Task<LemonResult?> ActivateAsync(RequestActivateOrder request);

        LemonResult<Order> Create(RequestCreateOrder request);

        Task<LemonResult<Order>?> CreateAsync(RequestCreateOrder request);

        Task<LemonResults<Order>?> GetAsync(OrderSearchFilter? request = null);

        Task<LemonResult<Order>?> GetAsync(string id);

        Task<LemonResult?> DeleteAsync(string id);

    }
}
