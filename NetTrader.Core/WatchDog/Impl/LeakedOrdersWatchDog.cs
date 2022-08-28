/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System.Collections.Generic;
using System.Threading.Tasks;
using NetTrader.Core.Database.Table;
using NetTrader.Core.Pipe.Interfaces;
using NetTrader.Core.Pipe.Messages;

namespace NetTrader.Core.WatchDog.Impl
{
    
    public class LeakedOrdersWatchDog : PeriodicWatchDogThread
    {
        public override int SecondsDelay => 5;

        private List<IPipeMessage> queue = new List<IPipeMessage>();

        public LeakedOrdersWatchDog(TraderCore core) : base(core)
        {
        }

        public void EnqueueOrderMessage(TradeCurrencyPairMessage msg)
        {
            if (queue == null)
                return;

            msg.IgnoreOrdersWatchDog = true;

            if(!queue.Contains(msg))
                queue.Add(msg);
        }

        protected override async Task RunScheduledTask()
        {
            for (int i = 0; i < queue.Count; i++)
            {
                if (i > 0 && i % 5 == 0)
                    await Task.Delay(1500);
                
                var msg = queue[i];

                bool success = await msg.ProcessAsync(core);

                if (success)
                    queue.RemoveAt(i);
            }
        }

        public override void Stop()
        {
            base.Stop();
            Clear();
        }

        public void Clear()
        {
            queue.Clear();
        }
    }
}
