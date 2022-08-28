/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System.Threading.Tasks;
using NetTrader.Core.Pipe.Interfaces;
using NetTrader.Core.Database.Table;
using NetTrader.Core.OrderProcessing;

namespace NetTrader.Core.Pipe.Messages
{
    public abstract class TradeCurrencyPairMessage : IPipeMessage
    {
        public string CurrencyPair { get; private set; }


        public int RetryCount => 1;

        public bool IgnoreOrdersWatchDog { get; set; }

        public TimeInForce TimeInForce { get; set; }

        public TradeCurrencyPairMessage(string currencyPair)
        {
            this.CurrencyPair = currencyPair;
        }

        public abstract Task<bool> ProcessAsync(TraderCore core);
    }
}
