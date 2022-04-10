using System;
using TradingAPI.MT4Server;
using Com.Lmax.Api;
using Com.Lmax.Api.OrderBook;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;


namespace Lentency_arbitrage
{
    class Program
    {
        private ISession _session;
        private const long GBP_USD_INSTRUMENT_ID = 4001;

        public void MarketDataUpdate(OrderBookEvent orderBookEvent)
        {
            Console.WriteLine("Market data: {0}", orderBookEvent);
        }

        static void Main(string[] args)
        {

            new Program().Lmax();
            //new Program().main_loop();
        }
        
        void main_loop()
        {

            Server[] slaves;
            MainServer primary = QuoteClient.LoadSrv(
            @"C:\Users\13616\OneDrive - Queensland University of Technology\Documents\coding\Lantency_arbitrage\Lantency_arbitrage\Server_config\DooPrime-Demo.srv", out slaves);
            QuoteClient qc = Connect(primary, slaves, 112351730, "SW1024sw");
            Console.WriteLine("MT4 connection established");
            
            
            qc.OnQuote += new QuoteEventHandler(qc_OnQuote);
            qc.Subscribe("EURUSD");
            Console.ReadKey();                
            
            
            
            


        }


        public void LoginCallback(ISession session)
        {
            /*
            Console.WriteLine("Logged in, account ID: " + session.AccountDetails.AccountId);
            _session = session;
            session.MarketDataChanged += MarketDataUpdate;

            session.Subscribe(new OrderBookSubscriptionRequest(GBP_USD_INSTRUMENT_ID),
                    () => Console.WriteLine("Successful subscription"),
                    failureResponse => Console.Error.WriteLine("Failed to subscribe: {0}", failureResponse));

            session.Start();*/

            string query = "CURRENCY"; // see above for how to do a more specific search
            long offsetInstrumentId = 0; // see above for more details on this offset parameter

            _session = session;

            session.SearchInstruments(new SearchRequest(query, offsetInstrumentId), SearchCallback,
                failureResponse => Console.Error.WriteLine("Failed to subscribe: {0}", failureResponse));

            _session.Start();
            //    decimal bestBid = GetBestPrice(orderBookEvent.BidPrices);
            //    decimal bestAsk = GetBestPrice(orderBookEvent.AskPrices);


        }
        private void SearchCallback(List<Instrument> instruments, bool hasMoreResults)
        {
            Console.WriteLine("Instruments Retrieved: {0}", instruments.Count);
            for(int i = 0; i <= 24; i++)
            {
                Console.WriteLine(instruments[i]);
            }
            
            
            /*while (hasMoreResults)
            {
                Console.WriteLine("To continue retrieving all instruments please start next search from: {0}", instruments[(instruments.Count - 1)]);
            }*/
        }

        private static OnFailure FailureCallback(string failedFunction)
        {
            return failureResponse => Console.Error.WriteLine("Failed to " + failedFunction + " due to: " + failureResponse.Message);
        }

        void Lmax()
        {
            Program Lmax = new Program();
            LmaxApi lmaxApi = new LmaxApi("https://trade.lmaxtrader.com");
            lmaxApi.Login(new LoginRequest("zyc7878", "Q010151q"), Lmax.LoginCallback, FailureCallback("log in"));

        }
        private static decimal GetBestPrice(List<PricePoint> prices)
        {
            return prices.Count != 0 ? prices[0].Price : 0m;
        }


        QuoteClient Connect(MainServer primary, Server[] slaves, int user, string password)
        {
            Console.WriteLine("Connecting...");
            QuoteClient qc = new QuoteClient(user, password, primary.Host, primary.Port);
            try
            {
                qc.Connect();
                return qc;
            }
            catch (Exception)
            {
                Console.WriteLine("Cannot connect to main server");
                return ConnectSlaves(slaves, user, password);
            }
        }
        QuoteClient ConnectSlaves(Server[] slaves, int user, string password)
        {
            Console.WriteLine("Connecting to slaves...");
            foreach (var server in slaves)
            {
                QuoteClient qc = new QuoteClient(user, password, server.Host, server.Port);
                try
                {
                    qc.Connect();
                    return qc;
                }
                catch (Exception)
                {
                }
            }
            throw new Exception("Cannot connect to slaves");
        }
        void qc_OnQuote(object sender, QuoteEventArgs args)
        {

            Console.WriteLine(args.Symbol + " " + args.Bid + " " + args.Ask +" "+(100000*(Convert.ToDouble(args.Ask)-Convert.ToDouble(args.Bid))));
        }

    }
}


