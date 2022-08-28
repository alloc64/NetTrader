using System;

namespace GDAX.NET
{
	public class RealtimeMessageWithOrderId : RealtimeMessage
	{ 
        public Guid order_id { get; set; }
        public DateTime time { get; set; }
	}
    
}