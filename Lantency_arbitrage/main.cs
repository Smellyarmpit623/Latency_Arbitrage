using System;
using TradingAPI.MT4Server;
using Com.Lmax.Api;
using Com.Lmax.Api.OrderBook;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using IronOcr;
using System.Linq;
using System.Runtime.InteropServices;
using Point = System.Drawing.Point;
using System.Drawing;
using System.Configuration;

using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Text;
using System.Security.Permissions;



namespace Lentency_arbitrage
{
    class Program
    {
        private ISession _session;
        private const long XAUUSD_INSTRUMENT_ID = 4017; //USDJPY
        decimal max=0;
        
        public void MarketDataUpdate(OrderBookEvent orderBookEvent)
        {
            decimal bestBid = GetBestPrice(orderBookEvent.BidPrices);
            decimal bestAsk = GetBestPrice(orderBookEvent.AskPrices);
            //Console.WriteLine("Market data: {0}", orderBookEvent);
            //Console.WriteLine("Quote of the best price: {0} {1}", (bestAsk),bestBid);
            var Result = new IronTesseract().Read(new Program().GetSreenshot()).Text;
            decimal Quotation_lmax = bestBid;
            decimal option_quote=0;
            try
            {
                option_quote = Convert.ToDecimal(Result);
                Console.WriteLine("Quote of the Lmax and option: {0} {1}", (Quotation_lmax), option_quote);
                decimal Difference=(Quotation_lmax*100000)-(option_quote*100000);
                if (Difference <= 0) { 
                    Difference = Difference * -1;
                    if(Difference <=10)
                    SendKeys.SendWait("+{S}");
                }
                else if(Difference>=10)
                    SendKeys.SendWait("+{W}");
                Console.WriteLine("Difference= {0}",Difference);
                if (max <= Difference) max = Difference;
                Console.WriteLine(max);
                /*using (StreamWriter sw = File.AppendText(@"D:\\data.txt"))
                {
                    sw.WriteLine("Fast Quote={0}  Slow Quote={1}  Difference={2}  Max_Difference={3}", Quotation_lmax, option_quote, Difference,max);
                    sw.Flush();
                    sw.Close();
                }
                using (StreamWriter sw = File.AppendText(@"D:\\Lmax.txt"))
                {
                    sw.WriteLine(Quotation_lmax);
                    sw.Flush();
                    sw.Close();
                }
                using (StreamWriter sw = File.AppendText(@"D:\\Slow_Quote.txt"))
                {
                    sw.WriteLine(option_quote);
                    sw.Flush();
                    sw.Close();
                }
                using (StreamWriter sw = File.AppendText(@"D:\\Difference.txt"))
                {
                    sw.WriteLine(Difference);
                    sw.Flush();
                    sw.Close();
                }
                using (StreamWriter sw = File.AppendText(@"D:\\Max.txt"))
                {
                    sw.WriteLine(max);
                    sw.Flush();
                    sw.Close();
                }
                */
            }
            catch (Exception)
            {
                Console.WriteLine("Cant convert");
            }
            
        }

        static void Main(string[] args)
        {
            
            
            new Program().Lmax();
            //Console.ReadKey();




            //new Program().main_loop();
        }




        private Bitmap GetSreenshot()
        {
           
            int x, y, dx, dy;
            x = 1485;
            y = 705;
            dx = 1579;
            dy = 731;

            Bitmap bm = new Bitmap(dx-x, dy-y);
            Graphics g = Graphics.FromImage(bm);
            g.CopyFromScreen(x, y, 0, 0, bm.Size);
            bm.Save(@"C:\Users\Steve\OneDrive - St Pauls School\Documents\GitHub\Lantency_Arbitrage\Lantency_arbitrage\test.png");
            return bm;

        }
        void main_loop()
        {

            Server[] slaves;
            MainServer primary = QuoteClient.LoadSrv(
            @"C:\Users\13616\OneDrive - Queensland University of Technology\Documents\coding\Lantency_arbitrage\Lantency_arbitrage\Server_config\DooPrime-Demo.srv", out slaves);
            QuoteClient qc = Connect(primary, slaves, 112351730, "SW1024sw");
            Console.WriteLine("MT4 connection established");
            
            
            qc.OnQuote += new QuoteEventHandler(qc_OnQuote);
            qc.Subscribe("XAUUSD");
            Console.ReadKey();                
            
            
            
            


        }


        public void LoginCallback(ISession session)
        {
            
            Console.WriteLine("Logged in, account ID: " + session.AccountDetails.AccountId);
            _session = session;
            session.MarketDataChanged += MarketDataUpdate;

            session.Subscribe(new OrderBookSubscriptionRequest(XAUUSD_INSTRUMENT_ID),
                    () => Console.WriteLine("Successful subscription"),
                    failureResponse => Console.Error.WriteLine("Failed to subscribe: {0}", failureResponse));

            session.Start();
            /*
            string query = "CURRENCY"; // see above for how to do a more specific search
            long offsetInstrumentId = 0; // see above for more details on this offset parameter

            _session = session;

            session.SearchInstruments(new SearchRequest(query, offsetInstrumentId), SearchCallback,
                failureResponse => Console.Error.WriteLine("Failed to subscribe: {0}", failureResponse));// market info

            _session.Start();
            */
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

            Console.WriteLine(args.Symbol + " " + args.Bid + " " + args.Ask +" "+(100*(Convert.ToDouble(args.Ask)-Convert.ToDouble(args.Bid))));
        }

    }
}


    