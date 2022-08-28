/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System.Threading.Tasks;
using NetTrader.Core.Pipe.Interfaces;
using NetTrader.Core.Server.Messages;

namespace NetTrader.Core.Pipe.Messages
{
    public class GetOpenOrdersMessage : IPipeMessage
    {
        public int RetryCount => 5;

        public Task<bool> ProcessAsync(TraderCore core)
        {
            core.Server.SendMessage(new ServerMessage(ServerMessage.Type.GetOrders, core.Database.GetOpenOrders()));

            return Task.FromResult(true);
        }
    }
}
