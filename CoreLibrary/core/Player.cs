using System;
using System.Collections.Generic;
using System.Text;

namespace CoreLibrary.core
{
    public class Player
    {
        private Bank CommunityOwner { get; }

        private double Attention { get; }
        private double Anxiety { get; }
        private double Shillyness { get; }
        private double Intelligence { get; }
        private double Whaley { get; }
        internal decimal MaximumSpending { get; }
        internal decimal CurrentSpending { get; set; } = 0;
        internal decimal Wallet { get; set; } = 0;
        internal List<TileSet> Portfolio { get; } = new List<TileSet>();
        internal bool Quitting { get; set; } = false;
        
        public Player(Bank bank, SimOptions opt)
        {
            CommunityOwner = bank;

            Attention = opt.Pick(opt.AttentionDistributionMean, opt.AttentionDistributionDeviation);
            Anxiety = opt.Pick(opt.AnxietyDistributionMean, opt.AnxietyDistributionDeviation);
            Shillyness = opt.Pick(opt.ShillynessDistributionMean, opt.ShillynessDistributionDeviation);
            Intelligence = opt.Pick(opt.IntelligenceDistributionMean, opt.IntelligenceDistributionDeviation);
            Whaley = opt.Pick(opt.WhaleyDistributionMean, opt.WhaleyDistributionDeviation);
            MaximumSpending =
                Math.Max(100M,
                    opt.SpendingPlateau *
                    (Decimal) ((1+Whaley*5) *
                    opt.Pick(opt.SpendingDistributionMean, opt.SpendingDistributionDeviation)
                    )
                );
        }


        public bool CanAffordSpending(decimal value)
        {
            return value < (MaximumSpending-CurrentSpending) + Wallet;
        }
        public void EnsureWalletValue(decimal value)
        {
            if (!CanAffordSpending(value))
                throw new Exception();

            decimal payable_from_wallet = Math.Min(Wallet, value);
            decimal deposit_needed = value - payable_from_wallet;
            
            DepositMoney(deposit_needed);
        }
        public void DepositMoney(decimal value)
        {
            if (value > MaximumSpending-CurrentSpending)
                throw new Exception();
            Wallet += value;
            CurrentSpending += value;
            //assume no transactions fee for deposits
        }
        public void DecideAction()
        {
            if (Quitting)
            {
                if (Portfolio.Count == 0)
                {
                    CommunityOwner.UnregisterPlayer(this);
                    return;
                }
                UpdateSale();
                return;
            }
            
            if (Portfolio.Count == 0)
            {
                BuyNew();
                return;
            }

            // 85% browsing, 10% selling, 4.9% buying, 0.1% quit
            //if (Bank.RNG.NextDouble() < 0.5)
            //{
                UpdateSale();
            //}
            double spendratio = (double)Math.Max(0M, (CurrentSpending - Wallet) / MaximumSpending);
            if (Bank.RNG.NextDouble() < 0.25 * spendratio) 
            {
                SellOnMarket();
            }
            if (Bank.RNG.NextDouble() < 0.25 * 1-spendratio)
            {
                Country c = CommunityOwner.GetRandomCheapCountry();
                double ratio = c.SoldTiles / (double)c.TotalTiles;
                if (ratio/2 < Bank.RNG.NextDouble())
                {
                    BuyNew();
                }
                //else
                //{
                    if (!BuyOffered()) 
                        BuyNew();
                //}
            }
            if (Bank.RNG.NextDouble() < 0.001)
            {
                QuitGame();
            }
        }
        public void QuitGame()
        {
            //improvements left Quitting model is meh
            Quitting = true;
            foreach (TileSet t in Portfolio)
            {
                if (!t.ForSale)
                {
                    t.SellPrice = ChooseSellPrice(t);
                    CommunityOwner.PutOnMarketplace(t);
                }
            }
            
        }

