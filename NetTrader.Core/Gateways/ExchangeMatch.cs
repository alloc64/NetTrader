/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using NetTrader.Core.Database.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NetTrader.Core.Gateways
{
    public class ExchangeMatch
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public Exchange ExchangeType { get; set; }

        public string CurrencyPair { get; set; }

        public float Price { get; set; }

        public float PriceNormalized { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TradeOrder.Type Type { get; set; }

        public float Amount { get; set; }

        public long Time { get; set; }

        protected ExchangeMatch(Exchange exchangeType, string currencyPair, float price, TradeOrder.Type type, float amount, long time)
        {
            this.ExchangeType = exchangeType;
            this.CurrencyPair = currencyPair;
            this.Price = price;
            this.Type = type;
            this.Amount = amount;
            this.Time = time;
        }

        public static ExchangeMatch ReadFromCSVLine(string line)
        {
            if (string.IsNullOrEmpty(line))
                return null;

            var arr = line.Split(","[0]);

            if (arr.Length != 6)
                return null;

            for (int i = 0; i < arr.Length; i++)
            {
                var val = arr[i];
                arr[i] = val?.Trim();
            }

            int exchangeType = 0;
            if (!int.TryParse(arr[0], out exchangeType))
                return null;

            string currencyPair = arr[1];

            float price = 0f;
            if (!float.TryParse(arr[2], out price))
                return null;

            int type = 0;
            if (!int.TryParse(arr[3], out type))
                return null;

            float amount = 0f;
            if (!float.TryParse(arr[4], out amount))
                return null;

            long time = 0;
            if (!long.TryParse(arr[5], out time))
                return null;

            return new ExchangeMatch((Exchange)exchangeType, currencyPair, price, (TradeOrder.Type)type, amount, time);
        }

        public string ToCSV()
        {
            return string.Format("{0}, {1}, {2}, {3}, {4}, {5}\n", (int)ExchangeType, CurrencyPair, Price, (int)Type, Amount, Time);
        }

        public override string ToString()
        {
            return string.Format("[ExchangeMatch: ExchangeType={0}, CurrencyPair={1}, Price={2}, Type={3}, Amount={4}, Time={5}]", ExchangeType, CurrencyPair, Price, Type, Amount, Time);
        }
    }
}
