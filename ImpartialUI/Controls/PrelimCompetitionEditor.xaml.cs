using Impartial;
using Impartial.Enums;
using ImpartialUI.Models;
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
    public partial class PrelimCompetitionEditor : UserControl
    {
        #region DependencyProperties
        public static readonly DependencyProperty PrelimCompetitionProperty = DependencyProperty.Register(
            nameof(PrelimCompetition),
            typeof(IPrelimCompetition),
            typeof(PrelimCompetitionEditor),
            new FrameworkPropertyMetadata(new PrelimCompetition(), OnCompetitionPropertyChanged));
        public IPrelimCompetition PrelimCompetition
        {
            get { return (IPrelimCompetition)GetValue(PrelimCompetitionProperty); }
            set { SetValue(PrelimCompetitionProperty, value); }
        }
        private static void OnCompetitionPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var control = (PrelimCompetitionEditor)source;
            var competition = (IPrelimCompetition)e.NewValue;

            if (competition == null)
                return;

            var judges = new List<IJudge>();
            List<Tuple<ICompetitor, List<IPrelimScore>>> c = new();

            foreach (var competitor in competition.Competitors)
            {
                c.Add(new(competitor, competition.PrelimScores.Where(s => s.Competitor.FullName == competitor.FullName).OrderBy(c => c.Judge.FullName).ToList()));
            }
            
            judges = competition.Judges.OrderBy(j => j.FullName).ToList();
            
            c = c.OrderBy(cc => cc.Item2.FirstOrDefault()).ToList();

            control.Clear();

            control._scores = new IPrelimScore[c.Count, judges.Count];

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
            typeof(PrelimCompetitionEditor),
            new FrameworkPropertyMetadata(Role.Leader, OnRolePropertyChanged));
        public Role Role
        {
            get { return (Role)GetValue(RoleProperty); }
            set { SetValue(RoleProperty, value); }
        }
        private static void OnRolePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            ((PrelimCompetitionEditor)source).PrelimCompetition.Role = (Role)e.NewValue;
        }

        public static readonly DependencyProperty RoundProperty = DependencyProperty.Register(
            nameof(Round),
            typeof(Round),
            typeof(PrelimCompetitionEditor),
            new FrameworkPropertyMetadata(Round.Prelims, OnRoundPropertyChanged));
        public Round Round
        {
            get { return (Round)GetValue(RoundProperty); }
            set { SetValue(RoundProperty, value); }
        }
        private static void OnRoundPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            ((PrelimCompetitionEditor)source).PrelimCompetition.Round = (Round)e.NewValue;
        }

        public static readonly DependencyProperty CompetitorsProperty = DependencyProperty.Register(
            nameof(Competitors),
            typeof(List<ICompetitor>),
            typeof(PrelimCompetitionEditor),
            new FrameworkPropertyMetadata(new List<ICompetitor>(), OnCompetitorsPropertyChanged));
        public List<ICompetitor> Competitors
        {
            get { return (List<ICompetitor>)GetValue(CompetitorsProperty); }
            set { SetValue(CompetitorsProperty, value); }
        }
        private static void OnCompetitorsPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
        }

        public static readonly DependencyProperty JudgesProperty = DependencyProperty.Register(
            nameof(Judges),
            typeof(List<IJudge>),
            typeof(PrelimCompetitionEditor),
            new FrameworkPropertyMetadata(new List<IJudge>(), OnJudgesPropertyChanged));
        public List<IJudge> Judges
        {
            get { return (List<IJudge>)GetValue(JudgesProperty); }
            set { SetValue(JudgesProperty, value); }
        }
        private static void OnJudgesPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion

        public string CountString => PrelimCompetition.Competitors.Count().ToString();

        private List<SearchTextBox> _competitorBoxes = new List<SearchTextBox>();
        private List<SearchTextBox> _judgeBoxes = new List<SearchTextBox>();

        private Border _addRowBorder;
        private Button _addColumnButton;

        private IPrelimScore[,] _scores;

        public PrelimCompetitionEditor()
        {
            InitializeComponent();
        }

        public void UpdateCompetitors()
        {
            foreach (SearchTextBox searchTextBox in _competitorBoxes)
            {
                var selectedId = searchTextBox.SelectedPerson?.Id;
                searchTextBox.ItemsSource = (IEnumerable<PersonModel>)Competitors;

                if (selectedId != null)
                    searchTextBox.SelectedPerson = searchTextBox.ItemsSource.Where(s => s.Id == selectedId).FirstOrDefault();
            }
        }
        public void UpdateJudges()
        {
            foreach (SearchTextBox searchTextBox in _judgeBoxes)
            {
                var selectedId = searchTextBox.SelectedPerson?.Id;
                searchTextBox.ItemsSource = (IEnumerable<PersonModel>)Judges;

                if (selectedId != null)
                    searchTextBox.SelectedPerson = searchTextBox.ItemsSource.Where(s => s.Id == selectedId).FirstOrDefault(); ;
            }
        }

        private void AddCompetitor(ICompetitor competitor = null, List<IPrelimScore> competitorPrelimScores = null)
        {
            ScoreGrid.RowDefinitions.Add(new RowDefinition());

            if (competitorPrelimScores == null && competitor != null)
            {
                competitorPrelimScores = PrelimCompetition.PrelimScores.Where(s => s.Competitor.FullName == competitor.FullName).OrderBy(c => c.Judge.FullName).ToList() ?? new();
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
                finaled = PrelimCompetition.PromotedCompetitors.Contains(competitor);
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
                ItemsSource = (IEnumerable<PersonModel>)Competitors,
                SelectedPerson = null,
                Placement = _competitorBoxes.Count() + 1
            };

            competitorSearchBox.SelectionChanged += (o, e) =>
            {
                if (e.AddedItems.Count > 0 && e.RemovedItems.Count <= 0)
                    UpdateCompetitorInScores(e.Placement, (ICompetitor)e.AddedItems[0]);
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
                        prelimScore.SetCompetitor(competitorSearchBox.SelectedPerson.Id);
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
                IPrelimScore prelimScore;

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

            OnPropertyChanged(nameof(PrelimCompetition));
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
                ItemsSource = (IEnumerable<PersonModel>)Judges,                
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
                ICompetitor competitor = _competitorBoxes.Where(c => c.Placement == i).FirstOrDefault().SelectedPerson as Competitor;

                int competitorIndex = i - 1;
                int judgeIndex = _judgeBoxes.Count() - 1;

                IPrelimScore prelimScore = _scores[competitorIndex, judgeIndex];

                Border scoreBorder = MakeScoreBorder(prelimScore);

                ScoreGrid.Children.Add(scoreBorder);
                Grid.SetRow(scoreBorder, i);
                Grid.SetColumn(scoreBorder, ScoreGrid.ColumnDefinitions.Count() - 2);
            }

            Grid.SetColumn(_addColumnButton, ScoreGrid.ColumnDefinitions.Count() - 1);
            OnPropertyChanged(nameof(PrelimCompetition));
        }

        private void UpdateCompetitorInScores(int placement, ICompetitor newCompetitor)
        {
            for (int j = 0; j < _judgeBoxes.Count(); j++)
            {
                _scores[placement, j].SetCompetitor(newCompetitor.Id);
            }

            OnPropertyChanged(nameof(PrelimCompetition));
        }
        private void UpdateJudgeInScores(int judgeIndex, IJudge newJudge)
        {
            for (int i = 0; i < _competitorBoxes.Count(); i++)
            {
                _scores[i, judgeIndex].SetJudge(newJudge.Id);
            }

            OnPropertyChanged(nameof(PrelimCompetition));
        }

        private void UpdateScore(int competitorIndex, int judgeIndex, CallbackScore newScore)
        {
             _scores[competitorIndex, judgeIndex].CallbackScore = newScore;
            OnPropertyChanged(nameof(PrelimCompetition));
        }

        private void AddBlankCompetitorToScores()
        {
            IPrelimScore[,] scores = new PrelimScore[_scores.GetLength(0) + 1, _scores.GetLength(1)];

            // transfer prelim scores from current array
            for (int competitorIndex = 0; competitorIndex < _scores.GetLength(0); competitorIndex++)
            {
                for (int judgeIndex = 0; judgeIndex < _scores.GetLength(1); judgeIndex++)
                {
                    scores[competitorIndex, judgeIndex] = _scores[competitorIndex, judgeIndex];
                }
            }

            List<IPrelimScore> newScores = new();

            // add new set of prelim scores for the new competitor
            for (int judgeIndex = 0; judgeIndex < _scores.GetLength(1); judgeIndex++)
            {
                var newPrelimScore = new PrelimScore(
                    judgeId: _judgeBoxes.ElementAt(judgeIndex).SelectedPerson.Id,
                    competitorId: Guid.Empty,
                    callbackScore: CallbackScore.NoScore);

                scores[scores.GetLength(0) - 1, judgeIndex] = newPrelimScore;
                newScores.Add(newPrelimScore);
            }

            PrelimCompetition.PrelimScores.AddRange(newScores);

            _scores = scores;
            OnPropertyChanged(nameof(PrelimCompetition));
        }
        private void AddBlankJudgeToScores()
        {
            IPrelimScore[,] scores = new PrelimScore[_scores.GetLength(0), _scores.GetLength(1) + 1];

            // transfer prelim scores from current array
            for (int competitorIndex = 0; competitorIndex < _scores.GetLength(0); competitorIndex++)
            {
                for (int judgeIndex = 0; judgeIndex < _scores.GetLength(1); judgeIndex++)
                {
                    scores[competitorIndex, judgeIndex] = _scores[competitorIndex, judgeIndex];
                }
            }

            List<IPrelimScore> newScores = new();

            // add new set of prelim scores for the new judge
            for (int competitorIndex = 0; competitorIndex < _scores.GetLength(0); competitorIndex++)
            {
                var newPrelimScore = new PrelimScore(
                    judgeId: Guid.Empty,
                    competitorId: _competitorBoxes.ElementAt(competitorIndex).SelectedPerson.Id,
                    callbackScore: CallbackScore.NoScore);

                scores[competitorIndex, scores.GetLength(1) - 1] = newPrelimScore;
                newScores.Add(newPrelimScore);
            }

            PrelimCompetition.PrelimScores.AddRange(newScores);

            _scores = scores;
            OnPropertyChanged(nameof(PrelimCompetition));
        }

        private Border MakeScoreBorder(IPrelimScore score)
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

            if (PrelimCompetition.PromotedCompetitors.Contains(score.Competitor))
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
