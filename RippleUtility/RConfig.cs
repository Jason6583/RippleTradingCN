using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace RippleUtility
{
    public class RConfig
    {
        public static double MILLION = 1000000.0;
        public static string gwRippleCN = "rnuF96W4SZoCJmbHYBFoJZpR8eCaxNvekK";
        public static string gwBitstamp = "rvYAfWj5gh67oV6fW32ZzP3Aw4Eubs59B";
        public static string gwRippleChina = "";
        public static string gwCNYTrade = "";
        public static string WSServer1 = "wss://s1.ripple.com";

        public static double adj = 0.000000000001;

        public static string rippleAddress = ConfigurationManager.AppSettings["rippleAddress"].Trim();
        public static string rippleSecret = ConfigurationManager.AppSettings["rippleSecret"].Trim();

    }
}
