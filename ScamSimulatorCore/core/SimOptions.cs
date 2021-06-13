using System;
using System.Collections.Generic;
using System.Text;

namespace ScamSimulatorCore.core
{
    class SimOptions
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

        public int PopulationPlateau { get; } = 500000;
        public decimal SpendingPlateau { get; } = 10000;

        public int TileSetPlateau { get; set; } = 25;

        ////////////////////////////////////////
        public double Pick(double mean, double dev)
        {
            return Math.Max(0, Math.Min(1, mean - 0.5*dev + dev*RNG.NextDouble()));
        }
        
    }
}
