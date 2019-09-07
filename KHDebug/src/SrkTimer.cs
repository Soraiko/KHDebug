using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KHDebug
{
    public static class SrkTimer
    {
        static List<string> Names = new List<string>(0);
        static List<DateTime> Ticks = new List<DateTime>(0);
        static int Index = -1;


        public static void Start(string name)
        {
            Index = Names.IndexOf(name);
            if (Index<0)
            {
                Names.Add(name);
                Ticks.Add(DateTime.Now);
                Index = Names.Count-1;
            }
            else
            {
                Ticks[Index] = DateTime.Now;
            }
        }

        public static void Stop()
        {
            System.Diagnostics.Debug.WriteLine(Names[Index]+ " spent "+ (DateTime.Now - Ticks[Index]).TotalMilliseconds  + " milliseconds");
        }
    }
}
