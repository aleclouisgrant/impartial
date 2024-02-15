using Impartial;
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
    public partial class PrelimsViewer : UserControl
    {
        #region DependencyProperties

        public static readonly DependencyProperty CompetitionProperty = DependencyProperty.Register(
            nameof(Competition),
            typeof(Competition),
            typeof(PrelimsViewer),
            new FrameworkPropertyMetadata(new Competition(), OnCompetitionPropertyChanged));
        public Competition Competition
        {
            get { return (Competition)GetValue(CompetitionProperty); }
            set { SetValue(CompetitionProperty, value); }
        }
        private static void OnCompetitionPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var control = (PrelimsViewer)source;
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

            var competition = (Competition)e.NewValue;
            if (competition == null)
                return;

            List<Judge> judges = new List<Judge>();

            if (control.Role == Role.Leader)
                judges = competition.PrelimLeaderJudges(control.Round)?.OrderBy(j => j.FullName).ToList();
            else if (control.Role == Role.Follower)
                judges = competition.PrelimFollowerJudges(control.Round)?.OrderBy(j => j.FullName).ToList();

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

            List<Competitor> c = new List<Competitor>();

            if (control.Role == Role.Leader)
                c = competition.PrelimLeaders(control.Round);
            else if (control.Role == Role.Follower)
                c = competition.PrelimFollowers(control.Round);

            var competitors = new LinkedList<Tuple<Competitor, List<PrelimScore>>>();

            if (control.Role == Role.Leader)
            {
                foreach (var competitor in c)
                {
                    competitors.AddLast(new Tuple<Competitor, List<PrelimScore>>(competitor, competition.LeaderPrelimScores.Where(s => s.Competitor.FullName == competitor.FullName && s.Round == control.Round).ToList()));
                }
            }
            else if (control.Role == Role.Follower)
            {
                foreach (var competitor in c)
                {
                    competitors.AddLast(new Tuple<Competitor, List<PrelimScore>>(competitor, competition.FollowerPrelimScores.Where(s => s.Competitor.FullName == competitor.FullName && s.Round == control.Round).ToList()));
                }
            }

            competitors = new LinkedList<Tuple<Competitor, List<PrelimScore>>>(competitors.OrderBy(s => s.Item2.FirstOrDefault().RawScore));

            int i = 1;
            int count = 1;
            int sameCount = 0;
            for (LinkedListNode<Tuple<Competitor, List<PrelimScore>>> competitor = competitors.First; competitor != null; competitor = competitor.Next)
            {
                control.ScoreGrid.RowDefinitions.Add(new RowDefinition());
                bool finaled = competitor.Value.Item2.FirstOrDefault().Finaled;

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

        public static readonly DependencyProperty RoleProperty = DependencyProperty.Register(
            nameof(Role),
            typeof(Role),
            typeof(PrelimsViewer),
            new FrameworkPropertyMetadata(Role.Leader, OnRolePropertyChanged));
        public Role Role
        {
            get { return (Role)GetValue(RoleProperty); }
            set { SetValue(RoleProperty, value); }
        }
        private static void OnRolePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            OnCompetitionPropertyChanged(source, 
                new DependencyPropertyChangedEventArgs(
                    CompetitionProperty, 
                    ((PrelimsViewer)source).Competition, 
                    ((PrelimsViewer)source).Competition));
        }

        public static readonly DependencyProperty RoundProperty = DependencyProperty.Register(
            nameof(Round),
            typeof(int),
            typeof(PrelimsViewer),
            new FrameworkPropertyMetadata(1, OnRoundPropertyChanged));
        public int Round
        {
            get { return (int)GetValue(RoundProperty); }
            set { SetValue(RoundProperty, value); }
        }
        private static void OnRoundPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            OnCompetitionPropertyChanged(source,
                new DependencyPropertyChangedEventArgs(
                    CompetitionProperty,
                    ((PrelimsViewer)source).Competition,
                    ((PrelimsViewer)source).Competition));
        }

        #endregion

        public string Count
        {
            get
            {
                if (Role == Role.Leader)
                    return Competition.TotalLeaders.ToString();
                else if (Role == Role.Follower)
                    return Competition.TotalFollowers.ToString();
                else
                    return string.Empty;
            }
        }
        public string RoundString
        {
            get 
            { 
                switch (Round)
                {
                    case 2:
                        return "Semis";
                    default:
                    case 1:
                        return "Prelims";
                } 
            }
        }

        public PrelimsViewer()
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
