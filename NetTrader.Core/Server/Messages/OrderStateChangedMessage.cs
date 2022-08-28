/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

namespace NetTrader.Core.Server.Messages
{
    public class OrderStateChangedMessage : ServerMessage
    {
        public OrderStateChangedMessage(object payload) : base(Type.OrderStateChanged, payload)
        {
        }
    }
}
