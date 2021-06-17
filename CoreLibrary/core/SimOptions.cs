using System;
using System.Collections.Generic;
using System.Text;

namespace CoreLibrary.core
{
    public class SimOptions
    {
        private static Random RNG = new Random();
        
        public double AttentionDistributionMean { get; set; } = 0.5;
        public double AttentionDistributionDeviation { get; set; } = 1;
        public double AnxietyDistributionMean { get; set; } = 0.5;
        public double AnxietyDistributionDeviation { get; set; } = 1;
        public double ShillynessDistributionMean { get; set; } = 0.5;
        public double ShillynessDistributionDeviation { get; set; } = 1;
        public double IntelligenceDistributionMean { get; set; } = 0.5;
        public double IntelligenceDistributionDeviation { get; set; } = 1;
        public double WhaleyDistributionMean { get; set; } = 0;
        public double WhaleyDistributionDeviation { get; set; } = 1;
        public double SpendingDistributionMean { get; set; } = 0;
        public double SpendingDistributionDeviation { get; set; } = 1;
        
        public int PopulationPlateau { get; } = 40000;
        public decimal SpendingPlateau { get; } = 1000;

        public int TileSetPlateauIncrementOffset { get; set; } = 500;
        public int TileSetPlateauStart { get; set; } = 10;//will be incremented first time
        public int TileSetPlateauIncrement { get; set; } = 10;
        public int TileSetPlateauMax { get; set; } = 10000;
        public int TileSetPlateau { get; set; }
        public SimOptions()
        {
            TileSetPlateau = TileSetPlateauStart;
        }

        ////////////////////////////////////////
        public double Pick(double mean, double dev)
        {
            double nr = RNG.NextDouble();
            double ch = nr*nr * dev;
            if (RNG.NextDouble() < 0.5)
                ch = -ch;
            return Math.Max(0, Math.Min(1, mean + ch));
        }
        
    }
}
