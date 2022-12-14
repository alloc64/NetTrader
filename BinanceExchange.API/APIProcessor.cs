using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BinanceExchange.API.Caching.Interfaces;
using BinanceExchange.API.Enums;
using BinanceExchange.API.Models.Response.Error;
using Newtonsoft.Json;

namespace BinanceExchange.API
{
    /// <summary>
    /// The API Processor is the chief piece of functionality responsible for handling and creating requests to the API
    /// </summary>
    public class APIProcessor : IAPIProcessor
    {
        private readonly string _secretKey;

        private RequestClient client;

        public APIProcessor(string apiKey, string secretKey)
        {
            _secretKey = secretKey;

            client = new RequestClient(apiKey);
        }

        private async Task<T> HandleResponse<T>(HttpResponseMessage message, string requestMessage) where T : class
        {
            if (message.IsSuccessStatusCode)
            {
                var messageJson = await message.Content.ReadAsStringAsync();
                T messageObject = null;
                try
                {
                    messageObject = JsonConvert.DeserializeObject<T>(messageJson);
                }
                catch (Exception ex)
                {
                    string deserializeErrorMessage = $"Unable to deserialize message from: {requestMessage}. Exception: {ex.Message}";
                    Console.WriteLine(deserializeErrorMessage);
                    throw new BinanceException(deserializeErrorMessage, new BinanceError()
                    {
                        RequestMessage = requestMessage,
                        Message = ex.Message
                    });
                }

                if (messageObject == null)
                {
                    throw new Exception("Unable to deserialize to provided type");
                }

                return messageObject;
            }
            var errorJson = await message.Content.ReadAsStringAsync();
            var errorObject = JsonConvert.DeserializeObject<BinanceError>(errorJson);
            if (errorObject == null) throw new BinanceException("Unexpected Error whilst handling the response", null);
            errorObject.RequestMessage = requestMessage;
            var exception = CreateBinanceException(message.StatusCode, errorObject);
            Console.WriteLine($"Error Message Received:", exception);
            throw exception;
        }

        private BinanceException CreateBinanceException(HttpStatusCode statusCode, BinanceError errorObject)
        {
            if (statusCode == HttpStatusCode.GatewayTimeout)
            {
                return new BinanceTimeoutException(errorObject);
            }
            var parsedStatusCode = (int)statusCode;
            if (parsedStatusCode >= 400 && parsedStatusCode <= 500)
            {
                return new BinanceBadRequestException(errorObject);
            }
            return parsedStatusCode >= 500 ? 
                new BinanceServerException(errorObject) : 
                new BinanceException("Binance API Error", errorObject);
        }


        /// <summary>
        /// Processes a GET request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<T> ProcessGetRequest<T>(BinanceEndpointData endpoint, int receiveWindow = 5000) where T : class
        {
            HttpResponseMessage message;
            switch (endpoint.SecurityType) { 
                case EndpointSecurityType.ApiKey:
                case EndpointSecurityType.None:
                    message = await client.GetRequest(endpoint.Uri);
                    break;
                case EndpointSecurityType.Signed:
                    message = await client.SignedGetRequest(endpoint.Uri, _secretKey, endpoint.Uri.Query, receiveWindow);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return await HandleResponse<T>(message, endpoint.ToString());
        }

        /// <summary>
        /// Processes a DELETE request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<T> ProcessDeleteRequest<T>(BinanceEndpointData endpoint, int receiveWindow = 5000) where T : class
        {
            HttpResponseMessage message;
            switch (endpoint.SecurityType) { 
                case EndpointSecurityType.ApiKey:
                    message = await client.DeleteRequest(endpoint.Uri);
                    break;
                case EndpointSecurityType.Signed:
                    message = await client.SignedDeleteRequest(endpoint.Uri, _secretKey, endpoint.Uri.Query, receiveWindow);
                    break;
                case EndpointSecurityType.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return await HandleResponse<T>(message, endpoint.ToString());
        }

        /// <summary>
        /// Processes a POST request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<T> ProcessPostRequest<T>(BinanceEndpointData endpoint, int receiveWindow = 5000) where T : class
        {
            HttpResponseMessage message;
            switch (endpoint.SecurityType) { 
                case EndpointSecurityType.ApiKey:
                    message = await client.PostRequest(endpoint.Uri);
                    break;
                case EndpointSecurityType.None:
                    throw new ArgumentOutOfRangeException();
                case EndpointSecurityType.Signed:
                    message = await client.SignedPostRequest(endpoint.Uri, _secretKey, endpoint.Uri.Query, receiveWindow);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return await HandleResponse<T>(message, endpoint.ToString());
        }

        /// <summary>
        /// Processes a PUT request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<T> ProcessPutRequest<T>(BinanceEndpointData endpoint, int receiveWindow = 5000) where T : class
        {
            HttpResponseMessage message;
            switch (endpoint.SecurityType) { 
                case EndpointSecurityType.ApiKey:
                    message = await client.PutRequest(endpoint.Uri);
                    break;
                case EndpointSecurityType.None:
                case EndpointSecurityType.Signed:
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return await HandleResponse<T>(message, endpoint.ToString());
        }
    }
}