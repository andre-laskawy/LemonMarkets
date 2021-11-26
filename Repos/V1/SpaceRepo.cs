using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LemonMarkets.Interfaces;
using LemonMarkets.Models;
using LemonMarkets.Models.Enums;
using LemonMarkets.Models.Requests.Trading;
using LemonMarkets.Models.Responses;
using WsApiCore;

namespace LemonMarkets.Repos.V1
{

    public class SpaceRepo : ISpacesRepo
    {

        #region vars

        private readonly WsAPICore tradingApi;

        #endregion vars

        #region ctor

        public SpaceRepo(WsAPICore tradingApi)
        {
            this.tradingApi = tradingApi;
        }

        #endregion ctor

        #region methods

        public Task<LemonResult<Space>?> CreateAsync ( RequestSpace request )
        {
            return this.tradingApi.PostAsync<RequestSpace, LemonResult<Space>> (request, "spaces");
        }

        public Task<LemonResult<Space>?> UpdateAsync ( string id, RequestSpace request )
        {
            return this.tradingApi.PutAsync<RequestSpace, LemonResult<Space>> (request, $"spaces/{id}");
        }

        public Task<LemonResults<Space>?> GetAsync ( SpaceSearchFilter? request = null )
        {
            if (request == null) return this.tradingApi.GetAsync<LemonResults<Space>>("spaces");

            List<string> param = new List<string>();

            if (request.Type != SpaceType.None) param.Add($"type={request.Type.ToString().ToLower()}");

            if (param.Count == 0) return this.tradingApi.GetAsync<LemonResults<Space>>("spaces");

            StringBuilder buildParams = new ();
            buildParams.Append("?");
            buildParams.AppendJoin("&", param);

            return this.tradingApi.GetAsync<LemonResults<Space>>("spaces", buildParams);
        }

        public Task<LemonResult<Space>?> GetAsync ( string id )
        {
            return this.tradingApi.GetAsync<LemonResult<Space>>("spaces", id);
        }

        public Task<LemonResult?> DeleteAsync ( string id )
        {
            return this.tradingApi.DeleteAsync<LemonResult>("spaces", id);
        }

        #endregion methods

    }

}