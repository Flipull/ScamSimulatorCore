using CoreLibrary.core;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ScamSimulatorCore
{

    class Program
    {
        private static int iteration = 0;
        private static bool Running = true;
        static void Main(string[] args)
        {
            SimOptions o = new SimOptions();
            Bank b = new Bank(o);

            long iteration = 1;
            string filename = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".txt";
            StreamWriter filewr = File.CreateText(filename);
            
            while (!Console.KeyAvailable || Console.ReadKey().Key != ConsoleKey.Q)
            {
                Player p = b.GetRandomPlayer();
                p.DecideAction();
                //b.SortCountriesOnNewTileValue();
                if (iteration % o.TileSetPlateauIncrementOffset == 0)
                    o.TileSetPlateau = Math.Min(o.TileSetPlateau + o.TileSetPlateauIncrement, o.TileSetPlateauMax);
                
                if (iteration % 10000 == 0)
                //if (iteration % Math.Pow(10,Math.Ceiling(Math.Log10(iteration+1)-1)) == 0)
                {
                    BankDataInfo i = b.Stats();
                    string s =
                        string.Format("B{0:C2} C{1:C2} Ct{2:C4} T%{3:N1} P#{4} PM%{5:N1} PW{6:C2} PP{7:C2} EX{8:C2}",
                                    i.BankWallet,
                                    i.CountriesWorth,
                                    i.AverageTileNewPrice,
                                    100 * i.SoldTiles / (double)i.TotalTiles,
                                    b.PlayerBase.Count,
                                    (100 * (double)i.PlayerSpend / (double)i.PlayerMaxSpend),
                                    i.PlayersWallet,
                                    i.PlayersPortfolio,
                                    i.ExPlayersWallet
                                );
                    string s2 =
                        string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                    i.BankWallet,
                                    i.CountriesWorth,
                                    i.AverageTileNewPrice,
                                    100 * i.SoldTiles / (double)i.TotalTiles,
                                    b.PlayerBase.Count,
                                    (100 * (double)i.PlayerSpend / (double)i.PlayerMaxSpend),
                                    i.PlayersWallet,
                                    i.PlayersPortfolio,
                                    i.ExPlayersWallet
                                );

                    Console.WriteLine(s);
                    filewr.WriteLine(s2);
                    //addToCharts(i, iteration);
                }
                iteration++;
            }
            filewr.Close();
            
        }



        
    }
}
