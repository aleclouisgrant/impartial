using Impartial;
using ImpartialUI.Models;
using MongoDB.Driver.Linq;
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

            control.ScoreGrid.RowDefinitions.Add(new RowDefinition());
            control.ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            control.ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition());

            var countBorder = new Border()
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(1),
                Height = 24
            };

            var placeTextBlock = new TextBlock()
            {
                Text = "Count",
                FontWeight = FontWeights.Bold,
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(1)
            };
            countBorder.Child = placeTextBlock;

            control.ScoreGrid.Children.Add(countBorder);
            Grid.SetRow(countBorder, 0);
            Grid.SetColumn(countBorder, 0);

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

            control.ScoreGrid.Children.Add(competitorBorder);
            Grid.SetRow(competitorBorder, 0);
            Grid.SetColumn(competitorBorder, 1);

            var competition = (IPrelimCompetition)e.NewValue;
            if (competition == null)
                return;

            List<IJudge> judges = competition?.Judges.OrderBy(j => j.FullName).ToList() ?? new();
            
            // judge names
            foreach (var judge in judges)
            {
                control.ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                var border = new Border()
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(1),
                    Height = 24
                };

                var textBlock = new TextBlock()
                {
                    Text = judge.FullName,
                    FontWeight = FontWeights.Bold,
                    FontStyle = FontStyles.Italic,
                    Margin = new Thickness(1)
                };
                border.Child = textBlock;

                control.ScoreGrid.Children.Add(border);
                Grid.SetColumn(border, control.ScoreGrid.Children.Count - 1);
                Grid.SetRow(border, 0);
            }

            // for total score
            control.ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            var scoreBorder = new Border()
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(1),
                Height = 24
            };

            var scoreTextBlock = new TextBlock()
            {
                Text = "Score",
                Margin = new Thickness(1),
                FontWeight = FontWeights.Bold,
            };

            scoreBorder.Child = scoreTextBlock;

            control.ScoreGrid.Children.Add(scoreBorder);
            Grid.SetColumn(scoreBorder, control.ScoreGrid.Children.Count - 1);
            Grid.SetRow(scoreBorder, 0);

            List<ICompetitor> c = competition.Competitors ?? new();

            var competitors = new LinkedList<Tuple<ICompetitor, List<IPrelimScore>>>();

            foreach (var competitor in c)
            {
                var prelimScores = competition.PrelimScores.Where(s => s.Competitor.FullName == competitor.FullName).ToList();

                competitors.AddLast(new Tuple<ICompetitor, List<IPrelimScore>>(
                    item1: competitor, 
                    item2: prelimScores));
            }

            competitors = new LinkedList<Tuple<ICompetitor, List<IPrelimScore>>>(competitors.OrderBy(s => s.Item2.FirstOrDefault()));

            int i = 1;
            int count = 1;
            int sameCount = 0;
            for (var competitor = competitors.First; competitor != null; competitor = competitor.Next)
            {
                control.ScoreGrid.RowDefinitions.Add(new RowDefinition());
                bool finaled = control.PrelimCompetition.PromotedCompetitors.Contains(competitor.Value.Item1);

                // placement
                var competitorCountBorder = new Border()
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(1),
                    Height = 24
                };

                if (competitor.Previous != null)
                {
                    if (competitor.Value.Item2.Sum(s => (int)s.CallbackScore) == competitor.Previous.Value.Item2.Sum(s => (int)s.CallbackScore))
                    {
                        sameCount++;
                    }
                    else
                    {
                        count = count + sameCount + 1;
                        sameCount = 0;
                    }
                }

                var competitorCountTextBlock = new TextBlock()
                {
                    Text = count.ToString(),
                    Margin = new Thickness(1)
                };

                competitorCountBorder.Child = competitorCountTextBlock;

                control.ScoreGrid.Children.Add(competitorCountBorder);
                Grid.SetColumn(competitorCountBorder, 0);
                Grid.SetRow(competitorCountBorder, i);

                // name
                var nameBorder = new Border()
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(1),
                    Height = 24
                };

                var nameTextBlock = new TextBlock()
                {
                    Text = competitor.Value.Item1.FullName,
                    Margin = new Thickness(1)
                };

                nameBorder.Child = nameTextBlock;

                if (finaled)
                {
                    competitorCountTextBlock.FontWeight = FontWeights.Bold;
                    nameTextBlock.FontWeight = FontWeights.Bold;
                }

                control.ScoreGrid.Children.Add(nameBorder);
                Grid.SetColumn(nameBorder, 1);
                Grid.SetRow(nameBorder, i);

                // scores
                int j = 2;
                foreach (var score in competitor.Value.Item2.OrderBy(c => c.Judge.FullName))
                {
                    var border = new Border()
                    {
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(1),
                        Margin = new Thickness(1),
                        Height = 24
                    };

                    var textBlock = new TextBlock()
                    {
                        Text = Util.CallbackScoreToString(score.CallbackScore),
                        Margin = new Thickness(1)
                    };

                    if (finaled)
                        textBlock.FontWeight = FontWeights.Bold;

                    border.Child = textBlock;

                    control.ScoreGrid.Children.Add(border);
                    Grid.SetColumn(border, j);
                    Grid.SetRow(border, i);
                    j++;
                }

                var callbackBorder = new Border()
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(1),
                    Height = 24
                };

                var callbackTextBlock = new TextBlock()
                {
                    Text = (competitor.Value.Item2.Sum(s => (int)s.CallbackScore) / 10.0f).ToString(),
                    Margin = new Thickness(1)
                };

                if (finaled)
                    callbackTextBlock.FontWeight = FontWeights.Bold;

                callbackBorder.Child = callbackTextBlock;

                control.ScoreGrid.Children.Add(callbackBorder);
                Grid.SetColumn(callbackBorder, j);
                Grid.SetRow(callbackBorder, i);

                i++;
            }
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
