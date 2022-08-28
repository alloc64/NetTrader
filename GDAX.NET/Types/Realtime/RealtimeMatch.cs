using System;

namespace GDAX.NET
{
    public class RealtimeMatch : RealtimeMessage
    {
        public decimal trade_id { get; set; }
        public string maker_order_id { get; set; }
        public string taker_order_id { get; set; }
        public float size { get; set; }
        public DateTime time { get; set; }
    }
}