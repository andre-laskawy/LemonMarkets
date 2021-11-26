using System;
using System.Text;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;

namespace WsApiCore
{
    public class WsAPICore
    {

        #region vars

        private Uri baseadress;

        private Func<HttpRequestMessage, System.Security.Cryptography.X509Certificates.X509Certificate2, System.Security.Cryptography.X509Certificates.X509Chain, System.Net.Security.SslPolicyErrors, bool>? certificateCheck;
        private string? version;
        private HttpClient client;

        private event CheckZertifkat? checkCertEasy;

        private event HTTPStatuscode? httpCode;

        private event WsApiEvent? beforeConnectToWebservice;

        #endregion vars

        #region delegate

        public delegate bool CheckZertifkat(string hostname, System.Security.Cryptography.X509Certificates.X509Certificate2 x509Certificate2, System.Security.Cryptography.X509Certificates.X509Chain x509Chain);

        public delegate bool HTTPStatuscode(HttpStatusCode code);

        public delegate bool WsApiEvent(WsAPICore api);

        #endregion delegate

        #region get/set

        public event CheckZertifkat CheckCertEasy
        {
            add
            {
                this.checkCertEasy += value;
            }
            remove
            {
                this.checkCertEasy -= value;
            }
        }

        public event HTTPStatuscode HttpCode
        {
            add
            {
                this.httpCode += value;
            }
            remove
            {
                this.httpCode -= value;
            }
        }

        public event WsApiEvent BeforeConnectToWebservice
        {
            add
            {
                this.beforeConnectToWebservice += value;
            }
            remove
            {
                this.beforeConnectToWebservice -= value;
            }
        }

        public string BaseAdress
        {
            get
            {
                return this.baseadress.ToString();
            }
            set
            {
                this.baseadress = new Uri(value);

                this.client = this.BuildHttpClient();
            }
        }

        public string ApiPath
        {
            get;
            set;
        }

        public Func<HttpRequestMessage, System.Security.Cryptography.X509Certificates.X509Certificate2, System.Security.Cryptography.X509Certificates.X509Chain, System.Net.Security.SslPolicyErrors, bool>? CheckCertificate
        {
            get
            {
                return this.certificateCheck;
            }
            set
            {
                this.certificateCheck = value;

                this.client = this.BuildHttpClient();
            }
        }

        private AuthenticationHeaderValue? Authorization
        {
            get;
            set;
        }

        public string Version
        {
            get
            {
                return this.version;
            }
            set
            {
                this.version = value;

                this.client = this.BuildHttpClient();
            }
        }

        #endregion get/set

        #region ctor

        public WsAPICore(string baseAdress, string apiPath = "")
        {
            this.baseadress = new Uri(baseAdress);
            this.ApiPath = apiPath;

            this.client = this.BuildHttpClient();
        }

        ~WsAPICore()
        {
            this.client.Dispose();
        }

        #endregion

        #region methods

        public bool SetNewAuth(byte[] token, byte[] hashpassword, string mode = "Basic")
        {
            return this.SetNewAuth(Convert.ToBase64String(token), Convert.ToBase64String(hashpassword), mode);
        }

        public bool SetNewAuth(string username, string password, string mode = "Login")
        {
            byte[] byteArray = Encoding.ASCII.GetBytes(username + ":" + password);

            this.Authorization = new AuthenticationHeaderValue(mode, Convert.ToBase64String(byteArray));

            this.client.DefaultRequestHeaders.Authorization = this.Authorization;

            return true;
        }

        public bool SetNewAuth(string value, string mode = "Bearer")
        {
            this.Authorization = new AuthenticationHeaderValue(mode, value);

            this.client.DefaultRequestHeaders.Authorization = this.Authorization;

            return true;
        }

        public Task<HttpResponseMessage> SendManuelRequest(HttpRequestMessage request)
        {
            return this.client.SendAsync(request);
        }

        private bool CheckCert(HttpRequestMessage? httpRequestMessage, System.Security.Cryptography.X509Certificates.X509Certificate2 x509Certificate2, System.Security.Cryptography.X509Certificates.X509Chain x509Chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            if (this.checkCertEasy == null) return false;

            string host = string.Empty;

            if ( httpRequestMessage != null )
            {
                if ( httpRequestMessage.RequestUri != null ) host = httpRequestMessage.RequestUri.Host;
            }

            return this.checkCertEasy(host, x509Certificate2, x509Chain);
        }

