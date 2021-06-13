using ScamSimulatorCore.core;
using System;
using System.Threading.Tasks;

namespace ScamSimulatorCore
{
    class Program
    {
        private static int iteration = 0;
        static void Main(string[] args)
        {
            Bank b = new Bank(new SimOptions());

            while (true)
            {
                Player p = b.GetRandomPlayer();
                p.DecideAction();
                
                if (iteration%10000 == 0)
                {
                    string s = 
                        string.Format("B{0:C2} C{1:C2} Ct{2:C2} T%{3:N4} P#{4} PM%{5:N4} PW{6:C2} PP{7:C2}", 
                                    b.Wallet,
                                    b.WorldValue(),
                                    b.AvgCountryTileValue(),
                                    (double)(b.SoldTiles()/b.TotalTiles()*100),
                                    b.PlayerBase.Count,
                                    b.PlayerBaseAvgPercentageSpended(),
                                    //b.PlayerBasePercentageSpended(),
                                    b.PlayerBaseValue(),
                                    b.PlayerBasePortfolio()
                                );
                    Console.WriteLine(s);
                }
                iteration++;
            }
        }
    }
}
