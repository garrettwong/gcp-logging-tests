using System;
using System.Collections.Generic;
using System.Text;

namespace gcp_logging_tests.Utilities
{
    public class SSNGenerator
    {
        public SSNGenerator()
        {
        }

        public void GetSSNs(List<string> curStrs, StringBuilder cur)
        {
            //const int NORMAL_LEN = 11;
            const int NORMAL_LEN = 9;
            if (cur.Length >= NORMAL_LEN)
            {
                curStrs.Add(cur.ToString());
                return;
            }

            if (cur.Length == 3 || cur.Length == 6)
            {
                // add a dash
                cur.Append('-');
            }

            var ALPHA = "0123456789";

            foreach (var c in ALPHA)
            {
                var newSb = new StringBuilder(cur.ToString());
                newSb.Append(c);
                GetSSNs(curStrs, newSb);
            }
        }

        public void GetSSNs(List<string> curStrs, string cur)
        {
            //const int NORMAL_LEN = 11;
            const int NORMAL_LEN = 9;
            if (cur.Length >= NORMAL_LEN)
            {
                curStrs.Add(cur);
                return;
            }

            if (cur.Length == 3 || cur.Length == 6)
            {
                // add a dash
                cur += "-";
            }

            var ALPHA = "0123456789";

            foreach (var c in ALPHA)
            {
                GetSSNs(curStrs, cur + c);
            }
        }
    }
}
