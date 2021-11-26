using LemonMarkets.Models.Requests.Trading;
using LemonMarkets.Models.Responses;
using System.Threading.Tasks;

namespace LemonMarkets.Interfaces
{
    public interface IOrdersRepo
    {

        LemonResult Activate(RequestActivateOrder request);

        Task<LemonResult> ActivateAsync(RequestActivateOrder request);



    }
}
