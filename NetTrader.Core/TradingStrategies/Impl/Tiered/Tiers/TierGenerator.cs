/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Collections.Generic;
using NetTrader.Core.Database.Table;

namespace NetTrader.Core.TradingStrategies.Impl.Tiers
{
    public class TierGenerator
    {
        public Tier[] GenerateTiers(float basePrice, float fiatFunds)
        {
            float unit = 0.05f;

            float currencyAmount = fiatFunds / basePrice;

            int floors = 2;

            List<Tier> tierList = new List<Tier>();

            int k = 0;
            for (int i = 0; i < floors; i++)
            {
                int tierCount = (int)(Math.Round(currencyAmount / unit) / floors);

                for (int h = 1; h < tierCount+1; h++)
                {
                    float offset = 7f;

                    float d = ((float)h / (float)tierCount);

                    if (d < 0.3f)
                        d = 0.3f;

                    float p = ((d + 0.3f) * 1f) * d * i;

                    if (p > 1f)
                        p = 1f;

                    //Console.WriteLine(p.ToString());

                    var profit = h * offset * p;

                    if (profit < 1f)
                        profit = 1;

                    var tier = new Tier()
                    {
                        Id = k++,
                        CurrencyAmount = unit,
                        BuyOffset = k * offset,
                        SellProfit = profit
                    };

                    tierList.Add(tier);
                }
            }


            foreach (var t in tierList)
            {
                float buyPrice = t.GetBuyPrice(basePrice);
                float sellPrice = t.GetSellPrice(t.GetBuyPrice(basePrice));

                Console.WriteLine(t.ToString() + "\nbuy price: " + buyPrice + ", sell price " + sellPrice + ", profit=" + ((sellPrice - buyPrice) * t.CurrencyAmount).ToString("F"));
            }

            return tierList.ToArray();
        }


        // stare metody
        // ETH - ideal
        /*
        float multiplier = 50f;
        float buyMultiplier = 2f;
        float maxBuyMultiplier = 1.8f;
        float sellMultiplier = 1.8f;
        core.Tiers = new Tier[]
        {
            new Tier()
            {
                Id = i++,
                CurrencyAmount = 0.01f * multiplier,
                BuyOffset = i * buyMultiplier,
                SellProfit = i * sellMultiplier * 0.8f
            },
            new Tier()
            {
                Id = i++,
                CurrencyAmount = 0.01f * multiplier,
                BuyOffset = i * buyMultiplier,
                SellProfit = i * sellMultiplier * 0.76f
            },
            new Tier()
            {
                Id = i++,
                CurrencyAmount = 0.01f * multiplier,
                BuyOffset = i * buyMultiplier,
                SellProfit = i * sellMultiplier * 0.74f
            },
            new Tier()
            {
                Id = i++,
                CurrencyAmount = 0.01f * multiplier,
                BuyOffset = i * buyMultiplier * maxBuyMultiplier * 1.0f,
                SellProfit = i * sellMultiplier * 1.0f
            },
            new Tier()
            {
                Id = i++,
                CurrencyAmount = 0.01f * multiplier,
                BuyOffset = i * buyMultiplier * maxBuyMultiplier * 1.1f,
                SellProfit = i * sellMultiplier * 1.1f
            },
            new Tier()
            {
                Id = i++,
                CurrencyAmount = 0.01f * multiplier,
                BuyOffset = i * buyMultiplier * maxBuyMultiplier * 1.2f,
                SellProfit = i * sellMultiplier * 1.2f
            },
            new Tier()
            {
                Id = i++,
                CurrencyAmount = 0.01f * multiplier,
                BuyOffset = i * buyMultiplier * maxBuyMultiplier * 1.3f,
                SellProfit = i * sellMultiplier * 1.3f
            },
            new Tier()
            {
                Id = i++,
                CurrencyAmount = 0.01f * multiplier,
                BuyOffset = i * buyMultiplier * maxBuyMultiplier * 1.4f,
                SellProfit = i * sellMultiplier * 1.4f
            },
            new Tier()
            {
                Id = i++,
                CurrencyAmount = 0.01f * multiplier,
                BuyOffset = i * buyMultiplier * maxBuyMultiplier * 1.5f,
                SellProfit = i * sellMultiplier * 1.5f
            },
            new Tier()
            {
                Id = i++,
                CurrencyAmount = 0.01f * multiplier,
                BuyOffset = i * buyMultiplier * maxBuyMultiplier * 1.6f,
                SellProfit = i * sellMultiplier* 1.6f
            },
            new Tier()
            {
                Id = i++,
                CurrencyAmount = 0.01f * multiplier,
                BuyOffset = i * buyMultiplier * maxBuyMultiplier * 1.7f,
                SellProfit = i * sellMultiplier * 1.7f
            },
            new Tier()
            {
                Id = i++,
                CurrencyAmount = 0.01f * multiplier,
                BuyOffset = i * buyMultiplier * maxBuyMultiplier * 1.8f,
                SellProfit = i * sellMultiplier * 1.75f
            },
            new Tier()
            {
                Id = i++,
                CurrencyAmount = 0.01f * multiplier,
                BuyOffset = i * buyMultiplier * maxBuyMultiplier * 1.9f,
                SellProfit = i * sellMultiplier * 1.8f
            }
        };
        */