        public decimal ChooseBuyPrice(TileSet t)
        {
            double relaxness = (1 - Anxiety) * (1 + Shillyness) * (1 + Whaley) * (1 - (double)(CurrentSpending / MaximumSpending));
            double strictness = 1 - ((1 - Attention) * (1 - Intelligence));
            //improvements left see docs
/*
            decimal price = (t.GetCurrentValue() +
                        (decimal)(Bank.RNG.NextDouble()/2d - 0.25) * t.GetCurrentValue()
                        +
                        t.SellPrice +
                        (decimal)(Bank.RNG.NextDouble()/2d - 0.25) * t.SellPrice
                    ) / 2;
*/
            decimal price = (t.GetCurrentValue() +
                        (decimal)(Bank.RNG.NextDouble() - 1 + relaxness + strictness) * t.GetCurrentValue()
                        +
                        t.BuyPrice +
                        (decimal)(Bank.RNG.NextDouble() - 1 + relaxness + strictness) * t.BuyPrice
                ) / 2;
            return Math.Max(0.001M, price);
        }
        public decimal ChooseSellPrice(TileSet t)
        {
            double relaxness = (1 - Anxiety) * (1+Shillyness) * (1+Whaley) * (1 - (double)(CurrentSpending / MaximumSpending));
            double strictness = 1 - ((1 - Attention) * (1 - Intelligence));
            //improvements left see docs
            decimal price = 0;
            if (!Quitting)
                price = (t.GetCurrentValue() +
                            (decimal)(Bank.RNG.NextDouble() - 1 + relaxness + strictness) * t.GetCurrentValue()
                            +
                            t.BuyPrice +
                            (decimal)(Bank.RNG.NextDouble() - 1 + relaxness + strictness) * t.BuyPrice
                    ) / 2;
            else
            {

                double lowlim = Math.Max(0, relaxness - strictness);
                price = (t.GetCurrentValue() +
                            (decimal)(Bank.RNG.NextDouble() - 1 + lowlim) * t.GetCurrentValue()
                            +
                            t.BuyPrice +
                            (decimal)(Bank.RNG.NextDouble() - 1 + lowlim) * t.BuyPrice
                    ) / 3;
            }
/*
            price = (t.GetCurrentValue() +
                        (decimal)(Bank.RNG.NextDouble() - 0.5) * t.GetCurrentValue()
                        +
                        t.BuyPrice +
                        (decimal)(Bank.RNG.NextDouble() - 0.5) * t.BuyPrice
                    ) / 2;
*/
            return Math.Max(0.001M, price);
        }
        public void SellOnMarket()
        {
            if (Portfolio.Count == 0)
                return;

            //improvements left see docs
            double c = Bank.RNG.NextDouble();
            int idx = (int)((c * c) * Portfolio.Count);
            if (!Portfolio[idx].ForSale) {
                decimal price = ChooseSellPrice(Portfolio[idx]);
                Portfolio[idx].SellPrice = price;
                CommunityOwner.PutOnMarketplace(Portfolio[idx]);
            };
        }
        public void UpdateSale()
        {
            if (Portfolio.Count == 0)
                return;
            
            //improvements left see docs
            double c = Bank.RNG.NextDouble();
            int idx = (int)((c * c) * Portfolio.Count);
            if (Portfolio[idx].ForSale)
            {
                Portfolio[idx].SellPrice = ChooseSellPrice(Portfolio[idx]);
            };
        }
        
        public bool BuyOffered()
        {
            if (CommunityOwner.MarketPlace.Count == 0)
                return false;
            
            //improvements left see docs
            double c = Bank.RNG.NextDouble();
            int idx = (int)(c * CommunityOwner.MarketPlace.Count);
            TileSet set = CommunityOwner.MarketPlace[idx];
            if (ChooseBuyPrice(set) < set.SellPrice )
                return false;

            CommunityOwner.BuyPlayersTiles(this, set);
            return true;
        }
        public void BuyNew()
        {
            double n = Bank.RNG.NextDouble();
            double n2 = 1 - (1 - n) * (1 - n);
            //int amount = (int)(n2 * CommunityOwner.Options.TileSetPlateau-5) +5;
            
            Country c = CommunityOwner.GetRandomCheapCountry(true);
            int amount = (int)(n2 * (double)((MaximumSpending - CurrentSpending + Wallet) / c.NewTileValue))+1;
            CommunityOwner.BuyNewTiles(this, c,
                                         amount);
        }

    }
}
