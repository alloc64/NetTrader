using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace GDAX.NET
{
    public interface IProductClient
    {
        Task<ApiResponse<IEnumerable<Product>>> GetProductsAsync();
        Task<ApiResponse<ProductTicker>> GetProductTickerAsync(string productId);
        Task<ApiResponse<OrderBook>> GetOrderBookAsync(string productId, int level = 1);
    }

    public class ProductClient : GdaxClient, IProductClient
    {
        public ProductClient(string baseUrl, RequestAuthenticator authenticator)
            : base(baseUrl, authenticator)
        {
        }

        public async Task<ApiResponse<IEnumerable<Product>>> GetProductsAsync()
        {
            return await this.GetResponseAsync<IEnumerable<Product>>(
                new ApiRequest(HttpMethod.Get, "/products")
            );
        }

        public async Task<ApiResponse<IEnumerable<double[]>>> GetHistoricRates(string productId, DateTime fromDate, DateTime toDate, int granularity)
        {
            var response = await this.GetResponseAsync<IEnumerable<double[]>>(
                new ApiRequest(HttpMethod.Get, $"/products/{productId}/candles?start={fromDate.ToString("s")}&end={toDate.ToString("s")}&granularity={granularity}")
            );

            List<double[]> filteredResponse = new List<double[]>();

            if(response != null && response.Value != null)
            {
                foreach(var r in response.Value)
                {
                    var date = r[0];

                    //Console.WriteLine(date.UnixTimeStampToDateTime());

                    if (date >= fromDate.ToUnixTimestamp() && date <= toDate.ToUnixTimestamp())
                        filteredResponse.Add(r);
                }
            }

            response.Value = filteredResponse;
            return response;
        }

        public async Task<List<RealtimeMatch>> GetHistoricMatchTrades(string productId, DateTime fromDate, DateTime toDate)
        {
            List<RealtimeMatch> matches = new List<RealtimeMatch>();

            fromDate = fromDate.ToUniversalTime();
            toDate = toDate.ToUniversalTime();

            long after = 0;
            bool running = true;

            int k = 0;
            do
            {
                var tradesResponse = await GetHistoricMatchTrades(productId, after, 0);

                after = tradesResponse.After;

                if (tradesResponse.Value != null)
                {
                    foreach (var trade in tradesResponse.Value)
                    {
                        if(trade.time < toDate)
                        {
                            running = false;
                            break;
                        }

                        //Console.WriteLine(trade.time + " > " + fromDate + " < " + toDate);

                        if (trade.time > toDate && trade.time < fromDate)
                        {
                            matches.Add(trade);
                        }
                    }
                }
                else
                {
                    running = false;
                }

                if (k > 0 && k % 6 == 0)
                    await Task.Delay(1500);

                k++;
            }
            while (running);

            //matches.Reverse();

            await Task.Delay(2500);

            return matches;
        }

        public async Task<ApiResponse<List<RealtimeMatch>>> GetHistoricMatchTrades(string productId, long after, int limit)
        {
            string url = $"/products/{productId}/trades?";

            if (after > 0)
                url += $"after={after}&";

            if(limit > 0)
                url += $"limit={limit}";

            //Console.WriteLine(url);

            var response = await this.GetResponseAsync<List<RealtimeMatch>>(
                new ApiRequest(HttpMethod.Get, url)
            );

            return response;
        }

        public async Task<ApiResponse<ProductTicker>> GetProductTickerAsync(string productId)
        {
            var response = await this.GetResponseAsync<ProductTicker>(
                new ApiRequest(HttpMethod.Get, $"/products/{productId}/ticker")
            );

            if(response.Value != null)
                response.Value.product_id = productId;

            return response;
        }

        public async Task<ApiResponse<OrderBook>> GetOrderBookAsync(string productId, int level = 1)
        {
            return await this.GetResponseAsync<OrderBook>(
                new ApiRequest(HttpMethod.Get, $"/products/{productId}/book?level={level}")
            );
        }
    }
}