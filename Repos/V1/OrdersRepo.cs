using LemonMarkets.Interfaces;
using LemonMarkets.Models;
using LemonMarkets.Models.Enums;
using LemonMarkets.Models.Requests.Trading;
using LemonMarkets.Models.Responses;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WsApiCore;

namespace LemonMarkets.Repos.V1
{

    public class OrdersRepo : IOrdersRepo
    {

        #region vars

        private readonly WsAPICore tradingApi;

        #endregion vars

        #region ctor

        public OrdersRepo(WsAPICore tradingApi)
        {
            this.tradingApi = tradingApi;
        }

        #endregion ctor

        #region methods

        public LemonResult Activate(RequestActivateOrder request)
        {
            string route = $"orders/{request.OrderId}/activate";

            LemonResult? response = this.tradingApi.PostData<RequestActivateOrder, LemonResult>(request, route);
            if (response == null) return new LemonResult<Order>("response is null");

            return response;
        }

        public Task<LemonResult?> ActivateAsync(RequestActivateOrder request)
        {
            string route = $"orders/{request.OrderId}/activate";

            return this.tradingApi.PostAsync<RequestActivateOrder, LemonResult>(request, route);
        }

        public LemonResult<Order> Create(RequestCreateOrder request)
        {
            LemonResult<Order>? response = this.tradingApi.PostData<RequestCreateOrder, LemonResult<Order>>(request, "orders");
            if (response == null) return new LemonResult<Order>("response is null");

            return response;
        }

        public Task<LemonResults<Order>?> GetAsync(OrderSearchFilter? request = null)
        {
            if (request == null) return this.tradingApi.GetAsync<LemonResults<Order>>("orders");

            List<string> param = new List<string>();

            if (request.From != null) param.Add($"from={request.From}");
            if (request.To != null) param.Add($"to={request.To}");
            if (request.Isin != null) param.Add($"isin={request.Isin}");
            if (request.SpaceUuid != null) param.Add($"space_id={request.SpaceUuid}");
            if (request.Side != OrderSide.All) param.Add($"side={request.Side.ToString().ToLower()}");
            if (request.Type != OrderType.All) param.Add($"type={request.Type.ToString().ToLower()}");
            if (request.Status != OrderStatus.All) param.Add($"type={request.Status.ToString().ToLower()}");

            if (param.Count == 0) return this.tradingApi.GetAsync<LemonResults<Order>>("orders");

            StringBuilder buildParams = new StringBuilder();
            buildParams.Append("?");
            buildParams.AppendJoin("&", param);

            return this.tradingApi.GetAsync<LemonResults<Order>>("orders", buildParams);
        }

        public Task<LemonResult<Order>?> GetAsync(string id)
        {
            return this.tradingApi.GetAsync<LemonResult<Order>>("orders", id);
        }

        public Task<LemonResult<Order>?> CreateAsync(RequestCreateOrder request)
        {
            return this.tradingApi.PostAsync<RequestCreateOrder, LemonResult<Order>>(request, "orders");
        }

        public Task<LemonResult?> DeleteAsync(string id)
        {
            return this.tradingApi.DeleteAsync<LemonResult>("orders", id);
        }

        #endregion methods
    }

}
