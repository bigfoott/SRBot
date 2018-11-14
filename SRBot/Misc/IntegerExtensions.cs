using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRBot.Misc
{
    public static class IntegerExtensions
    {
        public static string WithSuffix(this int num)
        {
            if (num.ToString().EndsWith("11")
                || num.ToString().EndsWith("12")
                || num.ToString().EndsWith("13")) return num.ToString() + "th";
            if (num.ToString().EndsWith("1")) return num + "st";
            if (num.ToString().EndsWith("2")) return num + "nd";
            if (num.ToString().EndsWith("3")) return num + "rd";
            return num + "th";
        }
    }
}
