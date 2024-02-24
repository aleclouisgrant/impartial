using Impartial;
using ImpartialUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace ImpartialUI.Controls
{
    public partial class CompetitorProfile : UserControl
    {
        public CompetitorProfile()
        {
            InitializeComponent();

            InitializePlot();
        }

        private void InitializePlot()
        {
            Plot.Plot.Clear();
            Plot.Plot.XLabel("Time");
            Plot.Plot.YLabel("Rating");
        }

        private void MakePlot()
        {
            var compDm = (CompetitorDataModel)DataContext;
            if (compDm == null)
                return;

            // y axis is the rating
            int yMin = 100000;
            int yMax = -100000;

            // x axis is time
            int xMin = (int)DateTime.MaxValue.ToOADate();
            int xMax = (int)DateTime.MinValue.ToOADate();

            var xPoints = new List<double>();
            var yPoints = new List<double>();

            var redXPoints = new List<double>();
            var redYPoints = new List<double>();
            
            foreach (var compHistory in compDm.CompetitionHistory)
            {
                yMin = compHistory.RatingAfter < yMin ? compHistory.RatingAfter : yMin;
                yMax = compHistory.RatingAfter > yMax ? compHistory.RatingAfter : yMax;

                xMin = compHistory.CompetitionDate.ToOADate() < xMin ? (int)compHistory.CompetitionDate.ToOADate() : xMin;
                xMax = compHistory.CompetitionDate.ToOADate() > xMax ? (int)compHistory.CompetitionDate.ToOADate() : xMax;

                xPoints.Add(compHistory.CompetitionDate.ToOADate());
                yPoints.Add(compHistory.RatingAfter);

                if (compHistory.RatingChange < 0)
                {
                    redXPoints.Add(compHistory.CompetitionDate.ToOADate());
                    redYPoints.Add(compHistory.RatingAfter);
                }
            }

            if (xPoints.Count > 0)
                Plot.Plot.AddScatter(xPoints.ToArray(), yPoints.ToArray(), System.Drawing.Color.Green);

            for (int i = 0; i < redXPoints.Count; i++)
            {
                Plot.Plot.AddPoint(redXPoints.ElementAt(i), redYPoints.ElementAt(i), color: System.Drawing.Color.Red);
            }

            Plot.Plot.SetAxisLimitsX(xMin, xMax);
            Plot.Plot.SetAxisLimitsY(Math.Round((double)yMin / 100d, 0) * 100 - 100, Math.Round((double)yMax / 100d, 0) * 100 + 100);

            Plot.Plot.XAxis.DateTimeFormat(true);

            Plot.Refresh();
        }

        private void UserControl_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            InitializePlot();
            MakePlot();
        }
    }
}
