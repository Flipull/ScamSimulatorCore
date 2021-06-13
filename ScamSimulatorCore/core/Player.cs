using System;
using System.Collections.Generic;
using System.Text;

namespace ScamSimulatorCore.core
{
    class Player
    {
        public Bank CommunityOwner { get; }

        public double Attention { get; }
        public double Anxiety { get; }
        public double Shillyness { get; }
        public double Intelligence { get; }
        public double Whaley { get; }
        public decimal MaximumSpending { get; }
        public decimal CurrentSpending { get; set; } = 0;
        public decimal Wallet { get; set; } = 0;
        public List<TileSet> Portfolio { get; } = new List<TileSet>();
        public bool Quitting { get; set; } = false;
        
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
                    (Decimal) ((1+Whaley) *
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
            
            if (Portfolio.Count == 0 && !Quitting)
            {
                BuyNew();
                return;
            }

            // 85% browsing, 10% selling, 4.9% buying, 0.1% quit
            if (Bank.RNG.NextDouble() < 0.5)
            {
                UpdateSale();
            }
            if (Bank.RNG.NextDouble() < 0.2)
            {
                SellOnMarket();
            }
            if (Bank.RNG.NextDouble() < 0.049 * (double)(MaximumSpending/(CurrentSpending-Wallet + 1)))
            {
                double ratio = (double)(CommunityOwner.SoldTiles() / CommunityOwner.TotalTiles());
                if ( (1- ratio) < Bank.RNG.NextDouble())
                {
                    BuyNew();
                } else
                {
                    BuyOffered();
                }
            }
            if (Bank.RNG.NextDouble() < 0.0001)
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
                    CommunityOwner.PutOnMarketplace(t);
                }
            }
            
        }

        public decimal ChooseBuyPrice(TileSet t)
        {
            double relaxness = (1 - Anxiety) * (1 + Shillyness) * (1 + Whaley) * (1 - (double)(CurrentSpending / MaximumSpending));
            double strictness = 1 - ((1 - Attention) * (1 - Intelligence));
            //improvements left see docs
            return t.GetCurrentValue() +
                            (decimal)(Bank.RNG.NextDouble() - 0.5 + relaxness - strictness) *
                            t.GetCurrentValue();
        }
        public decimal ChooseSellPrice(TileSet t)
        {
            double relaxness = (1 - Anxiety) * (1+Shillyness) * (1+Whaley) * (1 - (double)(CurrentSpending / MaximumSpending));
            double strictness = 1 - ((1 - Attention) * (1 - Intelligence));
            //improvements left see docs
            int divren = 2;
            if (!Quitting)
                return (t.GetCurrentValue() +
                            (decimal)(Bank.RNG.NextDouble() - 1 + relaxness + strictness) * t.GetCurrentValue()
                            +
                            t.BuyPrice +
                            (decimal)(Bank.RNG.NextDouble() - 1 + relaxness + strictness) * t.BuyPrice
                    ) / 2;
            else
            {

                double lowlim = Math.Max(0, relaxness - strictness);
                return (t.GetCurrentValue() +
                            (decimal)(Bank.RNG.NextDouble() - 1 + lowlim) * t.GetCurrentValue()
                            +
                            t.BuyPrice +
                            (decimal)(Bank.RNG.NextDouble() - 1 + lowlim) * t.BuyPrice
                    ) / 3;
            }

        }
        public void SellOnMarket()
        {
            if (Portfolio.Count == 0)
                return;

            //improvements left see docs
            double c = Bank.RNG.NextDouble();
            int idx = (int)((c * c) * Portfolio.Count);
            if (!Portfolio[idx].ForSale) {
                Portfolio[idx].SellPrice = ChooseSellPrice(Portfolio[idx]);
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
            int idx = (int)((c * c) * CommunityOwner.MarketPlace.Count);
            TileSet set = CommunityOwner.MarketPlace[idx];
            if (ChooseBuyPrice(set) < set.SellPrice )
                return false;

            CommunityOwner.BuyPlayersTiles(this, set);
            return true;
        }
        public void BuyNew()
        {
            double n = Bank.RNG.NextDouble();
            int amount = (int)(n*n * CommunityOwner.Options.TileSetPlateau-5) +5;
            CommunityOwner.BuyNewTiles(this, CommunityOwner.GetRandomCountry(),
                                        amount);
        }

    }
}
