using System;

namespace GDAX.NET
{
    public class Account
    {
        public Guid id { get; set; }
        public string currency { get; set; }
        public float balance { get; set; }
        public float hold { get; set; }
        public float available { get; set; }
        public bool margin_enabled { get; set; }
        public float funded_amount { get; set; }
        public float default_amount { get; set; }
    }
}