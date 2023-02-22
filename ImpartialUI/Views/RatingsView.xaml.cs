using ImpartialUI.ViewModels;
using MongoDB.Driver.Linq;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace ImpartialUI.Views
{
    public partial class RatingsView : UserControl
    {
        public RatingsView()
        {
            InitializeComponent();

            InitializePlot();
        }

        private void InitializePlot()
        {
            Plot.Plot.XLabel("Rating");
            Plot.Plot.YLabel("WSDC Points");

            Plot.Plot.SetAxisLimitsX(800, 1800);
            Plot.Plot.SetAxisLimitsY(0, 400);

            Plot.Plot.AddHorizontalLine(150);

            Plot.Refresh();
        }

        private void MakePlot()
        {
            var compDm = ((RatingsViewModel)DataContext).CompDm;
            var leads = compDm.Where(c => c.Competitor.LeadStats.Rating != 1000);
            var follows = compDm.Where(c => c.Competitor.FollowStats.Rating != 1000);

            foreach (var comp in leads)
            {
                Plot.Plot.AddPoint(comp.Competitor.LeadStats.Rating, comp.WsdcPoints, label: comp.Competitor.FullName, color: System.Drawing.Color.Blue);
            }
            foreach (var comp in follows)
            {
                Plot.Plot.AddPoint(comp.Competitor.FollowStats.Rating, comp.WsdcPoints, label: comp.Competitor.FullName, color: System.Drawing.Color.Red);
            }

            Plot.Refresh();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MakePlot();
        }
    }
}