        private HttpClientHandler BuildHandler()
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler();

            httpClientHandler.ServerCertificateCustomValidationCallback = this.CheckCert;

            if (this.CheckCertificate == null) return httpClientHandler;

            httpClientHandler.ServerCertificateCustomValidationCallback = this.CheckCertificate;

            return httpClientHandler;
        }

        private HttpClient BuildHttpClient()
        {
            HttpClientHandler handler = this.BuildHandler();

            HttpClient client = new HttpClient(handler);

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (this.Authorization != null) client.DefaultRequestHeaders.Authorization = this.Authorization;

            client.BaseAddress = this.baseadress;

            if (this.version != null) client.DefaultRequestHeaders.Add("X-Version", this.version);

            return client;
        }

        private string GetQuerryFromParams(params object[] header)
        {
            string query = this.ApiPath;

            foreach (object elem in header)
            {
                query += $"/{elem.ToString()}";
            }

            return query;
        }

        #region PutData
        
        
        public async Task<B?> PutAsync<T, B> ( T data, string route )
        {
            string apiPath = $"{this.ApiPath}/{route}";

            if (this.beforeConnectToWebservice != null) if (!this.beforeConnectToWebservice(this)) return default(B);

            HttpResponseMessage httpResponse = await this.client.PostAsJsonAsync<T>(apiPath, data);

            if (this.httpCode != null) this.httpCode(httpResponse.StatusCode);

            if (!httpResponse.IsSuccessStatusCode) return default(B);

            return await httpResponse.Content.ReadFromJsonAsync<B>();
        }

        public T PutData<T>(T data) where T : IModelElement
        {
            return this.PutDataFromQuery<T>($"{ApiPath}/{data.Id}", data);
        }

        public T PutData<T>(T data, params object[] header)
        {
            string query = this.GetQuerryFromParams(header);

            return this.PutDataFromQuery<T>(query, data);
        }

        private T PutDataFromQuery<T>(string query, T data)
        {
            try
            {
                if (this.beforeConnectToWebservice != null) if (!this.beforeConnectToWebservice(this)) return default(T);

                HttpResponseMessage httpResponse = this.client.PutAsJsonAsync<T>(query, data).Result;

                if (this.httpCode != null) this.httpCode(httpResponse.StatusCode);

                if (!httpResponse.IsSuccessStatusCode) return default(T);

                return httpResponse.Content.ReadFromJsonAsync<T>().Result;
            }
            catch
            {
                return default(T);
            }
        }

        public B PutData<T, B>(T data, params object[] header)
        {
            string query = this.GetQuerryFromParams(header);

            return this.PutDataFromQuery<T, B>(query, data);
        }

        public B PutData<T, B>(T data)
        {
            return this.PutDataFromQuery<T, B>($"{ApiPath}", data);
        }

        private B PutDataFromQuery<T, B>(string query, T data)
        {
            try
            {
                if (this.beforeConnectToWebservice != null) if (!this.beforeConnectToWebservice(this)) return default(B);

                HttpResponseMessage httpResponse = this.client.PutAsJsonAsync<T>(query, data).Result;

                if (this.httpCode != null) this.httpCode(httpResponse.StatusCode);

                if (!httpResponse.IsSuccessStatusCode) return default(B);

                return httpResponse.Content.ReadFromJsonAsync<B>().Result;
            }
            catch
            {
                return default(B);
            }
        }

        #endregion PutData

        #region PostData

        public B UploadFile<B>(string route, List<FileUploadRequest> files)
        {
            try
            {
                string query = $"{this.ApiPath}/{route}";

                if (this.beforeConnectToWebservice != null) if (!this.beforeConnectToWebservice(this)) return default(B);

                MultipartFormDataContent content = new MultipartFormDataContent();

                foreach (FileUploadRequest file in files)
                {
                    if (!file.File.Exists) continue;

                    StreamContent stream = new StreamContent(file.File.OpenRead());

                    content.Add(stream, file.Name, file.FileName);
                }

                HttpResponseMessage httpResponse = this.client.PostAsync(query, content).Result;

                if (this.httpCode != null) this.httpCode(httpResponse.StatusCode);

                if (!httpResponse.IsSuccessStatusCode) return default(B);

                return httpResponse.Content.ReadFromJsonAsync<B>().Result;
            }
            catch (Exception e)
            {
                return default(B);
            }
        }

