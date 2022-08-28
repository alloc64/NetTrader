/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using NetTrader.Core.Database.Table;

namespace NetTrader.Core.Gateways.Binance
{
    public class BinanceExchangeMatch : ExchangeMatch
    {
        public BinanceExchangeMatch(string currencyPair, decimal price, TradeOrder.Type type, decimal amount, long time) 
            : base(Exchange.Binance, currencyPair, (float)price, type, (float)amount, time)
        {
        }
    }
}
