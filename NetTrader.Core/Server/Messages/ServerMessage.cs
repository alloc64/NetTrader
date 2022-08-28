/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NetTrader.Core.Server.Messages
{
    public class ServerMessage
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum Type
        {
            Unknown = 0,
            Console,
            Match,
            GetOrders,
            OrderStateChanged,
            RestartTrading,
            MarketOrder,
            RemoveOrder,
            CancelOrder,
            CancelAllOrders,

            GetTargetBasePrice,
            SetTargetBasePrice,
            SetTBPRecalculationOffsetMessage,
            SetTBPRecalculationPriceOffsetMessage,
            SetStopLossThreshold,
            SetStopLossBasePriceOffset,

            GetProfitsMessage,
            GetAccountBalanceMessage,
            StopLossMessage,
        }

        public Type MessageType { get; set; }
        public object Payload { get; set; }

        public ServerMessage(Type type, object payload)
        {
            this.MessageType = type;
            this.Payload = payload;
        }
    }
}
