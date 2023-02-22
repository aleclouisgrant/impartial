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
    public partial class PrelimsAdder : UserControl
    {
        #region DependencyProperties

        public static readonly DependencyProperty CompetitionProperty = DependencyProperty.Register(
            nameof(Competition),
            typeof(Competition),
            typeof(PrelimsAdder),
            new FrameworkPropertyMetadata(new Competition(), OnCompetitionPropertyChanged));
        public Competition Competition
        {
            get { return (Competition)GetValue(CompetitionProperty); }
            set { SetValue(CompetitionProperty, value); }
        }
        private static void OnCompetitionPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var control = (PrelimsAdder)source;
            var competition = (Competition)e.NewValue;

            if (competition == null)
                return;

            var judges = new List<Judge>();
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

            control.Clear();

            foreach (var judge in judges)
            {
                if (control.Role == Role.Leader)
                    control._judgeScores.Add(new Tuple<Judge, List<PrelimScore>>(judge, competition.LeaderPrelimScores.Where(s => s.Judge.Id == judge.Id && s.Round == control.Round).ToList()));
                else if (control.Role == Role.Follower)
                    control._judgeScores.Add(new Tuple<Judge, List<PrelimScore>>(judge, competition.FollowerPrelimScores.Where(s => s.Judge.Id == judge.Id && s.Round == control.Round).ToList()));

                control.AddJudge(judge);
            }

            foreach (var competitor in c)
            {
                control.AddCompetitor(competitor.Item1, competitor.Item2);
            }

            foreach (var score in competition.LeaderPrelimScores)
            {
                score.Competition = competition;
            }
            foreach (var score in competition.FollowerPrelimScores)
            {
                score.Competition = competition;
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

        private LinkedList<Tuple<Competitor, List<PrelimScore>>> _prelimScores;
        public LinkedList<Tuple<Competitor, List<PrelimScore>>> PrelimScores
        {
            get { return _prelimScores; }
            set
            {
                _prelimScores = value;
                OnPropertyChanged();
            }
        }

        private List<Competitor> _competitors;
        public List<Competitor> Competitors
        {
            get { return _competitors; }
            set
            {
                _competitors = value;
                OnPropertyChanged();
            }
        }

        private List<Judge> _judges;
        public List<Judge> Judges
        {
            get { return _judges; }
            set
            {
                _judges = value;
                OnPropertyChanged();
            }
        }

        private List<SearchTextBox> _competitorBoxes = new List<SearchTextBox>();
        private List<SearchTextBox> _judgeBoxes = new List<SearchTextBox>();

        private List<Tuple<Judge, List<PrelimScore>>> _judgeScores = new();

        // not used right now
        private Border _addRowBorder;
        private Button _addColumnButton;

        public PrelimsAdder()
        {
            InitializeComponent();

            PrelimScores = new();

            UpdateCompetitors();
            UpdateJudges();
        }

        public async void UpdateCompetitors()
        {
            Competitors = (await App.DatabaseProvider.GetAllCompetitorsAsync()).ToList();
            foreach (SearchTextBox searchTextBox in _competitorBoxes)
            {
                var selectedId = searchTextBox.SelectedPerson?.Id;
                searchTextBox.ItemsSource = Competitors;

                if (selectedId != null)
                    searchTextBox.SelectedPerson = searchTextBox.ItemsSource.Where(s => s.Id == selectedId).FirstOrDefault();
            }
        }
        public async void UpdateJudges()
        {
            Judges = (await App.DatabaseProvider.GetAllJudgesAsync()).OrderBy(j => j.FirstName).ToList();

            // filter out anonymous judges
            //Judges = Judges.RemoveAll(j => Int32.TryParse(j.LastName, out int res) == true);

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

            if (competitorPrelimScores == null)
            {
                competitorPrelimScores = new();
                
                if (Role == Role.Leader)
                    competitorPrelimScores = Competition.LeaderPrelimScores.Where(s => s.Competitor.FullName == competitor.FullName && s.Round == Round).OrderBy(c => c.Judge.FullName).ToList();
                else if (Role == Role.Follower)
                    competitorPrelimScores = Competition.FollowerPrelimScores.Where(s => s.Competitor.FullName == competitor.FullName && s.Round == Round).OrderBy(c => c.Judge.FullName).ToList();
            }
            else
            {
                competitorPrelimScores = competitorPrelimScores.OrderBy(c => c.Judge.FullName).ToList();
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
                Text = (ScoreGrid.RowDefinitions.Count - 1).ToString(),
                Margin = new Thickness(1)
            };

            if (finaled)
                countTextBlock.FontWeight = FontWeights.Bold;

            countBorder.Child = countTextBlock;

            ScoreGrid.Children.Add(countBorder);
            Grid.SetColumn(countBorder, 0);
            Grid.SetRow(countBorder, ScoreGrid.RowDefinitions.Count - 1);

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
                SelectedPerson = null
            };

            competitorSearchBox.SelectionChanged += (o, e) =>
            {
                if (e.AddedItems.Count > 0 && e.RemovedItems.Count > 0)
                    UpdateCompetitorScores((Competitor)e.RemovedItems[0], (Competitor)e.AddedItems[0]);
            };

            bool competitorFound = false;
            if (competitor != null)
            {
                competitorSearchBox.SelectedPerson = competitorSearchBox.ItemsSource.Where(c => c.Id == competitor.Id).FirstOrDefault();

                if (competitorSearchBox.SelectedPerson == null)
                {
                    competitorSearchBox.AddMode(competitor.FirstName, competitor.LastName, competitor.WsdcId);
                }
                else
                {
                    competitorFound = true;
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
            Grid.SetRow(competitorBorder, ScoreGrid.RowDefinitions.Count - 1);

            // scores
            int i = 2;
            foreach (var score in competitorPrelimScores)
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

                ScoreGrid.Children.Add(border);
                Grid.SetColumn(border, i);
                Grid.SetRow(border, ScoreGrid.RowDefinitions.Count() - 1);
                i++;
            }

            //Grid.SetRow(_addRowBorder, ScoreGrid.RowDefinitions.Count() - 1);

            if (competitorFound)
            {
                PrelimScores.AddLast(new Tuple<Competitor, List<PrelimScore>>(
                    (Competitor)competitorSearchBox.SelectedPerson,
                    competitorPrelimScores));
            }
            else
            {
                PrelimScores.AddLast(new Tuple<Competitor, List<PrelimScore>>(
                    new Competitor(competitor.FirstName, competitor.LastName),
                    competitorPrelimScores));
            }

            OnPropertyChanged(nameof(Competition));
        }
        private void AddJudge(Judge judge = null)
        {
            ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            //ScoreGrid.ColumnDefinitions[ScoreGrid.ColumnDefinitions.Count - 2].Width = new GridLength(1.0, GridUnitType.Star);

            Border judgeBorder = new Border()
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(1),
                Height = 24
            };

            SearchTextBox judgeSearchBox = new SearchTextBox()
            {
                ItemsSource = Judges
            };
            _judgeBoxes.Add(judgeSearchBox);

            judgeSearchBox.SelectionChanged += (o, e) =>
            {
                if (e.AddedItems.Count > 0)
                {
                    int index = _judgeBoxes.IndexOf(judgeSearchBox);
                    UpdateJudgeScores(index, (Judge)e.AddedItems[0]);
                }
            };

            if (judge != null)
            {
                judgeSearchBox.SelectedPerson = judgeSearchBox.ItemsSource.Where(j => j.FullName == judge.FullName).FirstOrDefault();

                if (judgeSearchBox.SelectedPerson != null)
                {
                    UpdateJudgeScores(_judgeBoxes.IndexOf(judgeSearchBox), (Judge)judgeSearchBox.SelectedPerson);
                }
            }

            judgeBorder.Child = judgeSearchBox;

            ScoreGrid.Children.Add(judgeBorder);
            Grid.SetColumn(judgeBorder, ScoreGrid.ColumnDefinitions.Count() - 1);

            for (int i = 1; i < ScoreGrid.RowDefinitions.Count - 1; i++)
            {
                Border scoreBorder = new Border()
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(1),
                    Height = 24
                };

                TextBox scoreTextBox = new TextBox()
                {
                    Margin = new Thickness(1)
                };

                scoreBorder.Child = scoreTextBox;

                ScoreGrid.Children.Add(scoreBorder);
                Grid.SetRow(scoreBorder, i);
                Grid.SetColumn(scoreBorder, ScoreGrid.ColumnDefinitions.Count() - 1);
            }

            //Grid.SetColumn(_addColumnButton, ScoreGrid.ColumnDefinitions.Count() - 1);
            OnPropertyChanged(nameof(Competition));
        }

        private void UpdateCompetitorScores(Competitor oldCompetitor, Competitor newCompetitor)
        {
            List<PrelimScore> c = 
                PrelimScores.Where(s => s.Item1?.FullName == oldCompetitor.FullName).
                FirstOrDefault()?.Item2.ToList();

            if (c == null) //this shouldn't really happen
                return;

            foreach (PrelimScore score in c)
            {
                score.Competitor = newCompetitor;
            }

            OnPropertyChanged(nameof(Competition));
        }
        private void UpdateJudgeScores(int index, Judge judge)
        {
            foreach (var prelimScore in _judgeScores[index].Item2)
            {
                prelimScore.Judge = judge;
            }

            OnPropertyChanged(nameof(Competition));
        }

        private void Clear()
        {
            _judgeScores.Clear();
            _judgeBoxes.Clear();
            _prelimScores.Clear();
            _competitorBoxes.Clear();

            ScoreGrid.Children.Clear();
            ScoreGrid.RowDefinitions.Clear();
            ScoreGrid.ColumnDefinitions.Clear();

            ScoreGrid.RowDefinitions.Add(new RowDefinition());

            ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition());

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

            //var addColumnButton = new Button()
            //{
            //    Content = "+",
            //    HorizontalAlignment = HorizontalAlignment.Left,
            //    Width = 20,
            //    Margin = new Thickness(1)
            //};
            //addColumnButton.Click += AddColumn_Click;

            //ScoreGrid.Children.Add(addColumnButton);
            //Grid.SetRow(addColumnButton, 0);
            //Grid.SetColumn(addColumnButton, 3);

            //var addRowBorder = new Border()
            //{
            //    BorderThickness = new Thickness(0),
            //    BorderBrush = Brushes.Gray,
            //    Margin = new Thickness(1)
            //};

            //var addRowButton = new Button()
            //{
            //    Content = "+"
            //};
            //addRowButton.Click += AddRow_Click;
            //addRowBorder.Child = addRowButton;

            //ScoreGrid.Children.Add(addRowBorder);
            //Grid.SetRow(addRowBorder, 1);
            //Grid.SetColumn(addRowBorder, 0);

            //_addColumnButton = addColumnButton;
            //_addRowBorder = addRowBorder;
        }

        private void AddRow_Click(object sender, RoutedEventArgs e)
        {
            AddCompetitor();
        }
        private void AddColumn_Click(object sender, RoutedEventArgs e)
        {
            AddJudge();
        }

        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };
        public void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
