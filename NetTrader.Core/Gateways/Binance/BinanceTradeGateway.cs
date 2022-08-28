/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinanceExchange.API.Enums;
using BinanceExchange.API.Models.Request;
using BinanceExchange.API.Utility;
using NetTrader.Core.Database.Table;

namespace NetTrader.Core.Gateways.Binance
{
    public class BinanceTradeGateway : BinanceGateway, ITradeGateway
    {
        public BinanceTradeGateway(string currencyPair) : base(currencyPair)
        {
        }

        public async Task<List<ExchangeOrder>> GetOpenOrders()
        {
            List<ExchangeOrder> ordersList = new List<ExchangeOrder>();

            try
            {
                var exchangeOrders = await client.GetCurrentOpenOrders(new CurrentOpenOrdersRequest()
                {
                    Symbol = FixCurrencyPair(currencyPair)
                });

                if (exchangeOrders != null)
                {
                    foreach (var eo in exchangeOrders)
                    {
                        ordersList.Add(new BinanceExchangeOrder(eo));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return ordersList;
        }

        public async Task CancelOrders(IEnumerable<TradeOrder> knownOrders)
        {
            List<Guid> orderIds = new List<Guid>();

            var exchangeOrders = await GetOpenOrders();

            if (exchangeOrders != null)
            {
                foreach (var exchangeOrder in exchangeOrders)
                {
                    foreach (var order in knownOrders)
                    {
                        if (order.OrderId == exchangeOrder.Id)
                        {
                            await CancelLimitOrder(order.OrderId);
                            break;
                        }
                    }
                }
            }
        }

        public async Task CancelOrdersOfType(TradeOrder.Type type, List<Guid> knownOrders)
        {
            List<Guid> orderIds = new List<Guid>();

            var exchangeOrders = await GetOpenOrders();

            if (exchangeOrders != null)
            {
                foreach (var exchangeOrder in exchangeOrders)
                {
                    if (exchangeOrder.Type != type)
                        continue;

                    foreach (var orderId in knownOrders)
                    {
                        if (orderId == exchangeOrder.Id)
                        {
                            await CancelLimitOrder(orderId);
                            break;
                        }
                    }
                }
            }
        }

        public async Task<bool> CancelLimitOrder(Guid orderId)
        {
            try
            {
                await client.CancelOrder(new CancelOrderRequest()
                {
                    Symbol = FixCurrencyPair(currencyPair),
                    OrderId = orderId.ToLong()
                });

                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public Task<ExchangeOrder> PlaceMarketBuyOrder(string currencyPair, float amount)
        {
            throw new NotImplementedException();
        }

        public Task<ExchangeOrder> PlaceMarketSellOrder(string currencyPair, float amount)
        {
            throw new NotImplementedException();
        }

        public async Task<ExchangeOrder> PlaceLimitBuyOrder(string currencyPair, float price, float amount, bool allowTaker, OrderProcessing.TimeInForce timeInForce)
        {
            try
            {
                var response = await client.CreateOrder(new CreateOrderRequest()
                {
                    Symbol = FixCurrencyPair(currencyPair),
                    Price = price,
                    Quantity = (decimal)Math.Round(amount, 2),
                    Side = OrderSide.Buy,
                    Type = OrderType.Limit,
                    TimeInForce = TimeInForce.GTC
                });

                //TODO: timeInForce + allow taker not implemented

                var order = new BinanceExchangeOrder(response);

                if (!order.Succeded)
                {
                    Console.WriteLine("Failed to place Buy order, because order was rejected by matching engine.");
                }

                return order;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }

        public async Task<ExchangeOrder> PlaceLimitSellOrder(string currencyPair, float price, float amount, bool allowTaker, OrderProcessing.TimeInForce timeInForce)
        {
            try
            {
                var response = await client.CreateOrder(new CreateOrderRequest()
                {
                    Symbol = FixCurrencyPair(currencyPair),
                    Price = price,
                    Quantity = (decimal)Math.Round(amount, 2),
                    Side = OrderSide.Sell,
                    Type = OrderType.Limit,
                    TimeInForce = TimeInForce.GTC
                });

                //TODO: timeInForce + allow taker not implemented
                
                var order = new BinanceExchangeOrder(response);

                if (!order.Succeded)
                {
                    Console.WriteLine("Failed to place Sell order, because order was rejected by matching engine.");
                }

                return order;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }
    }
}