        /*
        // BTC - ideal
        float multiplier = 3f;
        float buyMultiplier = 15.5f;
        float maxBuyMultiplier = 0.8f;
        float sellMultiplier = 6f;
        core.Tiers = new Tier[]
        {
            new Tier()
            {
                Id = i++,
                CurrencyAmount = 0.01f * multiplier,
                BuyOffset = i * buyMultiplier * 1f,
                SellProfit = i * sellMultiplier * 0.9f
            },
            new Tier()
            {
                Id = i++,
                CurrencyAmount = 0.01f * multiplier,
                BuyOffset = i * buyMultiplier * 1.1f,
                SellProfit = i * sellMultiplier * 0.9f
            },
            new Tier()
            {
                Id = i++,
                CurrencyAmount = 0.01f * multiplier,
                BuyOffset = i * buyMultiplier * 1.2f,
                SellProfit = i * sellMultiplier * 0.9f
            },
            new Tier()
            {
                Id = i++,
                CurrencyAmount = 0.01f * multiplier,
                BuyOffset = i * buyMultiplier * 1.3f,
                SellProfit = i * sellMultiplier * 0.9f
            },
            new Tier()
            {
                Id = i++,
                CurrencyAmount = 0.01f * multiplier,
                BuyOffset = i * buyMultiplier * 1.4f,
                SellProfit = i * sellMultiplier * 0.9f
            },
            new Tier()
            {
                Id = i++,
                CurrencyAmount = 0.01f * multiplier,
                BuyOffset = i * buyMultiplier * 1.5f,
                SellProfit = i * sellMultiplier * 0.88f
            },
            new Tier()
            {
                Id = i++,
                CurrencyAmount = 0.01f * multiplier,
                BuyOffset = i * buyMultiplier * 1.6f,
                SellProfit = i * sellMultiplier * 0.86f
            },
            new Tier()
            {
                Id = i++,
                CurrencyAmount = 0.01f * multiplier,
                BuyOffset = i * buyMultiplier * 1.7f * maxBuyMultiplier,
                SellProfit = i * sellMultiplier * 0.84f
            },
            new Tier()
            {
                Id = i++,
                CurrencyAmount = 0.01f * multiplier,
                BuyOffset = i * buyMultiplier * 1.8f * maxBuyMultiplier * 1.0f,
                SellProfit = i * sellMultiplier * 0.82f
            },
            new Tier()
            {
                Id = i++,
                CurrencyAmount = 0.01f * multiplier,
                BuyOffset = i * buyMultiplier * 1.9f * maxBuyMultiplier * 1.1f,
                SellProfit = i * sellMultiplier * 0.80f
            },
            new Tier()
            {
                Id = i++,
                CurrencyAmount = 0.01f * multiplier,
                BuyOffset = i * buyMultiplier * 2f * maxBuyMultiplier * 1.2f,
                SellProfit = i * sellMultiplier * 0.78f
            },
            new Tier()
            {
                Id = i++,
                CurrencyAmount = 0.01f * multiplier,
                BuyOffset = i * buyMultiplier * 2.1f * maxBuyMultiplier * 1.3f,
                SellProfit = i * sellMultiplier * 0.76f
            },
            new Tier()
            {
                Id = i++,
                CurrencyAmount = 0.01f * multiplier,
                BuyOffset = i * buyMultiplier * 2.2f * maxBuyMultiplier * 1.4f,
                SellProfit = i * sellMultiplier * 0.74f
            }
        };
        */
    }
}
