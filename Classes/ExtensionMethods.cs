using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qbBot.Classes
{
    internal static class ExtensionMethods
    {
        public static string Shorten(this string startString, int maxLength)
        {
            if (startString.Length <= maxLength)
                return startString;
            return startString.Substring(0, maxLength - 1);
        }
    }
}
