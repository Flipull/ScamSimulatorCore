using System;
using System.Collections.Generic;
using System.Text;

namespace CoreLibrary.core
{
    public class TileSet
    {
        public Country MotherCountry { get; }
        public int Amount { get; }
        public decimal BuyPrice { get; set; }
        public double DensitySum { get; }
        public Player Owner { get; set; } = null;
        public bool ForSale { get; set; } = false;
        public decimal SellPrice { get; set; }

        public TileSet(Country country, int amount, decimal value, double density)
        {
            MotherCountry = country;
            Amount = amount;
            BuyPrice = value;
            DensitySum = density;
        }
        public decimal GetCurrentValue()
        {
            return MotherCountry.NewTileSetPrice(Amount);
        }
    }
}
