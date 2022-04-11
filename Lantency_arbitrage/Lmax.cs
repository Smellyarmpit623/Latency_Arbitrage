using System;
using TradingAPI.MT4Server;
using Com.Lmax.Api;
using Com.Lmax.Api.OrderBook;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace LmaxLmax
{ 
    class Lmax_moudle
    {
        private ISession _session;
        private const long GBP_USD_INSTRUMENT_ID = 100637;

        public void MarketDataUpdate(OrderBookEvent orderBookEvent)
        {
            Console.WriteLine("Market data: {0}", orderBookEvent);
        }
        public void nigga()
        {


        }
    }
}