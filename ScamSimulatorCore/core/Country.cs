using System;
using System.Collections.Generic;
using System.Text;

namespace ScamSimulatorCore.core
{
    class Country
    {
        public string Name { get; } = "Unknown";
        public decimal Worth { get; set; } = 0;
        public double DensityDistributionMean { get; }
        public double DensityDistributionDeviation { get; }
        public int Population { get; }
        public int TotalTiles { get; }
        public int SoldTiles { get; set; } = 0;
        public decimal NewTileValue { get; set; } = 0.10M;
        public List<TileSet> SoldTileSets { get; } = new List<TileSet>();

        public Country(string name, int population, int area_sq, double richness = 0.5, double classness = 0.5)
        {
            Name = name;
            Population = population;
            TotalTiles = area_sq;
            DensityDistributionMean = richness;
            DensityDistributionDeviation = classness;
        }

        /////////////////////////////////////////////////////

        public decimal NewTileSetPrice(int amount)
        {
            return NewTileValue * amount;
        }
        public bool CanCreateTiles(int amount)
        {
            return (TotalTiles - SoldTiles) - amount >= 0;
        }
        public double Pick(double mean, double dev)
        {
            return Math.Max(0, Math.Min(1, Bank.RNG.NextDouble()));
        }
        public TileSet CreateTilesForSelling(int amount)
        {
            //every call the country will automatically update
            if (!CanCreateTiles(amount))
                throw new Exception();
            decimal value = NewTileValue * amount;
            //first create tileset
            
            //needs better method - in order of density, high to low with some deviation
            TileSet t = new TileSet(this, amount, value,
                    Pick(DensityDistributionMean, DensityDistributionDeviation)
                        * TotalTiles);
            SoldTiles += amount;
            SoldTileSets.Add(t);

            //second update country
            Worth += value;
            NewTileValue += value /* * amount*/ / TotalTiles;
            

            return t;
        }
        public bool PlayerTilesSold(TileSet t, decimal new_value, decimal old_value)
        {
            decimal tmp = NewTileValue;

            //assume t is 100% in this country
            int amount = t.Amount;
            decimal real_value = NewTileValue*amount;

            //update country
            decimal delta_value = (new_value - old_value) * amount/TotalTiles;
            
            NewTileValue += real_value /* * amount*/ / TotalTiles
                    * (Worth+delta_value)/Worth;
            
            if (NewTileValue > 2*tmp)
            {
                Console.WriteLine("beh");
            }

            Worth += delta_value;
            return true;

        }
    }
}
