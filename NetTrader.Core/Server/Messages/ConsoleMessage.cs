/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;

namespace NetTrader.Core.Server.Messages
{
    public class ConsoleMessage : ServerMessage
    {
        public ConsoleMessage(object payload) : base(Type.Console, payload)
        {
            Console.WriteLine(payload.ToString());
        }
    }
}
