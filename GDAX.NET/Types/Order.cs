using System;

namespace GDAX.NET
{
    public class Order
    {
        public Guid id { get; set; }
        public float price { get; set; }
        public float size { get; set; }
        public string product_id { get; set; }
        public string side { get; set; }
        public string stp { get; set; }
        public string type { get; set; }
        public string time_in_force { get; set; }
        public string post_only { get; set; }
        public string created_at { get; set; }
        public float fill_fees { get; set; }
        public float filled_size { get; set; }
        public float executed_value { get; set; }
        public string status { get; set; }
        public bool settled { get; set; }
    }
}