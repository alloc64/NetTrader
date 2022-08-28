/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetTrader.Core.Pipe.Interfaces;
using NetTrader.Core.Server.Messages;

namespace NetTrader.Core.Pipe
{
    public class MessagePipe
    {
        private bool running;

        private Queue<IPipeMessage> queue = new Queue<IPipeMessage>();

        private TraderCore core;

        //

        public MessagePipe(TraderCore core)
        {
            this.core = core;
        }

        public void Start()
        {
            running = true;

            Task.Run(ProcessMessages);
        }

        public void Stop()
        {
            running = false;
        }

        //

        public void SendMessage(IPipeMessage msg)
        {

            if(msg != null)
                queue.Enqueue(msg);
        }

        //

        private async Task ProcessMessages()
        {
            while (running)
            {
                while (queue.Count > 0)
                {
                    var msg = queue.Peek();

                    try
                    {
                        //core.Server.SendMessage(new ConsoleMessage("Processing msg: " + msg + "\n"));

                        bool success = false;

                        if (msg != null)
                        {
                            for (int i = 0; i < msg.RetryCount; i++)
                            {
                                success = await ProcessMessage(msg);

                                if (success)
                                {
                                    break;
                                }
                            }

                            if (!success)
                            {
                                core.Server.SendMessage(new ConsoleMessage("Execution of message " + msg + " failed"));
                            }
                        }

                        queue.Dequeue();
                    }
                    catch(Exception e)
                    {
                        core.Server.SendMessage(new ConsoleMessage("Fatal exception occured while processing message " + msg + "\n" + e));
                    }
                }

                await Task.Delay(1);
            }
        }

        private async Task<bool> ProcessMessage(IPipeMessage msg)
        {
            return await msg.ProcessAsync(core);
        }
    }
}
