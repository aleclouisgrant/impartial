using Impartial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ImpartialUI.Controls
{
    public partial class CompetitionViewer : UserControl
    {
        #region DependencyProperties

        public static readonly DependencyProperty CompetitionProperty = DependencyProperty.Register(
            nameof(Competition),
            typeof(Competition),
            typeof(CompetitionViewer),
            new FrameworkPropertyMetadata(new Competition(), OnCompetitionPropertyChanged));
        public Competition Competition
        {
            get { return (Competition)GetValue(CompetitionProperty); }
            set { SetValue(CompetitionProperty, value); }
        }
        private static void OnCompetitionPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var viewer = (CompetitionViewer)source;
            viewer.ScoreGrid.Children.Clear();

            var placeBorder = new Border()
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(1)
            };

            var placeTextBlock = new TextBlock()
            {
                Text = "Place",
                FontWeight = FontWeights.Bold,
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(1)
            };
            placeBorder.Child = placeTextBlock;

            viewer.ScoreGrid.Children.Add(placeBorder);
            Grid.SetRow(placeBorder, 0);
            Grid.SetColumn(placeBorder, 0);

            var competitorBorder = new Border()
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(1)
            };

            var competitorTextBlock = new TextBlock()
            {
                Text = "Competitor",
                FontWeight = FontWeights.Bold,
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(1)
            };
            competitorBorder.Child = competitorTextBlock;

            viewer.ScoreGrid.Children.Add(competitorBorder);
            Grid.SetRow(competitorBorder, 0);
            Grid.SetColumn(competitorBorder, 1);

            var competition = (Competition)e.NewValue;

            var judges = competition.Judges.OrderBy(j => j.FullName);
            var couples = competition.Couples;

            //competition.Scores = competition.Scores.OrderBy(s => s.ActualPlacement).ThenBy(s => s.Judge.FullName).ToList();

            // judge names
            foreach (var judge in judges)
            {
                judge.Scores = competition.Scores.Where(s => s.Judge.Id == judge.Id).ToList();

                viewer.ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                var border = new Border()
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(1)
                };

                var textBlock = new TextBlock()
                {
                    Text = judge.FullName + " (" + judge.Accuracy.ToString() + ")" + "(" + judge.Top5Accuracy.ToString() + ")",
                    FontWeight = FontWeights.Bold,
                    FontStyle = FontStyles.Italic,
                    Margin = new Thickness(1)
                };
                border.Child = textBlock;

                viewer.ScoreGrid.Children.Add(border);
                Grid.SetColumn(border, viewer.ScoreGrid.Children.Count - 1);
                Grid.SetRow(border, 0);
            }

            foreach (var couple in couples)
            {
                couple.Scores = couple.Scores.OrderBy(s => s.Judge.FullName).ToList();

                viewer.ScoreGrid.RowDefinitions.Add(new RowDefinition());

                // placement
                var placementBorder = new Border()
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(1)
                };

                var placementTextBlock = new TextBlock()
                {
                    Text = couple.ActualPlacement.ToString(),
                    Margin = new Thickness(1)
                };

                placementBorder.Child = placementTextBlock;

                viewer.ScoreGrid.Children.Add(placementBorder);
                Grid.SetColumn(placementBorder, 0);
                Grid.SetRow(placementBorder, couple.ActualPlacement);

                // names
                var nameBorder = new Border()
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(1)
                };

                var nameTextBlock = new TextBlock()
                {
                    Text = couple.Leader.FullName + " and " + couple.Follower.FullName,
                    Margin = new Thickness(1)
                };

                nameBorder.Child = nameTextBlock;

                viewer.ScoreGrid.Children.Add(nameBorder);
                Grid.SetColumn(nameBorder, 1);
                Grid.SetRow(nameBorder, couple.ActualPlacement);

                // scores
                for (int i = 0; i < couple.Scores.Count; i++)
                {
                    var score = couple.Scores[i];

                    var border = new Border()
                    {
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(1),
                        Margin = new Thickness(1)
                    };

                    var textBlock = new TextBlock()
                    {
                        Margin = new Thickness(1)
                    };

                    if (score.Placement == score.ActualPlacement){
                        textBlock.Text = score.Placement.ToString();
                    }
                    else
                    {
                        textBlock.Inlines.Add(new Run()
                        {
                            Text = score.Placement.ToString()
                        });
                        textBlock.Inlines.Add(new Run()
                        {
                            Text = " (" + (-1 * Math.Abs(score.Placement - score.ActualPlacement)).ToString() + ")",
                            Foreground = Brushes.Red
                        });
                    }

                    border.Child = textBlock;

                    viewer.ScoreGrid.Children.Add(border);
                    Grid.SetColumn(border, i + 2);
                    Grid.SetRow(border, couple.ActualPlacement);
                }
            }
        }
        #endregion

        public CompetitionViewer()
        {
            InitializeComponent();
        }
    }
}
