using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jayrock.Json;
using Jayrock.JsonRpc;
using RippleUtility;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using WebSocket4Net;

namespace RippleTrading
{
    public class OfferBook
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

        public OfferBook(WebSocket wsClient, int bookID, string CCY1, string CCY2, string issuer1, string issuer2)
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

        public void fetchBook(JToken msg)
        {
            bidState = STATE.PROCESSING;
            askState = STATE.PROCESSING;

            JObject[] arrOffer = msg["offers"].ToObject<JObject[]>();
            dtBid = new DS.BidDataTable();
            dtAsk = new DS.AskDataTable();

            foreach (JObject j in arrOffer)
            {

                double takerPaysXRP;
                double takerGetsXRP;
                string takerPayCCY;
                string takerGetCCY;

                if (Double.TryParse(j["taker_pays"].ToString(), out takerPaysXRP))
                {
                    takerGetCCY = j["taker_gets"]["currency"].ToString();
                    if (CCY1 =="XRP" && takerGetCCY == CCY2)
                    {
                        dtBid.AddBidRow(RConfig.MILLION * j["taker_gets"]["value"].ToObject<double>() / takerPaysXRP,
                           takerPaysXRP / RConfig.MILLION);
                    }
                }
                else if (Double.TryParse(j["taker_gets"].ToString(), out takerGetsXRP))
                {
                    takerPayCCY = j["taker_pays"]["currency"].ToString();
                    if (CCY1 == "XRP" && takerPayCCY == CCY2)
                    dtAsk.AddAskRow(RConfig.MILLION * j["taker_pays"]["value"].ToObject<double>() / takerGetsXRP,
                       takerGetsXRP / RConfig.MILLION);
                }else{

                     takerPayCCY = j["taker_pays"]["currency"].ToString();
                     takerGetCCY = j["taker_gets"]["currency"].ToString();
                     if (takerPayCCY == CCY1 && takerGetCCY == CCY2)
                    {
                        dtBid.AddBidRow(j["taker_gets"]["value"].ToObject<double>() / j["taker_pays"]["value"].ToObject<double>(),
                           j["taker_pays"]["value"].ToObject<double>());
                    }
                     else if (takerPayCCY == CCY2 && takerGetCCY == CCY1)
                    {
                        dtAsk.AddAskRow(j["taker_pays"]["value"].ToObject<double>() / j["taker_gets"]["value"].ToObject<double>(),
                           j["taker_gets"]["value"].ToObject<double>());
                    }
                }
            }
            
            bidState = STATE.READY;
            askState = STATE.READY;
        }
    }
}
