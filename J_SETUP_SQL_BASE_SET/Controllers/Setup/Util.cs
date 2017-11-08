using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace J_SETUP_SQL.Controllers.Setup
{
    public class Util
    {

        /// <summary>
        /// Parse Int
        /// </summary>
        /// <param name="s">get data</param>
        /// <returns>return integer</returns>
        public static int Cint(string s)
        {
            int i;
            if (int.TryParse(s, out i)) return i;
            return 0;
        }

    }
}
