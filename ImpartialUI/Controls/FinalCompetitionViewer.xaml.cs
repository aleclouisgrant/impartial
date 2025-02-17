using Impartial;
using ImpartialUI.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ImpartialUI.Controls
{
    public partial class FinalCompetitionViewer : UserControl
    {
        private static readonly int PLACEMENT_COLUMN = 0;
        private static readonly int BIB_COLUMN = 1;
        private static readonly int COMPETITORS_COLUMN = 2;
        private static readonly int SCORE_COLUMN_START = 3;

        #region DependencyProperties
        public static readonly DependencyProperty ShowJudgeAccuracyProperty = DependencyProperty.Register(
            nameof(ShowJudgeAccuracy),
            typeof(bool),
            typeof(FinalCompetitionViewer),
            new FrameworkPropertyMetadata(false, OnFinalCompetitionPropertyChanged));
        public bool ShowJudgeAccuracy
        {
            get { return (bool)GetValue(ShowJudgeAccuracyProperty); }
            set { SetValue(ShowJudgeAccuracyProperty, value); }
        }

        public static readonly DependencyProperty FinalCompetitionProperty = DependencyProperty.Register(
            nameof(FinalCompetition),
            typeof(IFinalCompetition),
            typeof(FinalCompetitionViewer),
            new FrameworkPropertyMetadata(new FinalCompetition(), OnFinalCompetitionPropertyChanged));
        public IFinalCompetition FinalCompetition
        {
            get { return (IFinalCompetition)GetValue(FinalCompetitionProperty); }
            set { SetValue(FinalCompetitionProperty, value); }
        }
        private static void OnFinalCompetitionPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var control = (FinalCompetitionViewer)source;
            control.ScoreGrid.Children.Clear();
            control.ScoreGrid.RowDefinitions.Clear();
            control.ScoreGrid.ColumnDefinitions.Clear();

            #region Header
            /// Placement, Bib, Competitors, [Judges], Scoring

            control.ScoreGrid.RowDefinitions.Add(new RowDefinition());
            control.ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); // Placement
            control.ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); // Bib
            control.ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition()); // Competitors

            var headerBorder = new Border()
            {
                Style = Application.Current.Resources["ScoreViewerHeaderBorderStyle"] as Style
            };
            control.ScoreGrid.Children.Add(headerBorder);
            Grid.SetRow(headerBorder, 0);
            Grid.SetColumn(headerBorder, 0);
            Grid.SetColumnSpan(headerBorder, int.MaxValue);

            var placeTextBlock = new TextBlock()
            {
                Text = "Place",
                Style = Application.Current.Resources["ScoreViewerHeaderTextStyle"] as Style
            };
            control.ScoreGrid.Children.Add(placeTextBlock);
            Grid.SetRow(placeTextBlock, 0);
            Grid.SetColumn(placeTextBlock, PLACEMENT_COLUMN);

            var bibTextBlock = new TextBlock()
            {
                Text = "Bib",
                Style = Application.Current.Resources["ScoreViewerHeaderTextStyle"] as Style
            };
            control.ScoreGrid.Children.Add(bibTextBlock);
            Grid.SetRow(bibTextBlock, 0);
            Grid.SetColumn(bibTextBlock, BIB_COLUMN);

            var competitorsTextBlock = new TextBlock()
            {
                Text = "Competitors",
                Style = Application.Current.Resources["ScoreViewerHeaderTextStyle"] as Style
            };
            control.ScoreGrid.Children.Add(competitorsTextBlock);
            Grid.SetRow(competitorsTextBlock, 0);
            Grid.SetColumn(competitorsTextBlock, COMPETITORS_COLUMN);

            var finalCompetition = (IFinalCompetition)e.NewValue;
            if (finalCompetition == null)
                return;

            var judges = finalCompetition.Judges?.OrderBy(j => j.FullName);
            var couples = finalCompetition.Couples;

            // judge names
            for (int judgeIndex = 0; judgeIndex < judges.Count(); judgeIndex++)
            {
                var judge = judges.ElementAt(judgeIndex);
                judge.Scores = finalCompetition.FinalScores.Where(s => s.Judge.JudgeId == judge.JudgeId).ToList();

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
                    Grid.SetColumn(judgeTextBlock, SCORE_COLUMN_START + judgeIndex);
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
                    Grid.SetColumn(judgeNameStackPanel, SCORE_COLUMN_START + judgeIndex);
                }

                var judgeBorder = new Border()
                {
                    Style = Application.Current.Resources["ScoreViewerFinalsJudgeBorderStyle"] as Style,
                };
                control.ScoreGrid.Children.Add(judgeBorder);
                Grid.SetRow(judgeBorder, 0);
                Grid.SetRowSpan(judgeBorder, int.MaxValue);
                Grid.SetColumn(judgeBorder, SCORE_COLUMN_START + judgeIndex);
            }

            #endregion
            #region JudgeScores
            foreach (var couple in couples)
            {
                couple.Scores = couple.Scores.OrderBy(s => s.Judge.FullName).ToList();

                control.ScoreGrid.RowDefinitions.Add(new RowDefinition());

                // placement
                var placementTextBlock = new TextBlock()
                {
                    Text = couple.Placement.ToString(),
                    Style = Application.Current.Resources["ScoreViewerPlacementTextStyle"] as Style,
                };
                control.ScoreGrid.Children.Add(placementTextBlock);
                Grid.SetRow(placementTextBlock, couple.Placement);
                Grid.SetColumn(placementTextBlock, PLACEMENT_COLUMN);

                // bib numbers
                var leaderBibNumber = couple.LeaderRegistration?.BibNumber ?? "000";
                var followerBibNumber = couple.FollowerRegistration?.BibNumber ?? "000";
                var bibNumbersTextBlock = new TextBlock()
                {
                    Text = leaderBibNumber + " / " + followerBibNumber,
                    Style = Application.Current.Resources["ScoreViewerBibTextStyle"] as Style
                };
                control.ScoreGrid.Children.Add(bibNumbersTextBlock);
                Grid.SetRow(bibNumbersTextBlock, couple.Placement);
                Grid.SetColumn(bibNumbersTextBlock, BIB_COLUMN);

                // names
                var namesTextBlock = new TextBlock()
                {
                    Text = couple.Leader.FullName + " and " + couple.Follower.FullName,
                    Style = Application.Current.Resources["ScoreViewerCompetitorNamesTextStyle"] as Style
                };
                control.ScoreGrid.Children.Add(namesTextBlock);
                Grid.SetRow(namesTextBlock, couple.Placement);
                Grid.SetColumn(namesTextBlock, COMPETITORS_COLUMN);

                // scores
                for (int i = 0; i < couple.Scores.Count; i++)
                {
                    var score = couple.Scores[i];

                    var scoreTextBlock = new TextBlock()
                    {
                        Text = score.Score.ToString(),
                        Style = Application.Current.Resources["ScoreViewerScoresTextStyle"] as Style
                    };

                    if (control.ShowJudgeAccuracy && score.Score != score.Placement)
                    {
                        scoreTextBlock.Inlines.Add(new Run()
                        {
                            Text = " (" + (-1 * Math.Abs(score.Score - score.Placement)).ToString() + ")",
                            Foreground = Brushes.Red
                        });
                    }

                    control.ScoreGrid.Children.Add(scoreTextBlock);
                    Grid.SetRow(scoreTextBlock, couple.Placement);
                    Grid.SetColumn(scoreTextBlock, SCORE_COLUMN_START + i);
                }

                var competitorBorder = new Border()
                {
                    Style = Application.Current.Resources["ScoreViewerFinalsCompetitorBorderStyle"] as Style
                };
                control.ScoreGrid.Children.Add(competitorBorder);
                Grid.SetRow(competitorBorder, couple.Placement);
                Grid.SetColumn(competitorBorder, 0);
                Grid.SetColumnSpan(competitorBorder, int.MaxValue);
            }
            #endregion
        }
        #endregion

        public FinalCompetitionViewer()
        {
            InitializeComponent();
        }
    }
}
