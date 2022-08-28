/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;
using System;

namespace NetTrader.Core.Server
{
    public class WebsocketCommunicationChannel : WebSocketBehavior
    {
        private TraderServer server;

        public WebsocketCommunicationChannel(TraderServer server)
        {
            this.server = server;
        }

        public void Broadcast(object val)
        {
            if (Sessions != null && val != null)
            {
                var json = JsonConvert.SerializeObject(val);

                //Console.WriteLine("Broadcasting: " + json);

                Sessions.Broadcast(json);
            }
        }

        protected override void OnClose(CloseEventArgs e)
        {
            Sessions.Broadcast("Client disconnected: " + e.Reason);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            Sessions.Broadcast(e.Data);

            server.OnMessageReceived(e);
        }
    }
}
