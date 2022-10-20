using System.Dynamic;
using System.Globalization;
using LemonMarkets.Extensions;
using LemonMarkets.Models.Enums;

namespace LemonMarkets
{
    using IO.Ably;
    using LemonMarkets.Helper;
    using LemonMarkets.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security.Authentication;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;

    /// <summary>
    /// Defines the <see cref="LemonApi" />.
    /// </summary>
    public class LemonApiFactory
    {
        private static string streamingBaseUrl = "https://realtime.lemon.markets/v1/";

        public static async Task Init(string bearerToken, bool throwExceptions = true, bool withStreaming = false)
        {
            try
            {
                RestHelper.Init(bearerToken);
                LemonApi.ThrowErrors = throwExceptions;

                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                var httpClientHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true,
                    SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12
                };

                if (withStreaming) await InitStreaming();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static async Task InitStreaming()
        {
            try
            {
                var response = await $"{streamingBaseUrl}/auth".MakeRequest();
                LemonApi.UserId = JObject.Parse(response).GetValue<string>("user_id");
                var authtoken = JObject.Parse(response).GetValue<string>("token");

                LemonApi.StreamingClient = new AblyRealtime(new ClientOptions()
                {
                    Token = authtoken,
                    TransportParams = new Dictionary<string, object> { { "remainPresentFor", 1000 } },
                });
            }
            catch
            {
                throw;
            }
        }

        public static LemonApi Create()
        {
            return new LemonApi();
        }
    }
}
