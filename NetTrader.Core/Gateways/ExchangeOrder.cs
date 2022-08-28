/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/
using System;
using NetTrader.Core.Database.Table;

namespace NetTrader.Core.Gateways
{
    public class ExchangeOrder
    {
        public Guid Id { get; set; }

        public Exchange ExchangeType { get; set; }

        public TradeOrder.Type Type { get; set; }

        public string CurrencyPair { get; set; }

        public float Price { get; set; }

        public float Amount { get; set; }

        public TradeOrder.State State { get; set; }

        public bool Succeded { get; set; }

        public ExchangeOrder()
        {
            
        }

        protected ExchangeOrder(Exchange exchangeType)
        {
            this.ExchangeType = exchangeType;
        }
    }
}
