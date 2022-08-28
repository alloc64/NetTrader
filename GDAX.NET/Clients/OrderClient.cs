using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GDAX.NET
{
	public interface IOrderClient
	{
		Task<ApiResponse<IEnumerable<Order>>> GetOpenOrdersAsync();
		Task<ApiResponse<IEnumerable<Guid>>> CancelOpenOrdersAsync(string productId = null);
		Task<ApiResponse<Order>> PlaceOrderAsync(string side, string productId, float size, float price, string type, string cancelAfter = null, string timeInForce = null);
	}

	public class OrderClient : GdaxClient
	{
		public OrderClient(string baseUrl, RequestAuthenticator authenticator)
			: base(baseUrl, authenticator)
		{
		}

        public async Task<ApiResponse<Order>> PlaceLimitOrderAsync(string side, string productId, float size, float price, string cancelAfter = null, string timeInForce = null, bool postOnly = true)
		{
			return await this.GetResponseAsync<Order>(
				new ApiRequest(HttpMethod.Post, "/orders", Serialize(new
				{
					size = size,
                    side = side,
                    type = "limit",
					price = price,
					product_id = productId,
					cancel_after = cancelAfter,
					time_in_force = timeInForce,
					post_only = postOnly
				}))
			);
		}

        public async Task<ApiResponse<Order>> PlaceMarketOrderAsync(string side, string productId, float amount)
        {
            return await this.GetResponseAsync<Order>(
                new ApiRequest(HttpMethod.Post, "/orders", Serialize(new
                {
                    type = "market",
                    side = side,
                    product_id = productId,
                    size = amount,
                }))
            );
        }

		public async Task<ApiResponse<IEnumerable<Order>>> GetOpenOrdersAsync()
		{
			return await this.GetResponseAsync<IEnumerable<Order>>(
				new ApiRequest(HttpMethod.Get, "/orders")
			);
		}

		public async Task<ApiResponse<IEnumerable<Guid>>> CancelOrder(Guid orderId)
		{
			return await this.GetResponseAsync<IEnumerable<Guid>>(
				new ApiRequest(HttpMethod.Delete, $"/orders/{orderId}"));

		}

		public async Task<ApiResponse<IEnumerable<Guid>>> CancelOpenOrdersAsync(string productId = null)
		{
			return await this.GetResponseAsync<IEnumerable<Guid>>(
				new ApiRequest(HttpMethod.Delete, "/orders" + (productId == null ? "" : $"?product_id={productId}")));

		}
	}
}