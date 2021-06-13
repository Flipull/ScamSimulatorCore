using System;
using System.Collections.Generic;
using System.Text;

namespace ScamSimulatorCore.core
{
    class Bank
    {
        public static Random RNG = new Random();
        
        public decimal Wallet { get; set; } = 0;
        public List<Country> Countries { get; } = new List<Country>();
        public List<Player> PlayerBase { get; } = new List<Player>();
        
        public List<TileSet> MarketPlace { get; } = new List<TileSet>();
        public decimal TransactionFlatTax { get; set; } = 7.5M;
        public decimal TransactionPercTax { get; set; } = 1;
        public decimal TransactionOvervalueTaxPerc { get; set; } = 5;
        public SimOptions Options { get; }
        public Bank(SimOptions opt)
        {
            Options = opt;
            foreach(Country c in Seed.enumerateCountries())
            {
                Countries.Add(c);
            }
            CreatePlayer();
        }
        public decimal WorldValue()
        {
            decimal v = 0;
            foreach (Country c in Countries)
            {
                v += c.Worth;
            }
            return v;
        }
        public decimal PlayerBaseValue()
        {
            decimal v = 0;
            foreach (Player p in PlayerBase)
            {
                v += p.Wallet;
            }
            return v;
        }
        public decimal PlayerBasePortfolio()
        {
            decimal v = 0;
            foreach (Player p in PlayerBase)
            {
                foreach(TileSet t in p.Portfolio)
                {
                    v += t.GetCurrentValue();
                }
            }
            return v;
        }
        public decimal TotalTiles()
        {
            decimal v = 0;
            foreach (Country c in Countries)
            {
                v += c.TotalTiles;
            }
            return v;
        }
        public decimal SoldTiles()
        {
            decimal v = 0;
            foreach (Country c in Countries)
            {
                v += c.SoldTiles;
            }
            return v;
        }
        public decimal AvgCountryTileValue()
        {
            decimal v = 0;
            foreach (Country c in Countries)
            {
                v += c.NewTileValue;
            }
            return v/Countries.Count;
        }
        public double PlayerBaseAvgPercentageSpended()
        {
            decimal avgspending = 0;
            foreach (Player p in PlayerBase)
            {
                avgspending += (p.CurrentSpending - p.Wallet)/ p.MaximumSpending;
            }
            return (double)(avgspending/ PlayerBase.Count);
        }
        public double PlayerBasePercentageSpended()
        {
            decimal maxspending = 0;
            decimal spending = 0;
            foreach (Player p in PlayerBase)
            {
                maxspending += p.MaximumSpending;
                spending += p.CurrentSpending - p.Wallet;
            }
            return (double)(spending / maxspending);
        }

        public Player CreatePlayer() {
            Player p = new Player(this, Options);
            PlayerBase.Add(p);
            return p;
        }
        public Player GetRandomPlayer() {
            double ratio = (double)PlayerBase.Count / Options.PopulationPlateau;
            if (1-ratio > RNG.NextDouble() )
            {
                return CreatePlayer();
            } else
            {
                //improvements left see docs
                double c = RNG.NextDouble();
                int idx = (int)((c * c) * PlayerBase.Count);
                return PlayerBase[idx];
            }
        }
        public Country GetRandomCountry() {
            //improvements left see docs
            double c = RNG.NextDouble();
            int idx = (int)((c * c) * Countries.Count);
            return Countries[idx];
        }
        public bool BuyNewTiles(Player buyer, Country c, int amount)
        {
            decimal value = c.NewTileSetPrice(amount);
            if (!buyer.CanAffordSpending(value) || !c.CanCreateTiles(amount))
                return false;
            buyer.EnsureWalletValue(value);

            Wallet += value;
            //need referrals
            buyer.Wallet -= value;
            
            TileSet t = c.CreateTilesForSelling(amount);
            t.Owner = buyer;
            buyer.Portfolio.Add(t);
            t.BuyPrice = c.NewTileValue*amount;
            return true;
        }
        public bool BuyPlayersTiles(Player buyer, TileSet t)
        {
            Player seller = t.Owner;
            decimal value = t.SellPrice;

            if (!buyer.CanAffordSpending(value))
                return false;
            buyer.EnsureWalletValue(value);

            //need fees
            t.Owner.Wallet += value;
            buyer.Wallet -= value;
            
            t.MotherCountry.PlayerTilesSold(t, t.SellPrice,t.BuyPrice);
            seller.Portfolio.Remove(t);
            buyer.Portfolio.Add(t);

            t.Owner = buyer;
            t.BuyPrice = value;
            PullOffMarketplace(t);
            return true;
        }
        public void PutOnMarketplace(TileSet t)
        {
            MarketPlace.Add(t);
            t.ForSale = true;
        }

        //public void UpdateOnMarketPlace()
        //obsolete can be done in portfolio of player

        public void PullOffMarketplace(TileSet t)
        {
            MarketPlace.Remove(t);
            t.ForSale = false;
        }
    }
}
