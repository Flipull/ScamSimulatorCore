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
        public long Population { get; }
        public long TotalTiles { get; }
        public long SoldTiles { get; set; } = 0;
        public decimal NewTileValue = 0.1M;
        public void UpdateTileValue()
        {
            if (SoldTiles == 0)
            {
                NewTileValue = 0.1M;
                return;
            }
            //return Worth / SoldTiles + 0.1M * (1 - SoldTiles / TotalTiles);
            //return Math.Max(0.1M, Worth / SoldTiles) +0.1M*(1- SoldTiles/TotalTiles);
            int s = 0;
            decimal v = 0;
            var list = SoldTileSets.GetRange(Math.Max(0,SoldTileSets.Count - 20), Math.Min(20, SoldTileSets.Count));
            foreach (TileSet t in list)
            {
                v += t.BuyPrice;
                s += t.Amount;
            }
            NewTileValue = Math.Max(0.1M, v / s) + 0.1M * (1M - 1M / (SoldTiles+1M));// + 0.1M;
            //NewTileValue = Math.Max(0.1M, v / s) + 0.1M * (1 - SoldTiles / (decimal)TotalTiles);// + 0.1M;
        }
        public List<TileSet> SoldTileSets { get; } = new List<TileSet>();

        public Country(string name, long population, long area_sq, double richness = 0.5, double classness = 0.5)
        {
            Name = name;
            Population = population;
            TotalTiles = Math.Max(1, area_sq/25+10);
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
            t.BuyPrice = NewTileValue * amount;
            SoldTiles += amount;
            SoldTileSets.Add(t);

            //second update country
            Worth += value;
            UpdateTileValue();


            return t;
        }
        public bool PlayerTilesSold(TileSet t, decimal new_value, decimal old_value)
        {

            //assume t is 100% in this country
            int amount = t.Amount;
            decimal real_value = NewTileValue * amount;
            
            //update country
            decimal delta_value = (new_value - old_value);// * amount/TotalTiles;

            Worth += delta_value;
            UpdateTileValue();
            return true;
        }
    }
}
