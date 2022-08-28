/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NetTrader.Core;
using NetTrader.Core.Candles;
using NetTrader.Core.Database.Extensions;
using NetTrader.Core.Database.Table;
using NetTrader.Core.Gateways;
using NetTrader.Core.Gateways.GDAX;

namespace NetTrader.Gateways
{
    public class GDAXProductGateway
    {
        private GDAX.NET.ProductClient productClient;

        public GDAXProductGateway()
        {
            productClient = new GDAX.NET.ProductClient("https://api.gdax.com", Settings.Instance.GDAX.RequestAuthenticator);
        }

        public void DumpExchangeMatchesToCSV(string filePath, List<GDAXExchangeMatch> matchTrades)
        {
            using (StreamWriter outputFile = new StreamWriter(filePath))
            {
                if (matchTrades == null)
                    return;

                foreach(var c in matchTrades)
                {
                    outputFile.Write(c.ToCSV());
                }
            }
        }

        public List<GDAXExchangeMatch> GetHistoricTradesFromCSV(string filePath)
        {
            List<GDAXExchangeMatch> exchangeMatch = new List<GDAXExchangeMatch>();

            using(StreamReader reader = new StreamReader(filePath))
            {
                string line = null;
                while((line = reader.ReadLine()) != null)  
                {
                    var em = ExchangeMatch.ReadFromCSVLine(line);

                    if (em != null)
                    {
                        var c = new GDAXExchangeMatch(em);

                        if (c != null)
                            exchangeMatch.Add(c);
                    }
                }
            }

            return exchangeMatch;
        }

        public async Task<List<GDAXExchangeMatch>> GetHistoricMatchTrades(string productId, DateTime fromDate, DateTime toDate)
        {
            List<GDAXExchangeMatch> exchangeMatches = new List<GDAXExchangeMatch>();

            var matches = await productClient.GetHistoricMatchTrades(productId, fromDate, toDate);

            if(matches != null)
            {
                foreach (var match in matches)
                {
                    var type = match.side == "buy" ? TradeOrder.Type.Buy : TradeOrder.Type.Sell;

                    exchangeMatches.Add(new GDAXExchangeMatch(productId, match.price.GetValueOrDefault(), type, match.size, match.time.ToTimestamp()));
                }
            }

            return exchangeMatches;
        }

        public async Task<List<Candle>> GetHistoricRates(string currencyPair, DateTime fromDate, DateTime toDate)
        {
            List<Candle> candles = new List<Candle>();

            var timespan = fromDate - toDate;

            int fiveMinutes = 5 * 60 * 60;

            Console.WriteLine("GetHistoricRates " + fromDate + " - " + toDate + " doing " + (timespan.TotalSeconds / fiveMinutes) + " batches");

            int k = 0;
            for (int i = 0; i < timespan.TotalSeconds; i += fiveMinutes)
            {
                if (k > 0 && k % 3 == 0)
                    await Task.Delay(2500);

                var toDateSegment = fromDate.AddSeconds(i * -1);
                var fromDateSegment = fromDate.AddSeconds((i + fiveMinutes) * -1);

                Console.WriteLine(fromDateSegment + " - " + toDateSegment);

                var rates = await productClient.GetHistoricRates(currencyPair, fromDateSegment, toDateSegment, 60);

                foreach (var r in rates.Value)
                {
                    candles.Add(new Candle((long)r[0], (float)r[1], (float)r[2], (float)r[3], (float)r[4], (float)r[5]));
                }

                k++;
            }

            candles.Reverse();

            await Task.Delay(2000);

            return candles;
        }
    }
}
