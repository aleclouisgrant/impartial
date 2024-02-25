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
    /// <summary>
    /// Interaction logic for ScoreViewer.xaml
    /// </summary>
    public partial class ScoreViewer : UserControl
    {
        public static readonly DependencyProperty ScoresProperty = DependencyProperty.Register(
            nameof(Scores),
            typeof(IFinalScore),
            typeof(ScoreViewer),
            new FrameworkPropertyMetadata(null, OnScorePropertyChanged));
        public List<IFinalScore> Scores
        {
            get { return (List<IFinalScore>)GetValue(ScoresProperty); }
            set { SetValue(ScoresProperty, value); }
        }
        private static void OnScorePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var viewer = (ScoreViewer)source;
            viewer.ScoreGrid.Children.Clear();

            var placeBorder = new Border()
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(1),
                Height = 24
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
                Margin = new Thickness(1),
                Height = 24
            };

            var competitorTextBlock = new TextBlock()
            {
                Text = "Competitors",
                FontWeight = FontWeights.Bold,
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(1)
            };
            competitorBorder.Child = competitorTextBlock;

            viewer.ScoreGrid.Children.Add(competitorBorder);
            Grid.SetRow(competitorBorder, 0);
            Grid.SetColumn(competitorBorder, 1);

            var scores = (List<IFinalScore>)e.NewValue;
            if (scores == null)
                return;

            List<IJudge> judges = new();

            foreach (IFinalScore score in scores)
            {
                judges.Add(score.Judge);
            }

            judges = judges.OrderBy(j => j.FullName).ToList();
            scores.OrderBy(s => s.Judge.FullName).ToList();

            viewer.ScoreGrid.RowDefinitions.Add(new RowDefinition());

            // placement
            var placementBorder = new Border()
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(1),
                Height = 24
            };

            var placementTextBlock = new TextBlock()
            {
                Text = scores.First().Placement.ToString(),
                Margin = new Thickness(1)
            };

            placementBorder.Child = placementTextBlock;

            viewer.ScoreGrid.Children.Add(placementBorder);
            Grid.SetColumn(placementBorder, 0);
            Grid.SetRow(placementBorder, 1);

            // names
            var nameBorder = new Border()
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(1),
                Height = 24
            };

            var nameTextBlock = new TextBlock()
            {
                Text = scores.First().Leader.FullName + " and " + scores.First().Follower.FullName,
                Margin = new Thickness(1)
            };

            nameBorder.Child = nameTextBlock;

            viewer.ScoreGrid.Children.Add(nameBorder);
            Grid.SetColumn(nameBorder, 1);
            Grid.SetRow(nameBorder, 1);

            // judge names
            foreach (var judge in judges)
            {
                judge.Scores = scores.Where(s => s.Judge.Id == judge.Id).ToList();

                viewer.ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                var border = new Border()
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(1),
                    Height = 24
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

            // scores
            for (int i = 0; i < scores.Count; i++)
            {
                var score = scores[i];

                var border = new Border()
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(1),
                    Height = 24
                };

                var textBlock = new TextBlock()
                {
                    Margin = new Thickness(1)
                };

                if (score.Score == score.Placement)
                {
                    textBlock.Text = score.Score.ToString();
                }
                else
                {
                    textBlock.Inlines.Add(new Run()
                    {
                        Text = score.Score.ToString()
                    });
                    textBlock.Inlines.Add(new Run()
                    {
                        Text = " (" + (-1 * Math.Abs(score.Score - score.Placement)).ToString() + ")",
                        Foreground = Brushes.Red
                    });
                }

                border.Child = textBlock;

                viewer.ScoreGrid.Children.Add(border);
                Grid.SetColumn(border, i + 2);
                Grid.SetRow(border, 1);
            }
        }


        public ScoreViewer()
        {
            InitializeComponent();
        }
    }
}
