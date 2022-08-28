/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using SQLite.Net.Attributes;

namespace NetTrader.Core.Database.Table
{
    public class Tier
    {
        [Unique]
        public int Id { get; set; }

        private float _currencyAmount;
        public float CurrencyAmount 
        {
            get { return _currencyAmount;  }
            set { _currencyAmount = (float)Math.Round(value, 4); }
        }

        public float BuyOffset { get; set; }

        public float SellProfit { get; set; }

        public float GetBuyPrice(float currentTradePrice)
        {
            return (float)Math.Round(currentTradePrice - BuyOffset, 2);
        }

        public float GetSellPrice(float buyPrice)
        {
            return (float)Math.Round(buyPrice + SellProfit, 2);
        }

        public override string ToString()
        {
            return string.Format("[Tier: Id={0}, CurrencyAmount={1}, BuyOffset={2}, SellProfit={3}]", Id, CurrencyAmount, BuyOffset, SellProfit);
        }
    }
}
