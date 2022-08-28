/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
namespace NetTrader.Core.Gateways
{
    public class AccountInfo
    {
        public Guid Id { get; set;  }
        public string Currency { get; set; }

        public float Available { get; set; }
        public float Balance { get; set; }
    }
}
