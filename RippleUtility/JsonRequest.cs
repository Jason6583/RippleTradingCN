using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jayrock.Json;
using Jayrock.JsonRpc;

namespace RippleUtility
{
    public class JsonRequest
    {

        public static string getResponse(string methodName, JsonRpcClient client, JsonObject args)
        {
            return client.InvokeVargs(methodName, args).ToString();
        }

        public static JsonObject getServerInfoJsonRequest()
        {
            return new JsonObject { };
        }

        public static JsonObject getAccountInfoJsonRequest(string account)
        {
            return new JsonObject { { "account", account } };
        }

        public static JsonObject getAccountLinesJsonRequest(string account)
        {
            return new JsonObject { { "account", account } };
        }

        public static JsonObject getAccountOffersJsonRequest(string account)
        {
            return new JsonObject { { "account", account } };
        }

        public static JsonObject getAccountTXJsonRequest(string account)
        {
            return new JsonObject { { "account", account }, { "ledger_index_min", -1 }, { "ledger_index_max", -1 } };
        }

        public static JsonObject getBookOffersJsonRequest(string ccyTakerGet, string issuerTakerGet, string ccyTakerPay, string issuerTakerPay, int limit = 10)
        {
            JsonObject oTakerGets;
            JsonObject oTakerPays;

            if (ccyTakerGet.Equals("XRP"))
            {
                oTakerGets = new JsonObject { { "currency", ccyTakerGet } };
            }
            else
            {
                oTakerGets = new JsonObject { { "currency", ccyTakerGet }, { "issuer", issuerTakerGet } };
            }

            if (ccyTakerPay.Equals("XRP"))
            {
                oTakerPays = new JsonObject { { "currency", ccyTakerPay } };
            }
            else
            {
                oTakerPays = new JsonObject { { "currency", ccyTakerPay }, { "issuer", issuerTakerPay } };
            }

            return new JsonObject { { "taker_gets", oTakerGets }, { "taker_pays", oTakerPays }, { "limit", limit } };
        }


        public static JsonObject getSubmitJsonRequest(string account, string secret,
            string ccyTakerGet, string issuerTakerGet, double valueTakerGet,
            string ccyTakerPay, string issuerTakerPay, double valueTakerPay)
        {
            JsonObject oTakerGets;
            JsonObject oTakerPays;

            oTakerGets = new JsonObject { { "currency", ccyTakerGet }, { "issuer", issuerTakerGet }, { "value", valueTakerGet.ToString() } };
            oTakerPays = new JsonObject { { "currency", ccyTakerPay }, { "issuer", issuerTakerPay }, { "value", valueTakerPay.ToString() } };

            if (ccyTakerGet.Equals("XRP"))
            {
                return new JsonObject { 
                {"tx_json",new JsonObject{{ "TransactionType", "OfferCreate" }, { "Account", account }, { "TakerGets", valueTakerGet }, { "TakerPays", oTakerPays } }},
                {"secret", secret}};
            }
            else if (ccyTakerPay.Equals("XRP"))
            {
                return new JsonObject { 
                {"tx_json",new JsonObject{{ "TransactionType", "OfferCreate" }, { "Account", account }, { "TakerGets", oTakerGets }, { "TakerPays", valueTakerPay } }},
                {"secret", secret}};
            }
            else
            {
                return new JsonObject { 
                {"tx_json",new JsonObject{{ "TransactionType", "OfferCreate" }, { "Account", account }, { "TakerGets", oTakerGets }, { "TakerPays", oTakerPays } }},
                {"secret", secret}};
            }
        }

    }
}
