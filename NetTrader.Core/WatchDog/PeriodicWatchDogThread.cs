/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System.Threading.Tasks;

namespace NetTrader.Core.WatchDog
{
    public abstract class PeriodicWatchDogThread : WatchDogThread
    {
        public abstract int SecondsDelay { get; }

        private bool running = false;

        public PeriodicWatchDogThread(TraderCore core) : base(core)
        {
        }

        //
        public override void Start()
        {
            base.Start();

            running = true;
        }

        public virtual void Stop()
        {
            running = false;
        }

        protected abstract Task RunScheduledTask();

        protected override async Task Process()
        {
            while (running)
            {
                await RunScheduledTask();
                await Task.Delay(SecondsDelay * 1000);
            }
        }
    }
}