        public B DownloadFile<T, B>(T data, string route) where B : IResponseFileDownload, new()
        {
            string query = $"{this.ApiPath}/{route}";

            try
            {
                if (this.beforeConnectToWebservice != null) if (!this.beforeConnectToWebservice(this)) return default(B);

                HttpResponseMessage httpResponse = this.client.PostAsJsonAsync<T>(query, data).Result;

                if (this.httpCode != null) this.httpCode(httpResponse.StatusCode);

                if (!httpResponse.IsSuccessStatusCode) return default(B);

                IEnumerable<string> values;

                httpResponse.Content.Headers.TryGetValues("Content-Type", out values);

                string value = values.FirstOrDefault();

                if (value != "APPLICATION/octet-stream") return httpResponse.Content.ReadFromJsonAsync<B>().Result;

                B result = new B();

                result.IsOk = true;
                result.Result = httpResponse.Content.ReadAsStreamAsync().Result;

                return result;
            }
            catch (Exception e)
            {
                return default(B);
            }
        }

        public bool PostData<T>(T data)
        {
            return this.PostDataFromQuery<T>(this.ApiPath, data);
        }

        public B PostData<T, B>(T data)
        {
            return this.PostDataFromQuery<T, B>(this.ApiPath, data);
        }
        public bool PostData<T>(T data, string route)
        {
            return this.PostDataFromQuery<T>($"{this.ApiPath}/{route}", data);
        }

        public B? PostData<T, B>(T data, string route)
        {
            return this.PostDataFromQuery<T, B>($"{this.ApiPath}/{route}", data);
        }

        public async Task<B?> PostAsync<T, B>(T data, string route)
        {
            string apiPath = $"{this.ApiPath}/{route}";

            try
            {
                if (this.beforeConnectToWebservice != null) if (!this.beforeConnectToWebservice(this)) return default(B);

                HttpResponseMessage httpResponse = await this.client.PostAsJsonAsync<T>(apiPath, data);

                if (this.httpCode != null) this.httpCode(httpResponse.StatusCode);

                if (!httpResponse.IsSuccessStatusCode) return default(B);

                return await httpResponse.Content.ReadFromJsonAsync<B>();
            }
            catch (Exception e)
            {
                return default(B);
            }
        }

        private B PostDataFromQuery<T, B>(string query, T data)
        {
            try
            {
                if (this.beforeConnectToWebservice != null) if (!this.beforeConnectToWebservice(this)) return default(B);

                HttpResponseMessage httpResponse = this.client.PostAsJsonAsync<T>(query, data).Result;

                if (this.httpCode != null) this.httpCode(httpResponse.StatusCode);

                if (!httpResponse.IsSuccessStatusCode) return default(B);

                return httpResponse.Content.ReadFromJsonAsync<B>().Result;
            }
            catch (Exception e)
            {
                return default(B);
            }
        }
        private bool PostDataFromQuery<T>(string query, T data)
        {
            try
            {
                if (this.beforeConnectToWebservice != null) if (!this.beforeConnectToWebservice(this)) return false;

                HttpResponseMessage httpResponse = this.client.PostAsJsonAsync<T>(query, data).Result;

                if (this.httpCode != null) this.httpCode(httpResponse.StatusCode);

                if (!httpResponse.IsSuccessStatusCode) return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion PostData

        #region GetData

        public T GetData<T>()
        {
            return this.GetDataFromQuery<T>(this.ApiPath);
        }

        public async Task<T> GetAsync<T>()
        {
            return await this.GetDataFromQueryAsync<T>(this.ApiPath);
        }

        public T GetData<T>(params object[] header)
        {
            string query = this.ApiPath;

            foreach (object elem in header)
            {
                query += $"/{elem.ToString()}";
            }

            return this.GetDataFromQuery<T>(query);
        }

        public async IAsyncEnumerable<T?> GetAsyncEnumerable<T>(params object[] header)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(this.ApiPath);

            foreach (object elem in header)
            {
                sb.Append($"/{elem}");
            }

            Stream stream = await this.GetStreamFromQueryAsync(sb.ToString());

            IAsyncEnumerable<T?> enumarble = JsonSerializer.DeserializeAsyncEnumerable<T>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true, DefaultBufferSize = 128 });

