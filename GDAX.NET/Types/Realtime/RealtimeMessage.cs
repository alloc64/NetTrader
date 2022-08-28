using System;

namespace GDAX.NET
{
    public class RealtimeMessage
    {
        public string type { get; set; }
        public long sequence { get; set; }
        public float? price { get; set; }
        public string side { get; set; }
    }
}