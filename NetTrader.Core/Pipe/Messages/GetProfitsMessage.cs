/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NetTrader.Core.Database;
using NetTrader.Core.Database.Table;
using NetTrader.Core.Pipe.Interfaces;
using NetTrader.Core.Server.Messages;

namespace NetTrader.Core.Pipe.Messages
{
    public class GetProfitsMessage : IPipeMessage
    {
        public class ProfitInfo
        {
            public IEnumerable<TradeProfit> Profits { get; private set; }

            public float ProfitSum { get; private set; }

            public ProfitInfo(IEnumerable<TradeProfit> profits)
            {
                if (profits == null)
                    profits = new List<TradeProfit>();

                this.Profits = profits;

                foreach(var profit in profits)
                {
                    ProfitSum += profit.Profit;
                }
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();

                foreach (var profit in Profits)
                {
                    sb.Append(profit.ToString());
                    sb.Append("\n");
                }

                return string.Format("[ProfitInfo: Profits={0}\nProfitSum={1}]", sb.ToString(), ProfitSum);
            }
        }

        public int RetryCount => 5;

        public GetProfitsMessage()
        {
        }

        public Task<bool> ProcessAsync(TraderCore core)
        {
            var profits = new ProfitInfo(core.Database.GetProfits());

            core.Server.SendMessage(new ServerMessage(ServerMessage.Type.GetProfitsMessage, profits));

            return Task.FromResult(true);
        }
    }
}
