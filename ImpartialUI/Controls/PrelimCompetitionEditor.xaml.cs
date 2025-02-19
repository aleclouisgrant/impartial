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
        private static readonly int PROMOTED_COLUMN = 0;
        private static readonly int COUNT_COLUMN = 1;
        private static readonly int BIB_COLUMN = 2;
        private static readonly int COMPETITOR_COLUMN = 3;
        private static readonly int SCORE_COLUMN_START = 4;

        private class PrelimCompetitorScores
        {
            public string BibNumberString { get; set; }
            public int BibNumberValue
            {
                get
                {
                    if (Int32.TryParse(BibNumberString, out int n))
                        return n;

                    return -1;
                }
            }
            public ICompetitor Competitor { get; set; }
            public List<IPrelimScore> PrelimScores { get; set; } = new List<IPrelimScore>();
            public double TotalScore { get; set; }
        }

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
            List<PrelimCompetitorScores> competitorScoresList = new();

            foreach (var competitor in competition.Competitors)
            {
                var prelimScores = competition.PrelimScores.Where(s => s.Competitor.CompetitorId == competitor.CompetitorId).OrderBy(c => c.Judge.FullName).ToList();

                competitorScoresList.Add(new PrelimCompetitorScores 
                    {
                        Competitor = competitor,
                        PrelimScores = prelimScores,
                        TotalScore = prelimScores.Sum(c => Util.GetCallbackScoreValue(c.CallbackScore))
                    });
            }
            
            judges = competition.Judges.OrderBy(j => j.FullName).ToList();
            
            competitorScoresList = competitorScoresList.OrderByDescending(c => c.TotalScore).ToList();

            control.Clear();

            control._scores = new IPrelimScore[competitorScoresList.Count, judges.Count];

            for (int competitorIndex = 0; competitorIndex < competitorScoresList.Count; competitorIndex++)
            {
                for (int judgeIndex = 0; judgeIndex < judges.Count; judgeIndex++)
                {
                    control._scores[competitorIndex, judgeIndex] = competitorScoresList[competitorIndex].PrelimScores.ElementAt(judgeIndex);
                }
            }

            foreach (var judge in judges)
            {
                control.AddJudge(judge);
            }

            foreach (var competitor in competitorScoresList)
            {
                control.AddCompetitor(competitor.Competitor, competitor.PrelimScores);
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
            ((PrelimCompetitionEditor)source).UpdateCompetitors();
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
            ((PrelimCompetitionEditor)source).UpdateJudges();
        }
        #endregion

        public string CountString => PrelimCompetition.Competitors.Count().ToString();

        private List<SearchTextBox> _competitorBoxes = new List<SearchTextBox>();
        private List<SearchTextBox> _judgeBoxes = new List<SearchTextBox>();

        private Border _addRowBorder;
        private Button _addColumnButton;

        private IPrelimScore[,] _scores = new IPrelimScore[0,0];
        private string[] _bibNumbers = new string[0];

        public PrelimCompetitionEditor()
        {
            InitializeComponent();

            UpdateCompetitors();
            UpdateJudges();
        }

        public void UpdateCompetitors()
        {
            foreach (SearchTextBox searchTextBox in _competitorBoxes)
            {
                var selectedId = searchTextBox.SelectedPerson?.UserId;
                searchTextBox.ItemsSource = Competitors;

                if (selectedId != null)
                {
                    searchTextBox.SelectedPerson = searchTextBox.ItemsSource.FirstOrDefault(s => s.UserId == selectedId);
                }
                else
                {
                    searchTextBox.SelectedPerson = null;
                }
            }
        }
        public void UpdateJudges()
        {
            foreach (SearchTextBox searchTextBox in _judgeBoxes)
            {
                var selectedId = searchTextBox.SelectedPerson?.UserId;
                searchTextBox.ItemsSource = Judges;

                if (selectedId != null)
                {
                    searchTextBox.SelectedPerson = searchTextBox.ItemsSource.FirstOrDefault(s => s.UserId == selectedId);
                }
                else
                {
                    searchTextBox.SelectedPerson = null;
                }
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

            var row = ScoreGrid.RowDefinitions.Count - 2;

            bool promoted = competitorPrelimScores.Count > 0 ? PrelimCompetition.PromotedCompetitors.Contains(competitor) : false;
            var promotedButton = new PromotedButton()
            {
                Margin = new Thickness(20, 0, 0, 0),
                CallbackScore = promoted ? CallbackScore.Yes : CallbackScore.No
            };
            ScoreGrid.Children.Add(promotedButton);
            Grid.SetRow(promotedButton, row);
            Grid.SetColumn(promotedButton, PROMOTED_COLUMN);

            var competitorCountTextBlock = new TextBlock()
            {
                Text = (ScoreGrid.RowDefinitions.Count - 2).ToString(),
                Style = Application.Current.Resources["ScoreViewerCountTextStyleNonFinalist"] as Style
            };
            ScoreGrid.Children.Add(competitorCountTextBlock);
            Grid.SetRow(competitorCountTextBlock, row);
            Grid.SetColumn(competitorCountTextBlock, COUNT_COLUMN);

            var competitorBibTextBox = new TextBox()
            {
                Text = "",
                Style = Application.Current.Resources["ScoreViewerBibTextBoxStyleNonFinalist"] as Style
            };
            ScoreGrid.Children.Add(competitorBibTextBox);
            Grid.SetRow(competitorBibTextBox, row);
            Grid.SetColumn(competitorBibTextBox, BIB_COLUMN);

            // TODO: pass bib number into competitor registrations
            competitorBibTextBox.TextChanged += (o, e) =>
            {
                var bibNumber = competitorBibTextBox.Text;
            };

            var competitorSearchBox = new SearchTextBox()
            {
                ItemsSource = Competitors,
                SelectedPerson = null,
                Placement = _competitorBoxes.Count() + 1,
                Style = Application.Current.Resources["ScoreViewerSearchTextBox"] as Style
            };
            ScoreGrid.Children.Add(competitorSearchBox);
            Grid.SetRow(competitorSearchBox, row);
            Grid.SetColumn(competitorSearchBox, COMPETITOR_COLUMN);

            _competitorBoxes.Add(competitorSearchBox);

            competitorSearchBox.SelectionChanged += (o, e) =>
            {
                if (e.AddedItems.Count > 0 && e.RemovedItems.Count <= 1)
                {
                    UpdateCompetitorInScores(e.Placement, (ICompetitor)e.AddedItems[0]);
                }
            };

            if (competitor != null)
            {
                var oldCompetitor = competitorSearchBox.SelectedPerson;
                var newCompetitor = Util.FindCompetitorInCache(competitor.FirstName, competitor.LastName, (IEnumerable<ICompetitor>)competitorSearchBox.ItemsSource);
                competitorSearchBox.SelectedPerson = newCompetitor;

                if (newCompetitor != null)
                {
                    if (oldCompetitor == null || newCompetitor.UserId != oldCompetitor?.UserId)
                    {
                        foreach (var prelimScore in competitorPrelimScores)
                        {
                            if (!prelimScore.TrySetCompetitor(newCompetitor.CompetitorId))
                            {
                                // competitor id wasn't found in competitor database
                            }
                        }
                    }
                }
                else
                {
                    competitorSearchBox.AddMode(competitor.FirstName, competitor.LastName, competitor.WsdcId);
                }
            }

            // scores
            for (int scoreColumn = SCORE_COLUMN_START; scoreColumn < _judgeBoxes.Count() + 2; scoreColumn++)
            {
                IPrelimScore prelimScore;

                if (competitorPrelimScores != null && competitorPrelimScores?.Count > scoreColumn - 2)
                {
                    prelimScore = competitorPrelimScores[scoreColumn - SCORE_COLUMN_START];
                }
                else
                {
                    int judgeIndex = scoreColumn - SCORE_COLUMN_START;
                    int competitorIndex = _competitorBoxes.Count() - 1;

                    prelimScore = _scores[competitorIndex, judgeIndex];
                }

                var callbackScoreViewer = new CallbackScoreViewer()
                {
                    CallbackScore = prelimScore.CallbackScore,
                };
                ScoreGrid.Children.Add(callbackScoreViewer);
                Grid.SetRow(callbackScoreViewer, row);
                Grid.SetColumn(callbackScoreViewer, scoreColumn);
            }

            Grid.SetRow(_addRowBorder, ScoreGrid.RowDefinitions.Count() - 1);

            OnPropertyChanged(nameof(PrelimCompetition));
        }
        private void AddJudge(IJudge judge = null)
        {
            ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

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
                judgeSearchBox.SelectedPerson = Util.FindJudgeInCache(judge.FirstName, judge.LastName, (IEnumerable<IJudge>)judgeSearchBox.ItemsSource);

                if (judgeSearchBox.SelectedPerson != null)
                {
                    UpdateJudgeInScores(_judgeBoxes.IndexOf(judgeSearchBox), (IJudge)judgeSearchBox.SelectedPerson);
                }
            }

            ScoreGrid.Children.Add(judgeSearchBox);
            Grid.SetColumn(judgeSearchBox, ScoreGrid.ColumnDefinitions.Count() - 2);

            for (int i = 1; i < ScoreGrid.RowDefinitions.Count - 1; i++)
            {
                int competitorIndex = i - 1;
                int judgeIndex = _judgeBoxes.Count() - 1;

                IPrelimScore prelimScore = _scores[competitorIndex, judgeIndex];

                ComboBox scoreComboBox = new ComboBox()
                {
                    Margin = new Thickness(1),
                    ItemsSource = Enum.GetValues(typeof(CallbackScore)),
                    SelectedValue = prelimScore.CallbackScore
                };

                scoreComboBox.SelectionChanged += (s, e) =>
                {
                    int competitorIndex = Grid.GetRow((ComboBox)s) - 1;
                    int judgeIndex = Grid.GetColumn((ComboBox)s) - 2;

                    var val = (CallbackScore)((ComboBox)s).SelectedValue;

                    UpdateScore(competitorIndex, judgeIndex, val);
                };

                ScoreGrid.Children.Add(scoreComboBox);
                Grid.SetRow(scoreComboBox, i);
                Grid.SetColumn(scoreComboBox, ScoreGrid.ColumnDefinitions.Count() - 2);
            }

            Grid.SetColumn(_addColumnButton, ScoreGrid.ColumnDefinitions.Count() - 1);
            OnPropertyChanged(nameof(PrelimCompetition));
        }

        private void UpdateCompetitorInScores(int placement, ICompetitor newCompetitor)
        {
            for (int j = 0; j < _judgeBoxes.Count(); j++)
            {
                _scores[placement - 1, j].Competitor = newCompetitor;
            }

            OnPropertyChanged(nameof(PrelimCompetition));
        }
        private void UpdateJudgeInScores(int judgeIndex, IJudge newJudge)
        {
            for (int i = 0; i < _competitorBoxes.Count(); i++)
            {
                _scores[i, judgeIndex].Judge = newJudge;
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
            string[] bibNumbers = new string[_scores.GetLength(0) + 1];

            var registrations = new List<ICompetitorRegistration>();

            // transfer prelim scores from current array
            for (int competitorIndex = 0; competitorIndex < _scores.GetLength(0); competitorIndex++)
            {
                for (int judgeIndex = 0; judgeIndex < _scores.GetLength(1); judgeIndex++)
                {
                    scores[competitorIndex, judgeIndex] = _scores[competitorIndex, judgeIndex];
                }

                // add corresponding bib number
                bibNumbers[competitorIndex] = _bibNumbers[competitorIndex];

                registrations.Add(new CompetitorRegistration(scores[competitorIndex, 0].Competitor, bibNumbers[competitorIndex]));
            }

            // add additional bib number to end of array
            bibNumbers[bibNumbers.Length - 1] = "";

            List<IPrelimScore> newScores = new();

            // add new set of prelim scores for the new competitor
            for (int judgeIndex = 0; judgeIndex < _scores.GetLength(1); judgeIndex++)
            {
                var newPrelimScore = new PrelimScore(
                    judgeId: ((IJudge)_judgeBoxes.ElementAt(judgeIndex).SelectedPerson).JudgeId,
                    competitorId: Guid.Empty,
                    callbackScore: CallbackScore.Unscored);

                scores[scores.GetLength(0) - 1, judgeIndex] = newPrelimScore;
                newScores.Add(newPrelimScore);
            }

            PrelimCompetition.PrelimScores.AddRange(newScores);
            // TODO
            //PrelimCompetition.CompetitorRegistrations = registrations;

            _scores = scores;
            _bibNumbers = bibNumbers;

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
                    competitorId: ((ICompetitor)_competitorBoxes.ElementAt(competitorIndex).SelectedPerson)?.CompetitorId ?? Guid.Empty,
                    callbackScore: CallbackScore.Unscored);

                scores[competitorIndex, scores.GetLength(1) - 1] = newPrelimScore;
                newScores.Add(newPrelimScore);
            }

            PrelimCompetition.PrelimScores.AddRange(newScores);

            _scores = scores;
            OnPropertyChanged(nameof(PrelimCompetition));
        }

        private void Clear()
        {
            _scores = new IPrelimScore[0, 0];
            _bibNumbers = new string[0];

            _judgeBoxes.Clear();
            _competitorBoxes.Clear();

            ScoreGrid.Children.Clear();
            ScoreGrid.RowDefinitions.Clear();
            ScoreGrid.ColumnDefinitions.Clear();

            ScoreGrid.RowDefinitions.Add(new RowDefinition());
            ScoreGrid.RowDefinitions.Add(new RowDefinition());

            /// Promoted, Count, Bib, Competitor, [Judges], Add Judge Button
            ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); // Promoted
            ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); // Count
            ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); // Bib
            ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition()); // Competitor
            ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); // Add Judge Button

            var headerBorder = new Border()
            {
                Style = Application.Current.Resources["ScoreViewerHeaderBorderStyle"] as Style
            };
            ScoreGrid.Children.Add(headerBorder);
            Grid.SetRow(headerBorder, 0);
            Grid.SetColumn(headerBorder, 0);
            Grid.SetColumnSpan(headerBorder, 100);

            var promotedTextBlock = new TextBlock()
            {
                Text = "",
                Style = Application.Current.Resources["ScoreViewerHeaderTextStyle"] as Style
            };
            ScoreGrid.Children.Add(promotedTextBlock);
            Grid.SetRow(promotedTextBlock, 0);
            Grid.SetColumn(promotedTextBlock, PROMOTED_COLUMN);

            var countTextBlock = new TextBlock()
            {
                Text = "",
                Style = Application.Current.Resources["ScoreViewerHeaderTextStyle"] as Style
            };
            ScoreGrid.Children.Add(countTextBlock);
            Grid.SetRow(countTextBlock, 0);
            Grid.SetColumn(countTextBlock, COUNT_COLUMN);

            var bibTextBlock = new TextBlock()
            {
                Text = "Bib",
                Style = Application.Current.Resources["ScoreViewerHeaderTextStyle"] as Style
            };
            ScoreGrid.Children.Add(bibTextBlock);
            Grid.SetRow(bibTextBlock, 0);
            Grid.SetColumn(bibTextBlock, BIB_COLUMN);

            var competitorTextBlock = new TextBlock()
            {
                Text = "Competitor",
                Style = Application.Current.Resources["ScoreViewerHeaderTextStyle"] as Style
            };
            ScoreGrid.Children.Add(competitorTextBlock);
            Grid.SetRow(competitorTextBlock, 0);
            Grid.SetColumn(competitorTextBlock, COMPETITOR_COLUMN);

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
            Grid.SetColumn(addColumnButton, SCORE_COLUMN_START);

            var addRowBorder = new Border()
            {
                BorderThickness = new Thickness(0),
                BorderBrush = Brushes.Gray,
                Margin = new Thickness(1),
                Width = 30,
                HorizontalAlignment = HorizontalAlignment.Left
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
