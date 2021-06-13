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
        public List<Player> ExPlayerBase { get; } = new List<Player>();

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
        public BankDataInfo Stats()
        {
            decimal cv = 0;
            long ctt = 0;
            long cts = 0;
            decimal tv = 0;
            foreach (Country c in Countries)
            {
                cv += c.Worth;
                tv += c.NewTileValue;
                ctt += c.TotalTiles;
                cts += c.SoldTiles;
            }
            tv /= Countries.Count;

            
            decimal ps = 0;
            decimal psm = 0;
            decimal pv = 0;
            decimal portv = 0;
            decimal ex = 0;
            foreach (Player p in PlayerBase)
            {
                if (p.Quitting)
                    ex += p.Wallet;
                else
                {
                    pv += p.Wallet;
                    ps += Math.Max(0M, p.CurrentSpending - p.Wallet);
                    psm += p.MaximumSpending;
                    foreach (TileSet t in p.Portfolio)
                    {
                        portv += t.GetCurrentValue();
                    }
                }
            }
            foreach (Player p in ExPlayerBase)
                ex += p.Wallet;


            BankDataInfo i = new BankDataInfo()
            {
                BankWallet = Wallet,
                CountriesWorth = cv,
                SoldTiles = cts,
                TotalTiles = ctt,
                AverageTileNewPrice = tv,
                PlayerCount = PlayerBase.Count,
                PlayerSpend = ps,
                PlayerMaxSpend = psm,
                PlayersWallet = pv,
                PlayersPortfolio = portv,
                ExPlayersWallet = ex
            };
            


            return i;
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
                if (!p.Quitting)
                    v += p.Wallet;
            }
            return v;
        }
        public decimal ExPlayerBaseValue()
        {
            decimal v = 0;
            foreach (Player p in PlayerBase)
                if (p.Quitting)
                    v += p.Wallet;

            foreach (Player p in ExPlayerBase)
                    v += p.Wallet;
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
                avgspending += Math.Max(0,p.CurrentSpending - p.Wallet)/ p.MaximumSpending;
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
        public void UnregisterPlayer(Player p)
        {
            PlayerBase.Remove(p);
            ExPlayerBase.Add(p);
        }
        public Player GetRandomPlayer() {
            double ratio = Math.Min(1,(double)(PlayerBase.Count+ExPlayerBase.Count) / Options.PopulationPlateau);
            double r2 = (1 - ratio) * (1 - ratio);
            if ( (1-r2) < RNG.NextDouble() )
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
        public Country GetRandomCountry(bool fornewtiles = false) {
            //improvements left see docs
            Country chosen;
            double c;
            int idx;
            do
            {
                c = RNG.NextDouble();
                idx = (int)((c * c) * Countries.Count);
                chosen = Countries[idx];
            } while (fornewtiles && chosen.SoldTiles == chosen.TotalTiles);
            return chosen;
        }
        public bool BuyNewTiles(Player buyer, Country c, int amount)
        {
            if (amount > Options.TileSetPlateau)
                amount = Options.TileSetPlateau;
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
        public bool PutOnMarketplace(TileSet t)
        {
            if (t.SellPrice < 0.00001M)
                return false;
            MarketPlace.Add(t);
            t.ForSale = true;
            return true;
        }

        //public void UpdateOnMarketPlace()
        //obsolete can be done in portfolio of player
        
        public bool PullOffMarketplace(TileSet t)
        {
            MarketPlace.Remove(t);
            t.ForSale = false;
            return true;
        }
    }
}
