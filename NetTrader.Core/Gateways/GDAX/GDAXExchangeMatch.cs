/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using NetTrader.Core.Database.Extensions;
using NetTrader.Core.Database.Table;
using GDAX.NET;

namespace NetTrader.Core.Gateways.GDAX
{
    public class GDAXExchangeMatch : ExchangeMatch
    {
        public GDAXExchangeMatch(ExchangeMatch match)
            : base(Exchange.GDAX, match.CurrencyPair, match.Price, match.Type, match.Amount, match.Time)
        {
        }

        public GDAXExchangeMatch(string currencyPair, float price, TradeOrder.Type type, float amount, long time)
            : base(Exchange.GDAX, currencyPair, price, type, amount, time)
        {
        }

        public static GDAXExchangeMatch FromRealtimeMatch(string currencyPair, RealtimeMatch match)
        {
            var type = match.side == "buy" ? TradeOrder.Type.Buy : TradeOrder.Type.Sell;

            return new GDAXExchangeMatch(currencyPair, match.price.GetValueOrDefault(), type, match.size, match.time.ToTimestamp());
        }
    }
}
