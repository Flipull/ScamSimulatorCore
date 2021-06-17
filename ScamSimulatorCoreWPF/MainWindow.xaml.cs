using CoreLibrary.core;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace ScamSimulatorCoreWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static SimOptions o = new SimOptions();
        private static Bank b = new Bank(o);
        private static void Execute()
        {
            long divisor = 1;
            long iteration = 0;
            while (Running)
            {
                Player p = b.GetRandomPlayer();
                p.DecideAction();
                //b.SortCountriesOnNewTileValue();
                if (iteration % o.TileSetPlateauIncrementOffset == 0)
                    o.TileSetPlateau = Math.Min(o.TileSetPlateau + o.TileSetPlateauIncrement, o.TileSetPlateauMax);

                if (iteration % divisor == 0)
                //if (iteration % Math.Pow(10,Math.Ceiling(Math.Log10(iteration+1)-1)) == 0)
                {
                    if (iteration >= 100 * divisor)
                    {
                        if (bwv.Count > 100)
                            App.Current.Dispatcher.Invoke(cutInTen);
                        divisor *= 10;
                    }
                    BankDataInfo i = b.Stats();
                    //addToCharts(i, iteration);
                    App.Current.Dispatcher.Invoke(
                        new Action(() =>
                        {
                            addToCharts(i, iteration);
                        })

                        );
                }
                iteration++;
            }
        }

        private static Task executer = new Task(Execute);
        private static bool Running = false;

        static SeriesCollection chart = new SeriesCollection();

        static LineSeries bw = new LineSeries();
        static ChartValues<decimal> bwv = new ChartValues<decimal>();
        static LineSeries cw = new LineSeries();
        static ChartValues<decimal> cwv = new ChartValues<decimal>();
        static LineSeries t = new LineSeries();
        static ChartValues<decimal> tv = new ChartValues<decimal>();
        static LineSeries ts = new LineSeries();
        static ChartValues<long> tsv = new ChartValues<long>();

        static LineSeries p = new LineSeries();
        static ChartValues<decimal> pv = new ChartValues<decimal>();
        static LineSeries pm = new LineSeries();
        static ChartValues<decimal> pmv = new ChartValues<decimal>();
        static LineSeries pw = new LineSeries();
        static ChartValues<decimal> pwv = new ChartValues<decimal>();
        static LineSeries pp = new LineSeries();
        static ChartValues<decimal> ppv = new ChartValues<decimal>();

        static LineSeries ex = new LineSeries();
        static ChartValues<decimal> exv = new ChartValues<decimal>();

        private static ChartValues<T> partialCopy<T>(ChartValues<T> list)
        {
            ChartValues<T> newlist = new ChartValues<T>();

            for (int x = 0; x < list.Count; x += 10)
            {
                newlist.Add(list[x]);
            }
            return newlist;

        }
        private static void cutInTen()
        {
            bw.Values = partialCopy<decimal>(bwv);
            t.Values = partialCopy<decimal>(tv);
            ts.Values = partialCopy<long>(tsv);
            p.Values = partialCopy<decimal>(pv);
            pm.Values = partialCopy<decimal>(pmv);
            pw.Values = partialCopy<decimal>(pwv);
            pp.Values = partialCopy<decimal>(ppv);
            ex.Values = partialCopy<decimal>(exv);
        }

        private static void addToCharts(BankDataInfo i, long iteration)
        {
            long activecount = b.PlayerBase.Count;
            long excount = b.ExPlayerBase.Count;
            long totalcount = activecount + excount;
            bwv.Add(i.BankWallet);
            //cwv.Add(i.CountriesWorth / b.Countries.Count);
            tv.Add(i.AverageTileNewPrice);
            //tsv.Add(100 * i.SoldTiles / (double)i.TotalTiles);
            tsv.Add((i.TotalTiles - i.SoldTiles));
            pv.Add(activecount);
            //pmv.Add(100 * (double)i.PlayerSpend / (double)i.PlayerMaxSpend);
            pmv.Add((i.PlayerMaxSpend - i.PlayerSpend));
            pwv.Add(i.PlayersWallet);
            ppv.Add(i.PlayersPortfolio);
            exv.Add(i.ExPlayersWallet);
            /*
            if (bwv.Count > 50)
            {
                bwv.RemoveAt(0);
                //cwv.RemoveAt(0);
                tv.RemoveAt(0);
                tsv.RemoveAt(0);
                pv.RemoveAt(0);
                pmv.RemoveAt(0);
                pwv.RemoveAt(0);
                ppv.RemoveAt(0);
                exv.RemoveAt(0);
            }
            */
            bw.Values = bwv;
            t.Values = tv;
            ts.Values = tsv;
            p.Values =  pv;
            pm.Values = pmv;
            pw.Values = pwv;
            pp.Values = ppv;
            ex.Values = exv;
        }
        private static void createCharts()
        {
            //ColumnSeries col = new ColumnSeries();
            //ChartValues<long> colv = new ChartValues<long>();
            //col.Values = colv;

            bw = new LineSeries();
            bw.Title = "Bank Wealth / P";
            bwv = new ChartValues<decimal>();
            //cw = n    ew LineSeries();
            //cw.Title = "Country Wealth";
            //cwv = new ChartValues<decimal>();
            t = new LineSeries();
            t.Title = "E-cash/Tile";
            tv = new ChartValues<decimal>();
            ts = new LineSeries();
            ts.Title = "Tiles Left";
            tsv = new ChartValues<long>();
            p = new LineSeries();
            p.Title = "# Players";
            pv = new ChartValues<decimal>();
            pm = new LineSeries();
            pm.Title = "P Wealth / P";
            pmv = new ChartValues<decimal>();
            pw = new LineSeries();
            pw.Title = "P E-cash / P";
            pwv = new ChartValues<decimal>();
            pp = new LineSeries();
            pp.Title = "P Portfolio / P";
            ppv = new ChartValues<decimal>();
            ex = new LineSeries();
            ex.Title = "Quitters (Wealth/P)";
            exv = new ChartValues<decimal>();
            bw.Values = bwv;
            //cw.Values = cwv;
            //chart.Add(cw);
            t.Values = tv;
            ts.Values = tsv;
            p.Values = pv;
            pm.Values = pmv;
            pw.Values = pwv;
            pp.Values = ppv;
            ex.Values = exv;
            bindCharts();
        }
        private static void bindCharts()
        {
            lock(chart)
            {
                chart.Clear();
                chart.Add(bw);
                chart.Add(t);
                chart.Add(ts);
                chart.Add(p);
                chart.Add(pm);
                chart.Add(pw);
                chart.Add(pp);
                chart.Add(ex);
            }
        }
        public void UnbindChart(object sender, RoutedEventArgs e)
        {
            Chart.Series = null;
        }
        public void BindChart(object sender, RoutedEventArgs e)
        {
            Chart.Series = chart;
        }
        public MainWindow()
        {
            InitializeComponent();
            createCharts();
            Chart.Series = chart;
            Chart.DisableAnimations = true;
        }

        private void StartClick(object sender, RoutedEventArgs e)
        {
            if (Running)
            {
                Running = false;
                executer.Wait();
            }
            else
            {
                Running = true;
                executer = new Task(Execute);
                executer.Start();
            }

        }

        private void ResetClick(object sender, RoutedEventArgs e)
        {
            if (Running) return;
            b = new Bank(o);
            createCharts();
            Chart.Series = chart;
        }
    }
}
