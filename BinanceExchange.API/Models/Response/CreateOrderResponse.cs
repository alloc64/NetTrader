using System;
using System.Runtime.Serialization;
using BinanceExchange.API.Converter;
using BinanceExchange.API.Enums;
using BinanceExchange.API.Models.Response.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BinanceExchange.API.Models.Response
{
    /// <summary>
    /// Response following a call to the Create Order endpoint
    /*
        {"symbol":"ETHUSDT", "orderId":20214931,"clientOrderId":"yTLRHpKtazYReADr6GVFrb","transactTime":1516989949024,"price":"1038.00000000","origQty":"0.02000000","executedQty":"0.00000000","status":"NEW","timeInForce":"GTC","type":"LIMIT","side":"BUY"}
    */
    /// </summary>
    [DataContract]
    public class CreateOrderResponse : IResponse
    {
        [DataMember(Order = 1)]
        public string Symbol { get; set; }

        [DataMember(Order = 2)]
        public long OrderId { get; set; }

        [DataMember(Order = 3)]
        public string ClientOrderId { get; set; }

        [DataMember(Order = 4)]
        [JsonProperty("transactTime")]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime TransactionTime { get; set; }

        [DataMember(Order = 5)]
        public decimal Price { get; set; }

        [DataMember(Order = 6)]
        [JsonProperty(PropertyName = "origQty")]
        public decimal OriginalQuantity { get; set; }

        [DataMember(Order = 7)]
        [JsonProperty(PropertyName = "executedQty")]
        public decimal ExecutedQuantity { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember(Order = 8)]
        public OrderStatus Status { get; set; }

        [DataMember(Order = 9)]
        public TimeInForce TimeInForce { get; set; }

        [DataMember(Order = 10)]
        public OrderType Type { get; set; }

        [DataMember(Order = 11)]
        public OrderSide Side { get; set; }
    }
}

    