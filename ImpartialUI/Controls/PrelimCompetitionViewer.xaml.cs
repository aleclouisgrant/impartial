using Impartial;
using ImpartialUI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ImpartialUI.Controls
{
    public partial class PrelimCompetitionViewer : UserControl
    {
        private static readonly int PROMOTED_COLUMN = 0;
        private static readonly int COUNT_COLUMN = 1;
        private static readonly int BIB_COLUMN = 2;
        private static readonly int COMPETITOR_COLUMN = 3;
        private static readonly int SCORE_COLUMN_START = 4;

        private class PrelimCompetitorScores
        {
            public ICompetitor Competitor { get; set; }
            public List<IPrelimScore> PrelimScores { get; set; } = new List<IPrelimScore>();
            public double TotalScore { get; set; }
        }

        #region DependencyProperties

        public static readonly DependencyProperty PrelimCompetitionProperty = DependencyProperty.Register(
            nameof(PrelimCompetition),
            typeof(IPrelimCompetition),
            typeof(PrelimCompetitionViewer),
            new FrameworkPropertyMetadata(new PrelimCompetition(), OnPrelimCompetitionPropertyChanged));
        public IPrelimCompetition PrelimCompetition
        {
            get { return (IPrelimCompetition)GetValue(PrelimCompetitionProperty); }
            set { SetValue(PrelimCompetitionProperty, value); }
        }
        private static void OnPrelimCompetitionPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var control = (PrelimCompetitionViewer)source;
            control.ScoreGrid.Children.Clear();
            control.ScoreGrid.RowDefinitions.Clear();
            control.ScoreGrid.ColumnDefinitions.Clear();

            #region Header
            control.ScoreGrid.RowDefinitions.Add(new RowDefinition());
            control.ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); // Promoted
            control.ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); // Count
            control.ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); // Bib
            control.ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition()); // competitor

            var headerBorder = new Border()
            {
                Style = Application.Current.Resources["ScoreViewerHeaderBorderStyle"] as Style
            };
            control.ScoreGrid.Children.Add(headerBorder);
            Grid.SetRow(headerBorder, 0);
            Grid.SetColumn(headerBorder, 0);
            Grid.SetColumnSpan(headerBorder, 100);

            /// Promoted, Count, Bib, Competitor, [Judges], Sum

            var promotedTextBlock = new TextBlock()
            {
                Text = "Promoted",
                Style = Application.Current.Resources["ScoreViewerHeaderTextStyle"] as Style
            };
            control.ScoreGrid.Children.Add(promotedTextBlock);
            Grid.SetRow(promotedTextBlock, 0);
            Grid.SetColumn(promotedTextBlock, PROMOTED_COLUMN);

            var countTextBlock = new TextBlock()
            {
                Text = "Count",
                Style = Application.Current.Resources["ScoreViewerHeaderTextStyle"] as Style
            };
            control.ScoreGrid.Children.Add(countTextBlock);
            Grid.SetRow(countTextBlock, 0);
            Grid.SetColumn(countTextBlock, COUNT_COLUMN);

            var bibTextBlock = new TextBlock()
            {
                Text = "Bib",
                Style = Application.Current.Resources["ScoreViewerHeaderTextStyle"] as Style
            };
            control.ScoreGrid.Children.Add(bibTextBlock);
            Grid.SetRow(bibTextBlock, 0);
            Grid.SetColumn(bibTextBlock, BIB_COLUMN);

            var competitorTextBlock = new TextBlock()
            {
                Text = "Competitor",
                Style = Application.Current.Resources["ScoreViewerHeaderTextStyle"] as Style
            };
            control.ScoreGrid.Children.Add(competitorTextBlock);
            Grid.SetRow(competitorTextBlock, 0);
            Grid.SetColumn(competitorTextBlock, COMPETITOR_COLUMN);

            var competition = (IPrelimCompetition)e.NewValue;
            if (competition == null)
                return;

            List<IJudge> judges = competition?.Judges.OrderBy(j => j.FullName).ToList() ?? new();
            
            // judge names
            foreach (var judge in judges)
            {
                control.ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                var judgeName = judge.FirstName == "Anonymous" ? judge.FirstName : judge.FullName;

                var judgeTextBlock = new TextBlock()
                {
                    Text = judgeName,
                    Style = Application.Current.Resources["ScoreViewerHeaderTextStyle"] as Style
                };

                control.ScoreGrid.Children.Add(judgeTextBlock);
                Grid.SetRow(judgeTextBlock, 0);
                Grid.SetColumn(judgeTextBlock, control.ScoreGrid.Children.Count - 2);
            }

            // for total score
            control.ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            var scoreTextBlock = new TextBlock()
            {
                Text = "Score",
                Style = Application.Current.Resources["ScoreViewerHeaderTextStyle"] as Style
            };
            control.ScoreGrid.Children.Add(scoreTextBlock);
            Grid.SetRow(scoreTextBlock, 0);
            Grid.SetColumn(scoreTextBlock, control.ScoreGrid.Children.Count - 1);

            #endregion
            #region CompetitorData
            var competitorNodes = new LinkedList<PrelimCompetitorScores>();

            foreach (var competitor in competition?.Competitors)
            {
                var prelimScores = competition.PrelimScores.Where(s => s.Competitor.FullName == competitor.FullName).ToList();

                competitorNodes.AddLast(new PrelimCompetitorScores
                {
                    Competitor = competitor,
                    PrelimScores = prelimScores,
                    TotalScore = prelimScores.Sum(s => Util.GetCallbackScoreValue(s.CallbackScore))
                });
            }

            competitorNodes = new(competitorNodes.OrderByDescending(s => s.TotalScore));

            int row = 1;
            int count = 1;
            int sameCount = 0;
            for (var competitorNode = competitorNodes.First; competitorNode != null; competitorNode = competitorNode.Next)
            {
                control.ScoreGrid.RowDefinitions.Add(new RowDefinition());
                bool promoted = control.PrelimCompetition.PromotedCompetitors.Contains(competitorNode.Value.Competitor);

                if (competitorNode.Previous != null)
                {
                    if (competitorNode.Value.TotalScore == competitorNode.Previous.Value.TotalScore)
                    {
                        sameCount++;
                    }
                    else
                    {
                        count = count + sameCount + 1;
                        sameCount = 0;
                    }
                }

                var promotedButton = new PromotedButton()
                {
                    CallbackScore = promoted ? Impartial.Enums.CallbackScore.Yes : Impartial.Enums.CallbackScore.No,
                    Editable = false
                };
                control.ScoreGrid.Children.Add(promotedButton);
                Grid.SetRow(promotedButton, row);
                Grid.SetColumn(promotedButton, PROMOTED_COLUMN);

                var competitorCountTextBlock = new TextBlock()
                {
                    Text = count.ToString(),
                    Style = promoted ? Application.Current.Resources["ScoreViewerCountTextStyleFinalist"] as Style : Application.Current.Resources["ScoreViewerCountTextStyleNonFinalist"] as Style
                };
                control.ScoreGrid.Children.Add(competitorCountTextBlock);
                Grid.SetRow(competitorCountTextBlock, row);
                Grid.SetColumn(competitorCountTextBlock, COUNT_COLUMN);

                var competitorBibTextBlock = new TextBlock()
                {
                    Text = "",
                    Style = promoted ? Application.Current.Resources["ScoreViewerBibTextStyleFinalist"] as Style : Application.Current.Resources["ScoreViewerBibTextStyleNonFinalist"] as Style
                };
                control.ScoreGrid.Children.Add(competitorBibTextBlock);
                Grid.SetRow(competitorBibTextBlock, row);
                Grid.SetColumn(competitorBibTextBlock, BIB_COLUMN);

                var nameTextBlock = new TextBlock()
                {
                    Text = competitorNode.Value.Competitor.FullName,
                    Style = promoted ? Application.Current.Resources["ScoreViewerTextStyleFinalist"] as Style : Application.Current.Resources["ScoreViewerTextStyleNonFinalist"] as Style
                };
                control.ScoreGrid.Children.Add(nameTextBlock);
                Grid.SetRow(nameTextBlock, row);
                Grid.SetColumn(nameTextBlock, COMPETITOR_COLUMN);

                // scores
                int scoreColumn = SCORE_COLUMN_START;
                foreach (var score in competitorNode.Value.PrelimScores.OrderBy(c => c.Judge.FullName))
                {
                    var callbackScoreViewer = new CallbackScoreViewer()
                    {
                        CallbackScore = score.CallbackScore,
                    };
                    control.ScoreGrid.Children.Add(callbackScoreViewer);
                    Grid.SetRow(callbackScoreViewer, row);
                    Grid.SetColumn(callbackScoreViewer, scoreColumn);

                    scoreColumn++;
                }

                var totalScoreTextBlock = new TextBlock()
                {
                    Text = Math.Round(competitorNode.Value.TotalScore, 1).ToString(),
                    Style = promoted ? Application.Current.Resources["ScoreViewerCountTextStyleFinalist"] as Style : Application.Current.Resources["ScoreViewerCountTextStyleNonFinalist"] as Style
                };
                control.ScoreGrid.Children.Add(totalScoreTextBlock);
                Grid.SetRow(totalScoreTextBlock, row);
                Grid.SetColumn(totalScoreTextBlock, scoreColumn);

                row++;
            }
            #endregion

            control.OnPropertyChanged(nameof(control.Count));
            control.OnPropertyChanged(nameof(control.RoundString));
        }
        #endregion

        public string Count => PrelimCompetition.Competitors.Count().ToString();
        public string RoundString => PrelimCompetition.Round.ToString();

        public PrelimCompetitionViewer()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };
        public void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
