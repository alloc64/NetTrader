using System.Collections.Generic;

namespace GDAX.NET
{
    public class OrderBook
    {
        public long sequence { get; set; }
        public IEnumerable<string[]> bids { get; set; }
        public IEnumerable<string[]> asks { get; set; }
    }
}