/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Threading.Tasks;
using NetTrader.Core.Pipe.Interfaces;
using NetTrader.Core.Server.Messages;

namespace NetTrader.Core.Pipe.Messages
{
    public class GetAccountBalanceMessage : IPipeMessage
    {
        public int RetryCount => 5;

        public async Task<bool> ProcessAsync(TraderCore core)
        {
            var s = core.CurrencyPair.Split("-"[0]);
            var accounts = await core.AccountGateway.GetAccountsForCurrencyPair(s[0], s[1]);

            if (accounts != null)
                core.Server.SendMessage(new ServerMessage(ServerMessage.Type.GetAccountBalanceMessage, accounts));

            return true;
        }
    }
}
