/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System.Threading.Tasks;

namespace NetTrader.Core.Pipe.Interfaces
{
    public interface IPipeMessage
    {
        int RetryCount { get; }

        Task<bool> ProcessAsync(TraderCore core);
    }
}
