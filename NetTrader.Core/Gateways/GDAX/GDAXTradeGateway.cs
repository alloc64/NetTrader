/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetTrader.Core.Database.Table;
using NetTrader.Core.OrderProcessing;
using GDAX.NET;

namespace NetTrader.Core.Gateways.GDAX
{
    public class GDAXTradeGateway : ITradeGateway
    {
        private OrderClient orderClient = new OrderClient("https://api.gdax.com", Settings.Instance.GDAX.RequestAuthenticator);

        public async Task<List<ExchangeOrder>> GetOpenOrders()
        {
            List<ExchangeOrder> processedOrders = new List<ExchangeOrder>();

            var orders = await orderClient.GetOpenOrdersAsync();

            foreach(var order in orders.Value)
            {
                processedOrders.Add(new GDAXExchangeOrder(order));
            }

            return processedOrders;
        }

        public async Task<ExchangeOrder> PlaceMarketBuyOrder(string currencyPair, float amount)
        {
            return await PlaceOrderInternal("market", "buy", currencyPair, amount, price: 0f, allowTaker: false, timeInForce: TimeInForce.None);
        }

        public async Task<ExchangeOrder> PlaceMarketSellOrder(string currencyPair, float amount)
        {
            return await PlaceOrderInternal("market", "sell", currencyPair, amount, price: 0f, allowTaker: false, timeInForce: TimeInForce.None);
        }

        //

        public async Task<ExchangeOrder> PlaceLimitBuyOrder(string currencyPair, float price, float amount, bool allowTaker, TimeInForce timeInForce)
        {
            return await PlaceOrderInternal("limit", "buy", currencyPair, amount, price, allowTaker, timeInForce);
        }

        public async Task<ExchangeOrder> PlaceLimitSellOrder(string currencyPair, float price, float amount, bool allowTaker, TimeInForce timeInForce)
        {
            return await PlaceOrderInternal("limit", "sell", currencyPair, amount, price, allowTaker, timeInForce);
        }

        private async Task<ExchangeOrder> PlaceOrderInternal(string type, string side, string currencyPair, float amount, float price, bool allowTaker, TimeInForce timeInForce)
        {
            ApiResponse<Order> response;
            //TODO: zabstraktnit projekty na uplny basic, kdy by typy jako TimeInForce daly pouzit i v ostatnich knihovnach 
            if (type == "limit")
                response = await orderClient.PlaceLimitOrderAsync(side, currencyPair, amount, price, postOnly:!allowTaker, timeInForce: timeInForce == TimeInForce.None ? null : timeInForce.ToString());
            else if (type == "market")
                response = await orderClient.PlaceMarketOrderAsync(side, currencyPair, amount);
            else
                throw new ArgumentException("type is invalid");

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var order = new GDAXExchangeOrder(response.Value);

                if (!order.Succeded)
                {
                    Console.WriteLine("Failed to place " + side + " order, because order was rejected by matching engine. (Price lower or eq than last match)");
                }

                return order;
            }

            Console.WriteLine("Failed to place " + side + " order: " + response.Content);
            return null;
        }

        public async Task CancelOrders(IEnumerable<TradeOrder> knownOrders)
        {
            List<Guid> orderIds = new List<Guid>();

            var exchangeOrders = await GetOpenOrders();

            if (exchangeOrders != null)
            {
                int i = 0;
                foreach (var exchangeOrder in exchangeOrders)
                {
                    if (i > 0 && i % 5 == 0)
                        await Task.Delay(1500);

                    foreach (var order in knownOrders)
                    {
                        if(order.OrderId == exchangeOrder.Id)
                        {
                            await CancelLimitOrder(order.OrderId);
                            break;
                        }
                    }

                    i++;
                }
            }
        }

        public async Task CancelOrdersOfType(TradeOrder.Type type, List<Guid> knownOrders)
        {
            List<Guid> orderIds = new List<Guid>();

            var exchangeOrders = await GetOpenOrders();

            if (exchangeOrders != null)
            {
                int i = 0;
                foreach (var exchangeOrder in exchangeOrders)
                {
                    if (exchangeOrder.Type != type)
                        continue;

                    if (i > 0 && i % 5 == 0)
                        await Task.Delay(1500);
                    
                    foreach (var orderId in knownOrders)
                    {
                        if (orderId == exchangeOrder.Id)
                        {
                            await CancelLimitOrder(orderId);
                            break;
                        }
                    }

                    i++;
                }
            }
        }

        public async Task<bool> CancelLimitOrder(Guid orderId)
        {
            Console.WriteLine("Trying cancel order " + orderId);

            var response = await orderClient.CancelOrder(orderId);

            bool result = response.StatusCode == System.Net.HttpStatusCode.OK;

            if(!result)
                Console.WriteLine("Failed to cancel order: " + response.Content);

            return result;
        }
    }
}
