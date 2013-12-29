using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jayrock.Json;
using Jayrock.JsonRpc;

namespace RippleUtility
{
    public class WSRequest
    {

        public static string getRequest(string methodName, string ID, JsonObject args)
        {
            JsonObject call = args;
            call["id"] = methodName + ID;
            call["command"] = methodName;

            return call.ToString();

        }
    }
}
