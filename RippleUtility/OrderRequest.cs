using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jayrock.Json;
using Jayrock.JsonRpc;
using WebSocket4Net;

namespace RippleUtility
{
    public class OrderRequest
    {
        public static void submitOrder(WebSocket wsClient,
            string buyCCY, string buyIsser, double buyValue,
            string sellCCY, string sellIsser, double sellValue)
        {
            string req = WSRequest.getRequest("submit", buyCCY + sellCCY, JsonRequest.getSubmitJsonRequest(
               RConfig.rippleAddress, RConfig.rippleSecret, sellCCY, sellIsser, sellValue, buyCCY, buyIsser, buyValue));

            wsClient.Send(req);
        }


    }
}
