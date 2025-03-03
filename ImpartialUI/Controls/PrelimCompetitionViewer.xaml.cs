using Impartial;
using Impartial.Enums;
using ImpartialUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
            public string BibNumberString { get; set; }
            public int BibNumberValue
            {
                get
                {
                    if (Int32.TryParse(BibNumberString, out int n))
                        return n;

                    return -1;
                }
            }
            public ICompetitor Competitor { get; set; }
            public List<IPrelimScore> PrelimScores { get; set; } = new List<IPrelimScore>();
            public double TotalScore { get; set; }
            public CallbackScore Callback { get; set; } = CallbackScore.Unscored;
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
            /// Promoted, Count, Bib, Competitor, [Judges], Sum
            control.ScoreGrid.RowDefinitions.Add(new RowDefinition());
            control.ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); // Promoted
            control.ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); // Count
            control.ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); // Bib
            control.ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition()); // Competitor

            var headerBorder = new Border()
            {
                Style = Application.Current.Resources["ScoreViewerHeaderBorderStyle"] as Style
            };
            control.ScoreGrid.Children.Add(headerBorder);
            Grid.SetRow(headerBorder, 0);
            Grid.SetColumn(headerBorder, 0);
            Grid.SetColumnSpan(headerBorder, 100);

            var promotedTextBlock = new TextBlock()
            {
                Text = "",
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

                if (judge.FirstName == "Anonymous" || judge.LastName == string.Empty)
                {
                    var judgeTextBlock = new TextBlock()
                    {
                        Text = judge.FirstName,
                        Style = Application.Current.Resources["ScoreViewerJudgeHeaderTextStyle"] as Style
                    };

                    control.ScoreGrid.Children.Add(judgeTextBlock);
                    Grid.SetRow(judgeTextBlock, 0);
                    Grid.SetColumn(judgeTextBlock, control.ScoreGrid.Children.Count - 2);
                }
                else
                {
                    int margin = -2;

                    var judgeNameStackPanel = new StackPanel()
                    {
                        Orientation = Orientation.Vertical,
                        VerticalAlignment = VerticalAlignment.Center,
                    };

                    var judgeFirstNameTextBlock = new TextBlock()
                    {
                        Text = judge.FirstName,
                        Style = Application.Current.Resources["ScoreViewerJudgeHeaderTextStyle"] as Style,
                        Margin = new Thickness(0, 0, 0, margin)
                    };

                    var judgeLastNameTextBlock = new TextBlock()
                    {
                        Text = judge.LastName,
                        Style = Application.Current.Resources["ScoreViewerJudgeHeaderTextStyle"] as Style,
                        Margin = new Thickness(0, margin, 0, 0)
                    };

                    judgeNameStackPanel.Children.Add(judgeFirstNameTextBlock);
                    judgeNameStackPanel.Children.Add(judgeLastNameTextBlock);

                    control.ScoreGrid.Children.Add(judgeNameStackPanel);
                    Grid.SetRow(judgeNameStackPanel, 0);
                    Grid.SetColumn(judgeNameStackPanel, control.ScoreGrid.Children.Count - 2);
                }
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
            #region JudgeScores
            var competitorNodes = new LinkedList<PrelimCompetitorScores>();

            foreach (var competitor in competition?.Competitors)
            {
                var prelimScores = competition.PrelimScores.Where(s => s.Competitor.FullName == competitor.FullName).ToList();

                string bibNumberString = competition.CompetitorRegistrations.FirstOrDefault(c => c.Competitor == competitor)?.BibNumber ?? "0";

                var callback = competition.PromotedCompetitors.Contains(competitor) ? CallbackScore.Yes : CallbackScore.No;
                if (callback == CallbackScore.No)
                {
                    if (competition.Alternate1 == competitor)
                    {
                        callback = CallbackScore.Alt1;
                    }
                    else if (competition.Alternate2 == competitor)
                    {
                        callback = CallbackScore.Alt2; 
                    }
                }

                competitorNodes.AddLast(new PrelimCompetitorScores
                {
                    BibNumberString = bibNumberString,
                    Competitor = competitor,
                    PrelimScores = prelimScores,
                    TotalScore = prelimScores.Sum(s => Util.GetCallbackScoreValue(s.CallbackScore)),
                    Callback = callback
                });
            }

            competitorNodes = new(competitorNodes.OrderByDescending(s => s.TotalScore).ThenByDescending(s => s.Callback).ThenBy(s => s.BibNumberValue));

            int row = 1;
            int count = 1;
            int sameCount = 0;
            for (var competitorNode = competitorNodes.First; competitorNode != null; competitorNode = competitorNode.Next)
            {
                control.ScoreGrid.RowDefinitions.Add(new RowDefinition());
                bool promoted = competition.PromotedCompetitors.Contains(competitorNode.Value.Competitor);

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
                    Margin = new Thickness(20, 0, 0, 0),
                    CallbackScore = promoted ? Impartial.Enums.CallbackScore.Yes : Impartial.Enums.CallbackScore.No,
                    Editable = false
                };

                if (competitorNode.Value.Competitor == competition.Alternate1)
                {
                    promotedButton.CallbackScore = Impartial.Enums.CallbackScore.Alt1;
                }
                if (competitorNode.Value.Competitor == competition.Alternate2)
                {
                    promotedButton.CallbackScore = Impartial.Enums.CallbackScore.Alt2;
                }

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
                    Text = competitorNode.Value.BibNumberString,
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
        }
        #endregion

        public PrelimCompetitionViewer()
        {
            InitializeComponent();
        }
    }
}
