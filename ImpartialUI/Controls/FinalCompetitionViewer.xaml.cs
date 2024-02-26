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
        #region DependencyProperties

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
            var viewer = (FinalCompetitionViewer)source;
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
                Text = "Competitor",
                FontWeight = FontWeights.Bold,
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(1)
            };
            competitorBorder.Child = competitorTextBlock;

            viewer.ScoreGrid.Children.Add(competitorBorder);
            Grid.SetRow(competitorBorder, 0);
            Grid.SetColumn(competitorBorder, 1);

            var finalCompetition = (IFinalCompetition)e.NewValue;
            viewer.TitleTextBlock.Text = "";
            if (finalCompetition == null)
                return;

            //TODO: date and division are being incorrectly passed for some reason
            //viewer.TitleTextBlock.Text = "Jack & Jill " + Util.DivisionToString(finalCompetition.Division) + " Finals";
            viewer.TitleTextBlock.Text = "Jack & Jill Finals";

            var judges = finalCompetition.Judges?.OrderBy(j => j.FullName);
            var couples = finalCompetition.Couples;

            // judge names
            foreach (var judge in judges)
            {
                judge.Scores = finalCompetition.FinalScores.Where(s => s.Judge.Id == judge.Id).ToList();

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

            foreach (var couple in couples)
            {
                couple.Scores = couple.Scores.OrderBy(s => s.Judge.FullName).ToList();

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
                    Text = couple.Placement.ToString(),
                    Margin = new Thickness(1)
                };

                placementBorder.Child = placementTextBlock;

                viewer.ScoreGrid.Children.Add(placementBorder);
                Grid.SetColumn(placementBorder, 0);
                Grid.SetRow(placementBorder, couple.Placement);

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
                    Text = couple.Leader.FullName + " and " + couple.Follower.FullName,
                    Margin = new Thickness(1)
                };

                nameBorder.Child = nameTextBlock;

                viewer.ScoreGrid.Children.Add(nameBorder);
                Grid.SetColumn(nameBorder, 1);
                Grid.SetRow(nameBorder, couple.Placement);

                // scores
                for (int i = 0; i < couple.Scores.Count; i++)
                {
                    var score = couple.Scores[i];

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

                    if (score.Score == score.Score){
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
                            Text = " (" + (-1 * Math.Abs(score.Score - score.Score)).ToString() + ")",
                            Foreground = Brushes.Red
                        });
                    }

                    border.Child = textBlock;

                    viewer.ScoreGrid.Children.Add(border);
                    Grid.SetColumn(border, i + 2);
                    Grid.SetRow(border, couple.Placement);
                }
            }
        }
        #endregion

        public FinalCompetitionViewer()
        {
            InitializeComponent();
        }
    }
}