            await foreach (T? item in enumarble)
            {
                yield return item;
            }
        }

        private async Task<Stream> GetStreamFromQueryAsync(string query)
        {
            if (this.beforeConnectToWebservice != null) if (!this.beforeConnectToWebservice(this)) return null;

            HttpResponseMessage httpResponse = await this.client.GetAsync(query, HttpCompletionOption.ResponseHeadersRead);

            if (this.httpCode != null) this.httpCode(httpResponse.StatusCode);

            if (!httpResponse.IsSuccessStatusCode) return null;

            return await httpResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }

        public Task<T?> GetAsync<T>(params object[] header)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(this.ApiPath);

            foreach (object elem in header)
            {
                sb.Append($"/{elem}");
            }

            return this.GetDataFromQueryAsync<T>(sb.ToString());
        }

        private async Task<T?> GetDataFromQueryAsync<T>(string query)
        {
            if (this.beforeConnectToWebservice != null) if (!this.beforeConnectToWebservice(this)) return default(T);

            HttpResponseMessage httpResponse = await this.client.GetAsync(query, HttpCompletionOption.ResponseHeadersRead);

            if (this.httpCode != null) this.httpCode(httpResponse.StatusCode);

            if (!httpResponse.IsSuccessStatusCode) return default(T);

            return await httpResponse.Content.ReadFromJsonAsync<T>();
        }

        private T GetDataFromQuery<T>(string query)
        {
            try
            {
                if (this.beforeConnectToWebservice != null) if (!this.beforeConnectToWebservice(this)) return default(T);

                HttpResponseMessage httpResponse = this.client.GetAsync(query).Result;

                if (this.httpCode != null) this.httpCode(httpResponse.StatusCode);

                if (!httpResponse.IsSuccessStatusCode) return default(T);

                return httpResponse.Content.ReadFromJsonAsync<T>().Result;
            }
            catch (Exception e)
            {
                return default(T);
            }
        }

        #endregion GetData

        #region Delete


        public Task<T?> DeleteAsync<T>(params object[] header)
        {
            string query = this.ApiPath;

            foreach (object elem in header)
            {
                query += $"/{elem.ToString()}";
            }

            return this.DeleteFromQueryAsync<T>(query);
        }

        private async Task<T?> DeleteFromQueryAsync<T>(string query)
        {
            if (this.beforeConnectToWebservice != null) if (!this.beforeConnectToWebservice(this)) return default(T);

            HttpResponseMessage httpResponse = await this.client.DeleteAsync(query);

            if (this.httpCode != null) this.httpCode(httpResponse.StatusCode);

            if (!httpResponse.IsSuccessStatusCode) return default(T);

            return await httpResponse.Content.ReadFromJsonAsync<T>();
        }

        public T Delete<T>()
        {
            return this.DeleteFromQuery<T>(this.ApiPath);
        }

        public T Delete<T>(params object[] header)
        {
            string query = this.ApiPath;

            foreach (object elem in header)
            {
                query += $"/{elem.ToString()}";
            }

            return this.DeleteFromQuery<T>(query);
        }

        private T DeleteFromQuery<T>(string query)
        {
            try
            {
                if (this.beforeConnectToWebservice != null) if (!this.beforeConnectToWebservice(this)) return default(T);

                HttpResponseMessage httpResponse = this.client.DeleteAsync(query).Result;

                if (this.httpCode != null) this.httpCode(httpResponse.StatusCode);

                if (!httpResponse.IsSuccessStatusCode) return default(T);

                return httpResponse.Content.ReadFromJsonAsync<T>().Result;
            }
            catch (Exception e)
            {
                return default(T);
            }
        }

        #endregion Delete

        #endregion methods

    }
}
// -- [EOF] --