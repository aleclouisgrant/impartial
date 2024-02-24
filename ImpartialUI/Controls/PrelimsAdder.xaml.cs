using Impartial;
using Impartial.Enums;
using ImpartialUI.Implementations.Models;
using ImpartialUI.Models;
using ScottPlot.Renderable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ImpartialUI.Controls
{
    public partial class PrelimsAdder : UserControl
    {
        #region DependencyProperties
        public static readonly DependencyProperty CompetitionProperty = DependencyProperty.Register(
            nameof(Competition),
            typeof(ICompetition),
            typeof(PrelimsAdder),
            new FrameworkPropertyMetadata(new ICompetition(), OnCompetitionPropertyChanged));
        public ICompetition Competition
        {
            get { return (ICompetition)GetValue(CompetitionProperty); }
            set { SetValue(CompetitionProperty, value); }
        }
        private static void OnCompetitionPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var control = (PrelimsAdder)source;
            var competition = (ICompetition)e.NewValue;

            if (competition == null)
                return;

            var judges = new List<IJudge>();
            List<Tuple<Competitor, List<PrelimScore>>> c = new();

            if (control.Role == Role.Leader)
            {
                List<Competitor> competitors = competition.PrelimLeaders(control.Round);
                foreach (var competitor in competitors)
                {
                    c.Add(new(competitor, competition.LeaderPrelimScores.Where(s => s.Competitor.FullName == competitor.FullName && s.Round == control.Round).OrderBy(c => c.Judge.FullName).ToList()));
                }
                judges = competition.PrelimLeaderJudges(control.Round).OrderBy(j => j.FullName).ToList();
            }
            else if (control.Role == Role.Follower)
            {
                List<Competitor> competitors = competition.PrelimFollowers(control.Round);
                foreach (var competitor in competitors)
                {
                    c.Add(new(competitor, competition.FollowerPrelimScores.Where(s => s.Competitor.FullName == competitor.FullName && s.Round == control.Round).OrderBy(c => c.Judge.FullName).ToList()));
                }
                judges = competition.PrelimFollowerJudges(control.Round).OrderBy(j => j.FullName).ToList();
            }

            c = c.OrderBy(cc => cc.Item2.FirstOrDefault().RawScore).ToList();

            foreach (var score in competition.LeaderPrelimScores)
            {
                score.Competition = competition;
            }
            foreach (var score in competition.FollowerPrelimScores)
            {
                score.Competition = competition;
            }

            control.Clear();

            control._scores = new PrelimScore[c.Count, judges.Count];

            for (int competitorIndex = 0; competitorIndex < c.Count; competitorIndex++)
            {
                for (int judgeIndex = 0; judgeIndex < judges.Count; judgeIndex++)
                {
                    control._scores[competitorIndex, judgeIndex] = c[competitorIndex].Item2.ElementAt(judgeIndex);
                }
            }

            foreach (var judge in judges)
            {
                control.AddJudge(judge);
            }

            foreach (var competitor in c)
            {
                control.AddCompetitor(competitor.Item1, competitor.Item2);
            }
        }

        public static readonly DependencyProperty RoleProperty = DependencyProperty.Register(
            nameof(Role),
            typeof(Role),
            typeof(PrelimsAdder),
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
                    ((PrelimsAdder)source).Competition, 
                    ((PrelimsAdder)source).Competition));
        }

        public static readonly DependencyProperty RoundProperty = DependencyProperty.Register(
            nameof(Round),
            typeof(int),
            typeof(PrelimsAdder),
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
                    ((PrelimsAdder)source).Competition,
                    ((PrelimsAdder)source).Competition));
        }

        public static readonly DependencyProperty CompetitorsProperty = DependencyProperty.Register(
            nameof(Competitors),
            typeof(List<Competitor>),
            typeof(PrelimsAdder),
            new FrameworkPropertyMetadata(1, OnCompetitorsPropertyChanged));
        public List<Competitor> Competitors
        {
            get { return (List<Competitor>)GetValue(CompetitorsProperty); }
            set { SetValue(CompetitorsProperty, value); }
        }
        private static void OnCompetitorsPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
        }

        public static readonly DependencyProperty JudgesProperty = DependencyProperty.Register(
            nameof(Judges),
            typeof(List<IJudge>),
            typeof(PrelimsAdder),
            new FrameworkPropertyMetadata(1, OnJudgesPropertyChanged));
        public List<IJudge> Judges
        {
            get { return (List<IJudge>)GetValue(JudgesProperty); }
            set { SetValue(JudgesProperty, value); }
        }
        private static void OnJudgesPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion

        public string CountString
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

        public int Count
        {
            get
            {
                if (Role == Role.Leader)
                    return Competition.TotalLeaders;
                else if (Role == Role.Follower)
                    return Competition.TotalFollowers;
                else
                    return 0;
            }
        }

        private List<SearchTextBox> _competitorBoxes = new List<SearchTextBox>();
        private List<SearchTextBox> _judgeBoxes = new List<SearchTextBox>();

        private Border _addRowBorder;
        private Button _addColumnButton;

        private PrelimScore[,] _scores;

        public PrelimsAdder()
        {
            InitializeComponent();
        }

        public void UpdateCompetitors()
        {
            foreach (SearchTextBox searchTextBox in _competitorBoxes)
            {
                var selectedId = searchTextBox.SelectedPerson?.Id;
                searchTextBox.ItemsSource = Competitors;

                if (selectedId != null)
                    searchTextBox.SelectedPerson = searchTextBox.ItemsSource.Where(s => s.Id == selectedId).FirstOrDefault();
            }
        }
        public void UpdateJudges()
        {
            foreach (SearchTextBox searchTextBox in _judgeBoxes)
            {
                var selectedId = searchTextBox.SelectedPerson?.Id;
                searchTextBox.ItemsSource = Judges;

                if (selectedId != null)
                    searchTextBox.SelectedPerson = searchTextBox.ItemsSource.Where(s => s.Id == selectedId).FirstOrDefault(); ;
            }
        }

        private void AddCompetitor(Competitor competitor = null, List<PrelimScore> competitorPrelimScores = null)
        {
            ScoreGrid.RowDefinitions.Add(new RowDefinition());

            if (competitorPrelimScores == null && competitor != null)
            {
                competitorPrelimScores = new();
                
                if (Role == Role.Leader)
                    competitorPrelimScores = Competition.LeaderPrelimScores.Where(s => s.Competitor.FullName == competitor.FullName && s.Round == Round).OrderBy(c => c.Judge.FullName).ToList();
                else if (Role == Role.Follower)
                    competitorPrelimScores = Competition.FollowerPrelimScores.Where(s => s.Competitor.FullName == competitor.FullName && s.Round == Round).OrderBy(c => c.Judge.FullName).ToList();
            }
            else if (competitorPrelimScores != null)
            {
                competitorPrelimScores = competitorPrelimScores.OrderBy(c => c.Judge.FullName).ToList();
            }
            else
            {
                competitorPrelimScores = new();
            }

            bool finaled = false;
            if (competitorPrelimScores.Count > 0)
            {
                finaled = competitorPrelimScores.FirstOrDefault().Finaled;
            }

            var countBorder = new Border()
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(1),
                Height = 24
            };

            var countTextBlock = new TextBlock()
            {
                Text = (ScoreGrid.RowDefinitions.Count - 2).ToString(),
                Margin = new Thickness(1)
            };

            if (finaled)
                countTextBlock.FontWeight = FontWeights.Bold;

            countBorder.Child = countTextBlock;

            ScoreGrid.Children.Add(countBorder);
            Grid.SetColumn(countBorder, 0);
            Grid.SetRow(countBorder, ScoreGrid.RowDefinitions.Count - 2);

            // names
            var competitorBorder = new Border()
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(1),
                Height = 24
            };

            var competitorSearchBox = new SearchTextBox()
            {
                Margin = new Thickness(1),
                ItemsSource = Competitors,
                SelectedPerson = null,
                Placement = _competitorBoxes.Count() + 1
            };

            competitorSearchBox.SelectionChanged += (o, e) =>
            {
                if (e.AddedItems.Count > 0 && e.RemovedItems.Count <= 0)
                    UpdateCompetitorInScores(e.Placement, (Competitor)e.AddedItems[0]);
            };

            if (competitor != null)
            {
                competitorSearchBox.SelectedPerson = competitorSearchBox.ItemsSource.Where(c => c.Id == competitor.Id).FirstOrDefault();

                if (competitorSearchBox.SelectedPerson == null)
                {
                    competitorSearchBox.AddMode(competitor.FirstName, competitor.LastName, competitor.WsdcId);
                }
                else
                {
                    foreach (var prelimScore in competitorPrelimScores)
                    {
                        prelimScore.Competitor = (Competitor)competitorSearchBox.SelectedPerson;
                    }
                }
            }

            _competitorBoxes.Add(competitorSearchBox);
            competitorBorder.Child = competitorSearchBox;

            ScoreGrid.Children.Add(competitorBorder);
            Grid.SetColumn(competitorBorder, 1);
            Grid.SetRow(competitorBorder, ScoreGrid.RowDefinitions.Count - 2);

            // scores
            for (int i = 2; i < _judgeBoxes.Count() + 2; i++)
            {
                PrelimScore prelimScore;

                if (competitorPrelimScores != null && competitorPrelimScores?.Count > i - 2)
                {
                    prelimScore = competitorPrelimScores[i - 2];
                }
                else
                {
                    int judgeIndex = i - 2;
                    int competitorIndex = _competitorBoxes.Count() - 1;

                    prelimScore = _scores[competitorIndex, judgeIndex];
                }
                
                Border scoreBorder = MakeScoreBorder(prelimScore);

                ScoreGrid.Children.Add(scoreBorder);
                Grid.SetColumn(scoreBorder, i);
                Grid.SetRow(scoreBorder, ScoreGrid.RowDefinitions.Count() - 2);
            }

            Grid.SetRow(_addRowBorder, ScoreGrid.RowDefinitions.Count() - 1);

            OnPropertyChanged(nameof(Competition));
        }
        private void AddJudge(IJudge judge = null)
        {
            ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            Border judgeBorder = new Border()
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(1),
                Height = 24
            };

            SearchTextBox judgeSearchBox = new SearchTextBox()
            {
                ItemsSource = Judges,                
            };

            _judgeBoxes.Add(judgeSearchBox);

            judgeSearchBox.SelectionChanged += (o, e) =>
            {
                if (e.AddedItems.Count > 0)
                {
                    int judgeIndex = _judgeBoxes.IndexOf(judgeSearchBox);
                    UpdateJudgeInScores(judgeIndex, (IJudge)e.AddedItems[0]);
                }
            };

            if (judge != null)
            {
                judgeSearchBox.SelectedPerson = judgeSearchBox.ItemsSource.Where(j => j.FullName == judge.FullName).FirstOrDefault();

                if (judgeSearchBox.SelectedPerson != null)
                {
                    UpdateJudgeInScores(_judgeBoxes.IndexOf(judgeSearchBox), (IJudge)judgeSearchBox.SelectedPerson);
                }
            }

            judgeBorder.Child = judgeSearchBox;

            ScoreGrid.Children.Add(judgeBorder);
            Grid.SetColumn(judgeBorder, ScoreGrid.ColumnDefinitions.Count() - 2);

            for (int i = 1; i < ScoreGrid.RowDefinitions.Count - 1; i++)
            {
                Competitor competitor = _competitorBoxes.Where(c => c.Placement == i).FirstOrDefault().SelectedPerson as Competitor;

                int competitorIndex = i - 1;
                int judgeIndex = _judgeBoxes.Count() - 1;

                PrelimScore prelimScore = _scores[competitorIndex, judgeIndex];

                Border scoreBorder = MakeScoreBorder(prelimScore);

                ScoreGrid.Children.Add(scoreBorder);
                Grid.SetRow(scoreBorder, i);
                Grid.SetColumn(scoreBorder, ScoreGrid.ColumnDefinitions.Count() - 2);
            }

            Grid.SetColumn(_addColumnButton, ScoreGrid.ColumnDefinitions.Count() - 1);
            OnPropertyChanged(nameof(Competition));
        }

        private void UpdateCompetitorInScores(int placement, Competitor newCompetitor)
        {
            List<PrelimScore> prelimScores = new();

            if (Role == Role.Leader)
                prelimScores = Competition.LeaderPrelimScores.Where(s => s.RawScore == placement && s.Round == Round).ToList();
            else if (Role == Role.Follower)
                prelimScores = Competition.FollowerPrelimScores.Where(s => s.RawScore == placement && s.Round == Round).ToList();

            if (prelimScores == null) //this shouldn't really happen
                return;

            foreach (PrelimScore score in prelimScores)
            {
                score.Competitor = newCompetitor;
            }

            OnPropertyChanged(nameof(Competition));
        }
        private void UpdateJudgeInScores(int judgeIndex, IJudge newJudge)
        {
            for (int i = 0; i < _competitorBoxes.Count(); i++)
            {
                _scores[i, judgeIndex].Judge = newJudge;
            }

            OnPropertyChanged(nameof(Competition));
        }

        private void UpdateScore(int competitorIndex, int judgeIndex, CallbackScore newScore)
        {
             _scores[competitorIndex, judgeIndex].CallbackScore = newScore;
            OnPropertyChanged(nameof(Competition));
        }

        private void AddBlankCompetitorToScores()
        {
            PrelimScore[,] scores = new PrelimScore[_scores.GetLength(0) + 1, _scores.GetLength(1)];

            // transfer prelim scores from current array
            for (int competitorIndex = 0; competitorIndex < _scores.GetLength(0); competitorIndex++)
            {
                for (int judgeIndex = 0; judgeIndex < _scores.GetLength(1); judgeIndex++)
                {
                    scores[competitorIndex, judgeIndex] = _scores[competitorIndex, judgeIndex];
                }
            }

            List<PrelimScore> newScores = new();

            // add new set of prelim scores for the new competitor
            for (int judgeIndex = 0; judgeIndex < _scores.GetLength(1); judgeIndex++)
            {
                var newPrelimScore = new PrelimScore(
                    Competition,
                    _judgeBoxes.ElementAt(judgeIndex).SelectedPerson as IJudge,
                    null,
                    Role,
                    false,
                    CallbackScore.NoScore,
                    scores.GetLength(0),
                    Round);

                scores[scores.GetLength(0) - 1, judgeIndex] = newPrelimScore;
                newScores.Add(newPrelimScore);
            }

            if (Role == Role.Leader)
            {
                Competition.LeaderPrelimScores.AddRange(newScores);
            }
            else if (Role == Role.Follower)
            {
                Competition.FollowerPrelimScores.AddRange(newScores);
            }

            _scores = scores;
            OnPropertyChanged(nameof(Competition));
        }
        private void AddBlankJudgeToScores()
        {
            PrelimScore[,] scores = new PrelimScore[_scores.GetLength(0), _scores.GetLength(1) + 1];

            // transfer prelim scores from current array
            for (int competitorIndex = 0; competitorIndex < _scores.GetLength(0); competitorIndex++)
            {
                for (int judgeIndex = 0; judgeIndex < _scores.GetLength(1); judgeIndex++)
                {
                    scores[competitorIndex, judgeIndex] = _scores[competitorIndex, judgeIndex];
                }
            }

            List<PrelimScore> newScores = new();

            // add new set of prelim scores for the new judge
            for (int competitorIndex = 0; competitorIndex < _scores.GetLength(0); competitorIndex++)
            {
                var newPrelimScore = new PrelimScore(
                    Competition,
                    null,
                    _competitorBoxes.ElementAt(competitorIndex).SelectedPerson as Competitor,
                    Role,
                    false,
                    CallbackScore.NoScore,
                    scores.GetLength(0),
                    Round);

                scores[competitorIndex, scores.GetLength(1) - 1] = newPrelimScore;
                newScores.Add(newPrelimScore);
            }

            if (Role == Role.Leader)
            {
                Competition.LeaderPrelimScores.AddRange(newScores);
            }
            else if (Role == Role.Follower)
            {
                Competition.FollowerPrelimScores.AddRange(newScores);
            }

            _scores = scores;
            OnPropertyChanged(nameof(Competition));
        }

        private Border MakeScoreBorder(PrelimScore score)
        {
            Border scoreBorder = new Border()
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(1),
                Height = 24
            };

            ComboBox scoreComboBox = new ComboBox()
            {
                Margin = new Thickness(1),
                ItemsSource = Enum.GetValues(typeof(CallbackScore)),
                SelectedValue = score.CallbackScore
            };

            scoreComboBox.SelectionChanged += (s, e) =>
            {
                int competitorIndex = Grid.GetRow(((ComboBox)s).Parent as Border) - 1;
                int judgeIndex = Grid.GetColumn(((ComboBox)s).Parent as Border) - 2;

                var val = (CallbackScore)((ComboBox)s).SelectedValue;

                UpdateScore(competitorIndex, judgeIndex, val);
            };

            if (score.Finaled)
                scoreComboBox.FontWeight = FontWeights.Bold;

            scoreBorder.Child = scoreComboBox;

            return scoreBorder;
        }

        private void Clear()
        {
            _scores = new PrelimScore[0, 0];

            _judgeBoxes.Clear();
            _competitorBoxes.Clear();

            ScoreGrid.Children.Clear();
            ScoreGrid.RowDefinitions.Clear();
            ScoreGrid.ColumnDefinitions.Clear();

            ScoreGrid.RowDefinitions.Add(new RowDefinition());
            ScoreGrid.RowDefinitions.Add(new RowDefinition());

            ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition());
            ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            var placeBorder = new Border()
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
            placeBorder.Child = placeTextBlock;

            ScoreGrid.Children.Add(placeBorder);
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

            ScoreGrid.Children.Add(competitorBorder);
            Grid.SetRow(competitorBorder, 0);
            Grid.SetColumn(competitorBorder, 1);

            var addColumnButton = new Button()
            {
                Content = "+",
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 20,
                Margin = new Thickness(1)
            };
            addColumnButton.Click += AddColumn_Click;

            ScoreGrid.Children.Add(addColumnButton);
            Grid.SetRow(addColumnButton, 0);
            Grid.SetColumn(addColumnButton, 2);

            var addRowBorder = new Border()
            {
                BorderThickness = new Thickness(0),
                BorderBrush = Brushes.Gray,
                Margin = new Thickness(1)
            };

            var addRowButton = new Button()
            {
                Content = "+"
            };
            addRowButton.Click += AddRow_Click;
            addRowBorder.Child = addRowButton;

            ScoreGrid.Children.Add(addRowBorder);
            Grid.SetRow(addRowBorder, 1);
            Grid.SetColumn(addRowBorder, 0);

            _addColumnButton = addColumnButton;
            _addRowBorder = addRowBorder;
        }

        private void AddRow_Click(object sender, RoutedEventArgs e)
        {
            AddBlankCompetitorToScores();
            AddCompetitor();
        }
        private void AddColumn_Click(object sender, RoutedEventArgs e)
        {
            AddBlankJudgeToScores();
            AddJudge();
        }

        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
