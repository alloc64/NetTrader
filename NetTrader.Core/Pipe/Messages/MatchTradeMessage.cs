/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetTrader.Core.Candles;
using NetTrader.Core.Database.Table;
using NetTrader.Core.Gateways;
using NetTrader.Core.Pipe.Interfaces;
using NetTrader.Core.Server;
using NetTrader.Core.Server.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NetTrader.Core.Pipe.Messages
{
    public class TradeInfo
    {
        public TradeInfo(ExchangeMatch match)
        {
            ExchangeType = match.ExchangeType;
            Type = match.Type;
            Price = match.Price;
            Amount = match.Amount;
            Time = match.Time;
        }

        public class PriceInfo
        {
            public float Price { get; set; }
            public bool Active { get; set; }

            public PriceInfo(float price, bool active)
            {
                this.Price = price;
                this.Active = active;
            }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public Exchange ExchangeType { get; set;  }

        public float Price { get; set; }

        public Candle Candle { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TradeOrder.Type Type { get; set; }

        public float Amount { get; set; }

        public long Time { get; set;  }

        public List<PriceInfo> BuyPrices { get; set; } = new List<PriceInfo>();
        public List<PriceInfo> SellPrices { get; set; } = new List<PriceInfo>();

        public override string ToString()
        {
            return string.Format("[TradeInfo: Price={0}, Candle={1}, Type={2}, Amount={3}, Time={4}, BuyPrices={5}, SellPrices={6}]", Price, Candle, Type, Amount, Time, BuyPrices, SellPrices);
        }
    }

    public class MatchTradeMessage : IPipeMessage
    {
        public ExchangeMatch Match { get; private set; }

        //

        public MatchTradeMessage(ExchangeMatch match)
        {
            this.Match = match;
        }

        public int RetryCount => 5;

        public async Task<bool> ProcessAsync(TraderCore core)
        {
            bool isOurCurrencyMatch = core.CurrencyPair == Match.CurrencyPair;
            var lastMatch = core.LastMatch;

            if (isOurCurrencyMatch)
                core.LastMatch = Match;

            await core.Strategy.OnMatchOccured(Match);

            if (!isOurCurrencyMatch)
                return true;

            await core.StopLossProcessor.ProcessMatchTrade(Match, lastMatch);

            await core.TargetBasePriceCalculator.ProcessMatchTrade(Match);

            SendTradeInfo(core);

            return true;
        }

        private void SendTradeInfo(TraderCore core)
        {
            float price = Match.Price;

            TradeInfo tradeInfo = new TradeInfo(Match)
            {
                Candle = core.CandlesProcessor.CreateCandle(core, Match)
            };

            if (core.Tiers != null)
            {
                var database = core.Database;

                foreach (var tier in core.Tiers)
                {
                    var buyOrder = database.GetOpenOrderInTier(TradeOrder.Type.Buy, tier.Id);
                    float buyPrice = tier.GetBuyPrice(core.BasePrice);

                    tradeInfo.BuyPrices.Add(new TradeInfo.PriceInfo(buyPrice, buyOrder != null));

                    //

                    var sellOrder = database.GetOpenOrderInTier(TradeOrder.Type.Sell, tier.Id);
                    float sellPrice = tier.GetSellPrice(core.BasePrice);

                    tradeInfo.SellPrices.Add(new TradeInfo.PriceInfo(sellPrice, sellOrder != null));
                }
            }

            core.Server.SendMessage(new ServerMessage(ServerMessage.Type.Match, tradeInfo));
        }

        public override string ToString()
        {
            return string.Format("[MatchTradeMessage: match={0}]", Match);
        }
    }
}
