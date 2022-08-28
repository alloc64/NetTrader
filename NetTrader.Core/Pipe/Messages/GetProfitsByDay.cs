/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetTrader.Core.Pipe.Interfaces;

namespace NetTrader.Core.Pipe.Messages
{
    public class GetProfitsByDay : IPipeMessage
    {
        public int RetryCount => 1;

        public Task<bool> ProcessAsync(TraderCore core)
        {
            Dictionary<string, float> profitsByDay = new Dictionary<string, float>();

            var profits = core.Database.GetProfits();

            foreach(var tradeProfit in profits)
            {
                var date = tradeProfit.Timestamp.ToString("dd-MM-yyyy");

                float profit = 0f;

                profitsByDay.TryGetValue(date, out profit);

                profit += tradeProfit.Profit;

                profitsByDay[date] = profit;
            }

            foreach(var kvp in profitsByDay)
            {
                Console.WriteLine(kvp.Key + ", " + kvp.Value);
            }

            return Task.FromResult(false);
        }
    }
}
