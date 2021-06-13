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
            SimOptions o = new SimOptions();
            Bank b = new Bank(o);
            
            while (true)
            {
                Player p = b.GetRandomPlayer();
                p.DecideAction();

                if (iteration % o.TileSetPlateauIncrementOffset == 0)
                    o.TileSetPlateau = Math.Min(o.TileSetPlateau + o.TileSetPlateauIncrement, o.TileSetPlateauMax);
                if (iteration < 10000 || iteration % 100000 == 0)
                {
                    BankDataInfo i = b.Stats();
                    string s =
                        string.Format("B{0:C2} C{1:C2} Ct{2:C4} T%{3:N1} P#{4} PM%{5:N1} PW{6:C2} PP{7:C2} EX{8:C2}",
                                    i.BankWallet,
                                    i.CountriesWorth,
                                    i.AverageTileNewPrice,
                                    100 * i.SoldTiles / (double)i.TotalTiles,
                                    b.PlayerBase.Count,
                                    (100 * (double) i.PlayerSpend/ (double)i.PlayerMaxSpend),
                                    i.PlayersWallet,
                                    i.PlayersPortfolio,
                                    i.ExPlayersWallet
                                );
                    Console.WriteLine(s);
                }
                iteration++;
            }
        }
    }
}
