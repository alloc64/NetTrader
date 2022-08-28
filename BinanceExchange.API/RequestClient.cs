using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinanceExchange.API.Enums;
using BinanceExchange.API.Extensions;

namespace BinanceExchange.API
{
    internal class RequestClient
    {
        private HttpClient HttpClient;
     
        private string _apiKey = string.Empty;
        private const string APIHeader = "X-MBX-APIKEY";

        private static TimeSpan _timestampOffset;

        public RequestClient(string apiKey)
        {
            var httpClientHandler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                Proxy = new WebProxy()
                {
                    Address = new Uri("http://localhost:8888/"),
                },
                UseProxy = true
            };

            System.Net.ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            HttpClient = new HttpClient(httpClientHandler);
            //HttpClient.Timeout = TimeSpan.FromSeconds(5);
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(APIHeader, new[] { apiKey });
        }


        /// <summary>
        /// Used to adjust the client timestamp
        /// </summary>
        /// <param name="time">TimeSpan to adjust timestamp by</param>
        public void SetTimestampOffset(TimeSpan time)
        {
            _timestampOffset = time;
            //Console.WriteLine($"Timestamp offset is now : {time}");
        }

        /// <summary>
        /// Create a generic GetRequest to the specified endpoint
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> GetRequest(Uri endpoint)
        {
            Console.WriteLine($"Creating a GET Request to {endpoint.AbsoluteUri}");
            return await CreateRequest(endpoint);
        }

        /// <summary>
        /// Creates a generic GET request that is signed
        /// </summary>s
        /// <param name="endpoint"></param>
        /// <param name="apiKey"></param>
        /// <param name="secretKey"></param>
        /// <param name="signatureRawData"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> SignedGetRequest(Uri endpoint, string secretKey, string signatureRawData, long receiveWindow = 5000)
        {
            var uri = CreateValidUri(endpoint, secretKey, signatureRawData, receiveWindow);
            return await CreateRequest(uri, HttpVerb.GET);
        }

        /// <summary>
        /// Create a generic PostRequest to the specified endpoint
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PostRequest(Uri endpoint)
        {
            Console.WriteLine($"Creating a POST Request to {endpoint.AbsoluteUri}");
            return await CreateRequest(endpoint, HttpVerb.POST);
        }

        /// <summary>
        /// Create a generic DeleteRequest to the specified endpoint
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> DeleteRequest(Uri endpoint)
        {
            Console.WriteLine($"Creating a DELETE Request to {endpoint.AbsoluteUri}");
            return await CreateRequest(endpoint, HttpVerb.DELETE);
        }

        /// <summary>
        /// Create a generic PutRequest to the specified endpoint
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PutRequest(Uri endpoint)
        {
            Console.WriteLine($"Creating a PUT Request to {endpoint.AbsoluteUri}");
            return await CreateRequest(endpoint, HttpVerb.PUT);
        }

        /// <summary>
        /// Creates a generic GET request that is signed
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="secretKey"></param>
        /// <param name="signatureRawData"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> SignedPostRequest(Uri endpoint,string secretKey, string signatureRawData, long receiveWindow = 5000)
        {
            var uri = CreateValidUri(endpoint, secretKey, signatureRawData, receiveWindow);
            return await CreateRequest(uri, HttpVerb.POST);
        }

        /// <summary>
        /// Creates a generic DELETE request that is signed
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="secretKey"></param>
        /// <param name="signatureRawData"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> SignedDeleteRequest(Uri endpoint, string secretKey, string signatureRawData, long receiveWindow = 5000)
        {
            Console.WriteLine($"Creating a SIGNED DELETE Request to {endpoint.AbsoluteUri}");
            var uri = CreateValidUri(endpoint, secretKey, signatureRawData, receiveWindow);
            return await CreateRequest(uri, HttpVerb.DELETE);
        }


        /// <summary>
        /// Creates a valid Uri with signature
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="secretKey"></param>
        /// <param name="signatureRawData"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        /// 
        private Uri CreateValidUri(Uri endpoint, string secretKey, string signatureRawData, long receiveWindow)
        {
            string timestamp;
#if NETSTANDARD2_0
            timestamp = DateTimeOffset.UtcNow.AddMilliseconds(_timestampOffset.TotalMilliseconds).ToUnixTimeMilliseconds().ToString();
#else
            timestamp = DateTime.UtcNow.AddMilliseconds(_timestampOffset.TotalMilliseconds).ConvertToUnixTime().ToString();
#endif
            var qsDataProvided = !string.IsNullOrEmpty(signatureRawData);
            var argEnding = $"timestamp={timestamp}&recvWindow={receiveWindow}";
            var adjustedSignature = !string.IsNullOrEmpty(signatureRawData) ? $"{signatureRawData.Substring(1)}&{argEnding}" : $"{argEnding}";
            var hmacResult = CreateHMACSignature(secretKey, adjustedSignature);
            var connector = !qsDataProvided ? "?" : "&";
            var uri = new Uri($"{endpoint}{connector}{argEnding}&signature={hmacResult}");
            return uri;
        }

        /// <summary>
        /// Creates a HMACSHA256 Signature based on the key and total parameters
        /// </summary>
        /// <param name="key">The secret key</param>
        /// <param name="totalParams">URL Encoded values that would usually be the query string for the request</param>
        /// <returns></returns>
        private static string CreateHMACSignature(string key, string totalParams)
        {
            var messageBytes = Encoding.UTF8.GetBytes(totalParams);
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var hash = new HMACSHA256(keyBytes);
            var computedHash = hash.ComputeHash(messageBytes);
            return BitConverter.ToString(computedHash).Replace("-", "").ToLower();
        }

        /// <summary>
        /// Makes a request to the specifed Uri, only if it hasn't exceeded the call limit 
        /// </summary>
        /// <param name="endpoint">Endpoint to request</param>
        /// <param name="verb"></param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> CreateRequest(Uri endpoint, HttpVerb verb = HttpVerb.GET)
        {
            switch (verb)
            {
                case HttpVerb.GET:
                    return await HttpClient.GetAsync(endpoint).ConfigureAwait(false);

                case HttpVerb.POST:
                    return await HttpClient.PostAsync(endpoint, null).ConfigureAwait(false);

                case HttpVerb.DELETE:
                    return await HttpClient.DeleteAsync(endpoint).ConfigureAwait(false);

                case HttpVerb.PUT:
                    return await HttpClient.PutAsync(endpoint, null).ConfigureAwait(false);

                default:
                    throw new ArgumentOutOfRangeException(nameof(verb), verb, null);
            }
        }
    }
}
