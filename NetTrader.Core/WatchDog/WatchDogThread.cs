/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System.Threading.Tasks;

namespace NetTrader.Core.WatchDog
{
    public abstract class WatchDogThread : IWatchDogThread
    {
        protected TraderCore core;

        public WatchDogThread(TraderCore core)
        {
            this.core = core;
        }

        public virtual void Start()
        {
            Task.Run(Process);
        }

        protected abstract Task Process();
    }
}
