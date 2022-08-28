using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GDAX.NET
{
    public abstract class GdaxClient
    {
        private readonly RequestAuthenticator _authenticator;
        private readonly HttpClient _httpClient;

        public GdaxClient(string baseUrl, RequestAuthenticator authenticator)
        {
            _authenticator = authenticator;
            _httpClient = new HttpClient();
            BaseUri = new Uri(baseUrl, UriKind.Absolute);
        }

        public Uri BaseUri { get; }

        public async Task<ApiResponse<TResponse>> GetResponseAsync<TResponse>(ApiRequest request)
        {
            //Console.WriteLine("Sending request:\n " + request.RequestUrl + " " + request.HttpMethod + "\n" + request.RequestBody);

            var httpResponse = await GetResponseAsync(request);
            var contentBody = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

            //Console.Write("Received response for " + request.RequestUrl + " - " + httpResponse.StatusCode);

            //if (!string.IsNullOrEmpty(contentBody))
            //    Console.WriteLine("\n" + contentBody);

            Dictionary<string, IEnumerable<string>> headers = new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase);

            if (headers != null)
                foreach (var kvp in httpResponse.Headers)
                    headers[kvp.Key] = kvp.Value;

            var response = new ApiResponse<TResponse>(headers, httpResponse.StatusCode, contentBody);

            try
            {
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    response.Value = Deserialize<TResponse>(contentBody);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("GDAX api returned invalid value:\n" + contentBody + "\n" + e);
            }

            return response;
        }

        public async Task<HttpResponseMessage> GetResponseAsync(ApiRequest request)
        {
            var httpRequest = BuildRequestMessagee(request);
            httpRequest.Headers.Add("User-Agent", "GDAX.NET API Client");
            return await _httpClient.SendAsync(httpRequest).ConfigureAwait(false);
        }

        private HttpRequestMessage BuildRequestMessagee(ApiRequest request)
        {
            var method = request.HttpMethod;

            var requestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri(BaseUri, request.RequestUrl),
                Method = method,
            };

            if (method.Method == "POST" || method.Method == "PUT") // hack
            {
                requestMessage.Content = new StringContent(string.IsNullOrEmpty(request.RequestBody) ? "" : request.RequestBody, Encoding.UTF8, "application/json");
            }

            var token = _authenticator.GetAuthenticationToken(request);
            SetHttpRequestHeaders(requestMessage, token);

            return requestMessage;
        }

        private void SetHttpRequestHeaders(HttpRequestMessage requestMessage, AuthenticationToken token)
        {
            requestMessage.Headers.Add("CB-ACCESS-KEY", token.Key);
            requestMessage.Headers.Add("CB-ACCESS-SIGN", token.Signature);
            requestMessage.Headers.Add("CB-ACCESS-TIMESTAMP", token.Timestamp.ToString());
            requestMessage.Headers.Add("CB-ACCESS-PASSPHRASE", token.Passphrase);
        }

        protected virtual string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        protected virtual T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}