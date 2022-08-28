/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.IO;
using System.Security;
using Newtonsoft.Json;
using BinanceAPI = BinanceExchange.API.Client;

namespace NetTrader.Core
{
    public class Settings
    {
        public static Settings Instance { get; private set; }

        public class GenericSettings
        {
            public string DatabasePath;

            public string ServerDomain;
            public int ServerPort;

            public string CertificatePath;
            public string CertificatePassword;

            public string UserLogin;
            public string UserPassword;

            public bool IsSSLEnabled;

            public Gateways.Exchange Exchange { get; set; }

            public float Budget { get; set; }
            public string CurrencyPair { get; set; }

            public float DefaultTargetBasePrice { get; set; }
            public float TBPRecalculationOffset { get; set; }
            public float TBPRecalculationPriceOffset { get; set; }
            public float StopLossThreshold { get; set; }
            public float StopLossBasePriceOffset { get; set; }
        }

        public class GDAXExchange
        {

            public string ApiKey { get; set; }
            public string Passphrase { get; set; }
            public string Secret { get; set; }

            public GDAX.NET.RequestAuthenticator RequestAuthenticator { get; set; }

            public void CreateAuthenticator()
            {
                if (!string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(Passphrase) && !string.IsNullOrEmpty(Secret))
                    RequestAuthenticator = new GDAX.NET.RequestAuthenticator(ApiKey, Passphrase, Secret);
            }
        }


        public class BinanceExchange
        {
            public string ApiKey { get; set; }
            public string SecretKey { get; set; }

            public BinanceAPI.ClientConfiguration RequestAuthenticator { get; set; }

            public void CreateAuthenticator()
            {
                if (!string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(SecretKey))
                {
                    RequestAuthenticator = new BinanceAPI.ClientConfiguration()
                    {
                        ApiKey = ApiKey,
                        SecretKey = SecretKey,
                    };
                }
            }
        }

        public readonly GenericSettings Generic = new GenericSettings();

        public readonly GDAXExchange GDAX = new GDAXExchange();

        public readonly BinanceExchange Binance = new BinanceExchange();

        public static void Load(string filePath)
        {
            using (StreamReader file = File.OpenText(filePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                Settings settings = (Settings)serializer.Deserialize(file, typeof(Settings));

                if (settings == null)
                    throw new InvalidOperationException("Failed to load settings");

                settings.GDAX.CreateAuthenticator();
                settings.Binance.CreateAuthenticator();

                Instance = settings;
            }
        }

    }
}
