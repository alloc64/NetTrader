/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetTrader.Core.Database.Table;
using NetTrader.Core.Gateways;
using NetTrader.Core.Pipe.Messages;
using NetTrader.Core.Utils;

namespace NetTrader.Core.StopLoss
{
    public class StopLossProcessor
    {
        //

        private TraderCore core;

        private bool panicModeTriggered = false;

        private long panicTimestamp = -1;

        //

        public StopLossProcessor(TraderCore core)
        {
            this.core = core;
        }

        public async Task ProcessMatchTrade(ExchangeMatch currentMatch, ExchangeMatch lastMatch)
        {
            if (lastMatch == null)
                return;
            
            if(panicModeTriggered)
            {
                ProcessPanicMode(currentMatch);
                return;
            }

            var lastMatchDifference = lastMatch.Price - currentMatch.Price;

            //Console.WriteLine("lastMatchDifference " + lastMatchDifference);

            if (lastMatchDifference > core.StopLossThreshold)
            {
                Console.WriteLine("Huge sell off occured, but not triggered panic stop loss.");
                panicTimestamp = -1;
                return;
            }

            var tbpDifference = currentMatch.Price - core.BasePrice;

            //Console.WriteLine("tbpDifference " + tbpDifference);

            if (-tbpDifference > core.StopLossThreshold)
            {
                var openedSellOrders = core.Database.GetOpenOrdersOfType(Database.Table.TradeOrder.Type.Sell);

                var openOrdersCnt = openedSellOrders.Count();

                if (openOrdersCnt > 0)
                {
                    if (panicTimestamp < 0)
                    {
                        Console.WriteLine("We have " + openOrdersCnt + " open sell orders, preparing for panic mode.");
                        panicTimestamp = DateTimeUtils.Timestamp;
                    }
                }
                else
                {
                    Console.WriteLine("Huge sell off occured, but no sell orders, panic mode is not needed...");
                }
            }
            else
                panicTimestamp = -1;

            if(panicTimestamp > 0 && DateTimeUtils.Timestamp - panicTimestamp > 15f && !panicModeTriggered)
            {
                await TriggerStopLoss(currentMatch);
            }
        }

        public async Task TriggerStopLoss(ExchangeMatch currentMatch)
        {
            this.panicModeTriggered = true;

            float sellOffPrice = (float)Math.Round(currentMatch.Price + 0.11f, 2);

            Console.WriteLine("Panic mode triggered, selling all coins under price " + sellOffPrice);

            var database = core.Database;

            var openedSellOrders = database.GetOpenOrdersOfType(Database.Table.TradeOrder.Type.Sell);

            var clonedOpenedSellOrders = new List<TradeOrder>(openedSellOrders);

            await new CancelOpenedOrdersMessage().ProcessAsync(core);

            // zde pamatovat, ze orders jsou jiz v DB zrusene a toto je pouze zustatek v pameti
            // zaroven by se to dalo prodat najednou, bohuzel pak nezjistime jaky byly ztraty
            foreach (var order in clonedOpenedSellOrders)
            {
                var parentBuyOrder = database.GetOrder(order.ParentOrderId);

                if (parentBuyOrder == null)
                    Console.WriteLine("parentBuyOrder for order " + order.OrderId + " was not found, profits will not match");

                await new SellCurrencyAtPrice(core.CurrencyPair, sellOffPrice, order.Amount, parentBuyOrder).ProcessAsync(core);
            }
        }

        private void ProcessPanicMode(ExchangeMatch currentMatch)
        {
            var openedSellOrders = core.Database.GetOpenOrdersOfType(Database.Table.TradeOrder.Type.Sell);

            var openOrdersCnt = openedSellOrders.Count();

            if(openOrdersCnt > 0)
            {
                //Console.WriteLine("Still have " + openOrdersCnt  + " open orders. Cannot deactivate panic mode.");
                return;
            }
            else
            {
                Console.WriteLine("No open sell orders found, killing panic mode.");
            }

            panicModeTriggered = false;
            panicTimestamp = -1;

            // ukonci se panic mode
            // objednavky, pokud nejake jsou se zrusi v nasledujicich operacich

            core.TradingPipe.SendMessage(new SetTargetBasePriceMessage((float)Math.Round(currentMatch.Price - core.StopLossBasePriceOffset)));
        }
    }
}
