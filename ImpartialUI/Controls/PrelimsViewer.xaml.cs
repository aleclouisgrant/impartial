using Impartial;
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
                Margin = new Thickness(1)
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

            control.ScoreGrid.Children.Add(competitorBorder);
            Grid.SetRow(competitorBorder, 0);
            Grid.SetColumn(competitorBorder, 1);

            var competition = (Competition)e.NewValue;
            if (competition == null)
                return;

            List<Judge> judges = new List<Judge>();

            if (control.Role == Role.Leader)
                judges = competition.PrelimLeaderJudges?.OrderBy(j => j.FullName).ToList();
            else if (control.Role == Role.Follower)
                judges = competition.PrelimFollowerJudges?.OrderBy(j => j.FullName).ToList();

            // judge names
            foreach (var judge in judges)
            {
                List<PrelimScore> scores = new List<PrelimScore>();
                if (control.Role == Role.Leader)
                    scores = competition.LeaderPrelimScores.Where(s => s.Judge.Id == judge.Id).ToList();
                else if (control.Role == Role.Follower)
                    scores = competition.FollowerPrelimScores.Where(s => s.Judge.Id == judge.Id).ToList();

                control.ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

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

                control.ScoreGrid.Children.Add(border);
                Grid.SetColumn(border, control.ScoreGrid.Children.Count - 1);
                Grid.SetRow(border, 0);
            }

            List<Competitor> c = new List<Competitor>();

            if (control.Role == Role.Leader)
                c = competition.PrelimLeaders;
            else if (control.Role == Role.Follower)
                c = competition.PrelimFollowers;

            var competitors = new List<Tuple<Competitor, List<PrelimScore>>>();

            if (control.Role == Role.Leader)
            {
                foreach (var competitor in c)
                {
                    competitors.Add(new Tuple<Competitor, List<PrelimScore>>(competitor, competition.LeaderPrelimScores.Where(s => s.Competitor.FullName == competitor.FullName).ToList()));
                }
            }
            else if (control.Role == Role.Follower)
            {
                foreach (var competitor in c)
                {
                    competitors.Add(new Tuple<Competitor, List<PrelimScore>>(competitor, competition.FollowerPrelimScores.Where(s => s.Competitor.FullName == competitor.FullName).ToList()));
                }
            }

            competitors = competitors.OrderByDescending(s => s.Item2.Sum(x => (int)x.CallbackScore)).ToList();

            int i = 1;
            foreach (var competitor in competitors)
            {
                control.ScoreGrid.RowDefinitions.Add(new RowDefinition());
                int count = 0; //todo
                bool finaled = competitor.Item2.FirstOrDefault().Finaled;

                // placement
                var competitorCountBorder = new Border()
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(1)
                };

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
                    Margin = new Thickness(1)
                };

                var nameTextBlock = new TextBlock()
                {
                    Text = competitor.Item1.FullName,
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
                foreach (var score in competitor.Item2)
                {
                    var border = new Border()
                    {
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(1),
                        Margin = new Thickness(1)
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
                i++;
            }
            control.OnPropertyChanged(nameof(Count));
            control.OnPropertyChanged(nameof(Role));
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
