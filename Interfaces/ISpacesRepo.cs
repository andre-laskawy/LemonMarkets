using LemonMarkets.Models;
using LemonMarkets.Models.Requests.Trading;
using LemonMarkets.Models.Responses;
using System.Threading.Tasks;

namespace LemonMarkets.Interfaces
{
    public interface ISpacesRepo
    {

        Task<LemonResult<Space>?> CreateAsync(RequestSpace request);

        Task<LemonResult<Space>?> UpdateAsync(string id, RequestSpace request);

        Task<LemonResults<Space>?> GetAsync(SpaceSearchFilter? request = null);

        Task<LemonResult<Space>?> GetAsync(string id);

        Task<LemonResult?> DeleteAsync(string id);

    }
}
