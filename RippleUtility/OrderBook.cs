using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jayrock.Json;
using Jayrock.JsonRpc;
using RippleUtility;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using WebSocket4Net;

namespace RippleTrading
{
    public class OrderBook
    {

        public string CCY1 { get; set; }
        public string CCY2 { get; set; }
        public string issuer1 { get; set; }
        public string issuer2 { get; set; }
        public DS.BidDataTable dtBid { get; set; }
        public DS.AskDataTable dtAsk { get; set; }
        public DS.BidAskDataTable dtBidAsk { get; set; }
        public WebSocket wsClient;
        public int bookID;

        public enum STATE { PROCESSING, READY };
        public STATE bidState { get; set; }
        public STATE askState { get; set; }

        public double bestBid { get; set; }
        public double bestBidQty { get; set; }
        public double bestAsk { get; set; }
        public double bestAskQty { get; set; }

        public OrderBook(WebSocket wsClient, int bookID, string CCY1, string CCY2, string issuer1, string issuer2)
        {
            this.wsClient = wsClient;
            this.bookID = bookID;

            this.CCY1 = CCY1;
            this.CCY2 = CCY2;
            this.issuer1 = issuer1;
            this.issuer2 = issuer2;
        }

        public bool isReady()
        {
            return (bidState == STATE.READY && askState == STATE.READY);
        }

        public void fetchBook(int limit)
        {
            bidState = STATE.PROCESSING;
            string bidReq = WSRequest.getRequest("book_offers", "bid" + bookID,
                JsonRequest.getBookOffersJsonRequest(CCY2, issuer2, CCY1, issuer1, limit));
            wsClient.Send(bidReq);

            askState = STATE.PROCESSING;
            string askReq = WSRequest.getRequest("book_offers", "ask" + bookID,
                JsonRequest.getBookOffersJsonRequest(CCY1, issuer1, CCY2, issuer2, limit));
            wsClient.Send(askReq);

        }

        public void fetchBookBid(JToken msg)
        {
            JObject[] arrBid = msg["offers"].ToObject<JObject[]>();
            dtBid = new DS.BidDataTable();

            if (CCY1 == "XRP" || CCY2 == "XRP")
            {
                foreach (JObject j in arrBid)
                {
                    if (j["taker_pays_funded"] == null)
                    {
                        dtBid.AddBidRow(RConfig.MILLION / j["quality"].ToObject<double>(), j["TakerPays"].ToObject<double>() / RConfig.MILLION);
                    }
                    else
                    {
                        dtBid.AddBidRow(RConfig.MILLION / j["quality"].ToObject<double>(), j["taker_pays_funded"].ToObject<double>() / RConfig.MILLION);
                    }
                }
            }
            else
            {
                foreach (JObject j in arrBid)
                {
                    if (j["taker_pays_funded"] == null)
                    {
                        dtBid.AddBidRow(1.0 / j["quality"].ToObject<double>(), j["TakerPays"]["value"].ToObject<double>());
                    }
                    else
                    {
                        dtBid.AddBidRow(1.0 / j["quality"].ToObject<double>(), j["taker_pays_funded"]["value"].ToObject<double>());
                    }
                }

            }

            bestBid = dtBid[0].Bid;
            bestBidQty = dtBid[0].BidQty;

            bidState = STATE.READY;
        }



        public void fetchBookAsk(JToken msg)
        {
            JObject[] arrAsk = msg["offers"].ToObject<JObject[]>();
            dtAsk = new DS.AskDataTable();

            if (CCY1 == "XRP" || CCY2 == "XRP")
            {
                foreach (JObject j in arrAsk)
                {
                    if (j["taker_gets_funded"] == null)
                    {
                        dtAsk.AddAskRow(RConfig.MILLION * j["quality"].ToObject<double>(), j["TakerGets"].ToObject<double>() / RConfig.MILLION);
                    }
                    else
                    {
                        dtAsk.AddAskRow(RConfig.MILLION * j["quality"].ToObject<double>(), j["taker_gets_funded"].ToObject<double>() / RConfig.MILLION);
                    }
                }
            }
            else
            {
                foreach (JObject j in arrAsk)
                {
                    if (j["taker_gets_funded"] == null)
                    {
                        dtAsk.AddAskRow(j["quality"].ToObject<double>(), j["TakerGets"]["value"].ToObject<double>());
                    }
                    else
                    {
                        dtAsk.AddAskRow(j["quality"].ToObject<double>(), j["taker_gets_funded"]["value"].ToObject<double>());
                    }
                }

            }

            bestAsk = dtAsk[0].Ask;
            bestAskQty = dtAsk[0].AskQty;

            askState = STATE.READY;
        }

    }
}
