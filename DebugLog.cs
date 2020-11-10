using System.Collections.Generic;
using System.Linq;

namespace HeatSim
{
    public static class DebugLog
    {
        private static readonly int MAX_RECORDS = 100;
        private static int dropped = 0;
        private static Queue<string> buf = new Queue<string>();

        public static void WriteLine(string s)
        {
            buf.Enqueue(s);
            if (buf.Count > MAX_RECORDS)
            {
                buf.Dequeue();
                dropped++;
            }
        }

        public static string ReadAll()
        {
            string res = "dropped " + dropped + "\n" + string.Join<string>("\n", buf);
            dropped = 0;
            buf.Clear();
            return res;
        }
    }
}
