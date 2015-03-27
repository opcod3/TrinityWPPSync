using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrinityWPPSync
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!Config.Load(args))
                return;

            if (Config.noCMSG && Config.noSMSG)
            {
                Console.WriteLine("All Sync Disabled!");
                return;
            }

            WPP.TryPopulate();
            Trinity.Sync();
        }
    }
}
