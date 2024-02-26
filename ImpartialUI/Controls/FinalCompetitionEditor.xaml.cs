using Impartial;
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
    public partial class FinalCompetitionEditor : UserControl
    {
        #region DependencyProperties
        public static readonly DependencyProperty FinalCompetitionProperty = DependencyProperty.Register(
            nameof(FinalCompetition),
            typeof(IFinalCompetition),
            typeof(FinalCompetitionEditor),
            new FrameworkPropertyMetadata(new FinalCompetition(), OnCompetitionPropertyChanged));
        public IFinalCompetition FinalCompetition
        {
            get { return (IFinalCompetition)GetValue(FinalCompetitionProperty); }
            set { SetValue(FinalCompetitionProperty, value); }
        }
        private static void OnCompetitionPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var viewer = (FinalCompetitionEditor)source;
            var competition = (IFinalCompetition)e.NewValue;

            var judges = competition?.Judges.OrderBy(j => j.FullName);
            var couples = competition?.Couples.OrderBy(c => c.Placement);

            viewer.Clear();

            if (judges != null)
            {
                foreach (var judge in judges)
                {
                    viewer.AddJudge(judge);
                }
            }

            if (couples != null)
            {
                foreach (var couple in couples)
                {
                    viewer._coupleCount++;
                    viewer.AddCouple(couple);
                }
            }

            viewer.UpdateCompetition();
        }

        public static readonly DependencyProperty CompetitorsProperty = DependencyProperty.Register(
            nameof(Competitors),
            typeof(List<ICompetitor>),
            typeof(FinalCompetitionEditor),
            new FrameworkPropertyMetadata(new List<ICompetitor>(), OnCompetitorsPropertyChanged));
        public List<ICompetitor> Competitors
        {
            get { return (List<ICompetitor>)GetValue(CompetitorsProperty); }
            set { SetValue(CompetitorsProperty, value); }
        }
        private static void OnCompetitorsPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            ((FinalCompetitionEditor)source).UpdateCompetitors();
        }

        public static readonly DependencyProperty JudgesProperty = DependencyProperty.Register(
            nameof(Judges),
            typeof(List<IJudge>),
            typeof(FinalCompetitionEditor),
            new FrameworkPropertyMetadata(new List<IJudge>(), OnJudgesPropertyChanged));
        public List<IJudge> Judges
        {
            get { return (List<IJudge>)GetValue(JudgesProperty); }
            set { SetValue(JudgesProperty, value); }
        }
        private static void OnJudgesPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            ((FinalCompetitionEditor)source).UpdateJudges();
        }
        #endregion

        private List<SearchTextBox> _competitorBoxes = new List<SearchTextBox>();
        private List<SearchTextBox> _judgeBoxes = new List<SearchTextBox>();

        private Border _addRowBorder;
        private Button _addColumnButton;

        private int _coupleCount = 0;

        public FinalCompetitionEditor()
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
                    searchTextBox.SelectedPerson = searchTextBox.ItemsSource.Where(s => s.UserId == selectedId).FirstOrDefault();
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
                    searchTextBox.SelectedPerson = searchTextBox.ItemsSource.Where(s => s.UserId == selectedId).FirstOrDefault();
                }
                else
                {
                    searchTextBox.SelectedPerson = null;
                }
            }
        }

        private void AddCouple(ICouple couple = null)
        {
            ScoreGrid.RowDefinitions.Add(new RowDefinition());

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
                Text = (ScoreGrid.RowDefinitions.Count - 2).ToString(),
                Margin = new Thickness(1)
            };

            placementBorder.Child = placementTextBlock;

            ScoreGrid.Children.Add(placementBorder);
            Grid.SetColumn(placementBorder, 0);
            Grid.SetRow(placementBorder, ScoreGrid.RowDefinitions.Count - 2);

            // names
            var leaderBorder = new Border()
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(1),
                Height = 24
            };

            var leaderSearchBox = new SearchTextBox()
            {
                Margin = new Thickness(1),
                ItemsSource = Competitors,
                SelectedPerson = null,
                Placement = FinalCompetition.Couples.Count + 1
            };

            if (couple?.Leader != null)
            {
                leaderSearchBox.SelectedPerson = Util.FindCompetitorInCache(couple.Leader.FirstName, couple.Leader.LastName, (IEnumerable<ICompetitor>)leaderSearchBox.ItemsSource);

                if (leaderSearchBox.SelectedPerson == null)
                {
                    leaderSearchBox.AddMode(couple.Leader.FirstName, couple.Leader.LastName, couple.Leader.WsdcId);
                }
            }

            leaderBorder.Child = leaderSearchBox;
            leaderSearchBox.SelectionChanged += (s, e) =>
            {
                if (e.AddedItems.Count > 0)
                {
                    UpdateCompetitor((ICompetitor)e.AddedItems[0], e.Placement, Role.Leader);
                }
            };

            ScoreGrid.Children.Add(leaderBorder);
            Grid.SetColumn(leaderBorder, 1);
            Grid.SetRow(leaderBorder, ScoreGrid.RowDefinitions.Count - 2);

            var followerBorder = new Border()
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(1),
                Height = 24
            };

            var followerSearchBox = new SearchTextBox()
            {
                Margin = new Thickness(1),
                ItemsSource = Competitors,
                SelectedPerson = null,
            };

            if (couple != null)
            {
                leaderSearchBox.Placement = couple.Placement; 
                followerSearchBox.Placement = couple.Placement;
            }
            else
            {
                leaderSearchBox.Placement = FinalCompetition.Couples.Count;
                followerSearchBox.Placement = FinalCompetition.Couples.Count;
            }

            if (couple?.Follower != null)
            {
                followerSearchBox.SelectedPerson = Util.FindCompetitorInCache(couple.Follower.FirstName, couple.Follower.LastName, (IEnumerable<ICompetitor>)followerSearchBox.ItemsSource);

                if (followerSearchBox.SelectedPerson == null)
                {
                    followerSearchBox.AddMode(couple.Follower.FirstName, couple.Follower.LastName, couple.Follower.WsdcId);
                }
            }

            followerBorder.Child = followerSearchBox;
            followerSearchBox.SelectionChanged += (s, e) =>
            {
                if (e.AddedItems.Count > 0)
                {
                    UpdateCompetitor((ICompetitor)e.AddedItems[0], e.Placement, Role.Follower);
                }
            };
            
            ScoreGrid.Children.Add(followerBorder);
            Grid.SetColumn(followerBorder, 2);
            Grid.SetRow(followerBorder, ScoreGrid.RowDefinitions.Count - 2);

            _competitorBoxes.Add(leaderSearchBox);
            _competitorBoxes.Add(followerSearchBox);

            if (couple?.Scores != null)
            {
                couple.Scores = couple.Scores.OrderBy(s => s.Judge.FullName).ToList();
            }

            // scores
            for (int i = 1; i < ScoreGrid.ColumnDefinitions.Count - 3; i++)
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

                if (couple?.Scores != null)
                {
                    scoreTextBox.Text = couple.Scores[i - 1].Score.ToString();
                }

                scoreBorder.Child = scoreTextBox;

                ScoreGrid.Children.Add(scoreBorder);
                Grid.SetColumn(scoreBorder, i + 2);
                Grid.SetRow(scoreBorder, ScoreGrid.RowDefinitions.Count() - 2);
            }

            Grid.SetRow(_addRowBorder, ScoreGrid.RowDefinitions.Count() - 1);

            FinalCompetition.Couples.Add(new Couple(
                (ICompetitor)leaderSearchBox.SelectedPerson, 
                (ICompetitor)followerSearchBox.SelectedPerson, 
                ScoreGrid.RowDefinitions.Count - 2));
        }
        private void AddJudge(IJudge judge = null)
        {
            ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto } );
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
                ItemsSource = Judges,
                SelectedPerson = null
            };

            judgeSearchBox.SelectionChanged += (o, e) =>
            {
                UpdateCompetition();
            };

            if (judge != null)
            {
                judgeSearchBox.SelectedPerson = Util.FindJudgeInCache(judge.FirstName, judge.LastName, (IEnumerable<IJudge>)judgeSearchBox.ItemsSource);
            }

            judgeBorder.Child = judgeSearchBox;

            _judgeBoxes.Add(judgeSearchBox);

            ScoreGrid.Children.Add(judgeBorder);
            Grid.SetColumn(judgeBorder, ScoreGrid.ColumnDefinitions.Count() - 2);

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
                Grid.SetColumn(scoreBorder, ScoreGrid.ColumnDefinitions.Count() - 2);
            }

            Grid.SetColumn(_addColumnButton, ScoreGrid.ColumnDefinitions.Count() - 1);
        }

        private void Clear()
        {
            ScoreGrid.Children.Clear();
            ScoreGrid.RowDefinitions.Clear();
            ScoreGrid.ColumnDefinitions.Clear();

            ScoreGrid.RowDefinitions.Add(new RowDefinition());
            ScoreGrid.RowDefinitions.Add(new RowDefinition());

            ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
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
                Text = "Place",
                FontWeight = FontWeights.Bold,
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(1)
            };
            placeBorder.Child = placeTextBlock;

            ScoreGrid.Children.Add(placeBorder);
            Grid.SetRow(placeBorder, 0);
            Grid.SetColumn(placeBorder, 0);

            var leaderBorder = new Border()
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(1),
                Height = 24
            };

            var leaderTextBlock = new TextBlock()
            {
                Text = "Competitor (Leader)",
                FontWeight = FontWeights.Bold,
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(1)
            };
            leaderBorder.Child = leaderTextBlock;

            ScoreGrid.Children.Add(leaderBorder);
            Grid.SetRow(leaderBorder, 0);
            Grid.SetColumn(leaderBorder, 1);

            var followerBorder = new Border()
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(1),
                Height = 24
            };

            var followerTextBlock = new TextBlock()
            {
                Text = "Competitor (Follower)",
                FontWeight = FontWeights.Bold,
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(1)
            };
            followerBorder.Child = followerTextBlock;

            ScoreGrid.Children.Add(followerBorder);
            Grid.SetRow(followerBorder, 0);
            Grid.SetColumn(followerBorder, 2);

            var addColumnButton = new Button()
            {
                Content = "+",
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 20,
                Margin = new Thickness(1),
                Height = 24
            };
            addColumnButton.Click += AddColumn_Click;

            ScoreGrid.Children.Add(addColumnButton);
            Grid.SetRow(addColumnButton, 0);
            Grid.SetColumn(addColumnButton, 3);

            var addRowBorder = new Border()
            {
                BorderThickness = new Thickness(0),
                BorderBrush = Brushes.Gray,
                Margin = new Thickness(1),
                Height = 24
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

        private void UpdateCompetition()
        {
            if (FinalCompetition == null)
                return;

            FinalCompetition.Clear();

            for (int placement = 1; placement <= ScoreGrid.RowDefinitions.Count - 2; placement++)
            {
                var leader = (ICompetitor)((SearchTextBox)ScoreGrid.Children.OfType<Border>().
                    First(c => Grid.GetRow(c) == placement && Grid.GetColumn(c) == 1).Child).SelectedPerson;
                var follower = (ICompetitor)((SearchTextBox)ScoreGrid.Children.OfType<Border>().
                    First(c => Grid.GetRow(c) == placement && Grid.GetColumn(c) == 2).Child).SelectedPerson;

                FinalCompetition.Couples.Add(new Couple(leader, follower, placement));

                for (int i = 3; i < ScoreGrid.ColumnDefinitions.Count - 1; i++)
                {
                    var judge = (IJudge)((SearchTextBox)ScoreGrid.Children.OfType<Border>().
                        First(c => Grid.GetRow(c) == 0 && Grid.GetColumn(c) == i).Child).SelectedPerson;
                    var scoreBox = (TextBox)ScoreGrid.Children.OfType<Border>().
                        First(c => Grid.GetRow(c) == placement && Grid.GetColumn(c) == i).Child;

                    try
                    {
                        FinalCompetition.FinalScores.Add(new FinalScore(judge, leader, follower, Int32.Parse(scoreBox.Text), placement));
                    }
                    catch { }
                }
            }

            OnPropertyChanged(nameof(FinalCompetition));
        }

        private void UpdateCompetitor(ICompetitor competitor, int placement, Role role)
        {
            if (placement < 1 || placement > FinalCompetition.Couples.Count)
                return;

            if (role == Role.Leader)
            {
                var scores = FinalCompetition.FinalScores.Where(s => s.Placement == placement).ToList();
                foreach (var score in scores)
                {
                    score.Leader = competitor;
                }
            } else if (role == Role.Follower)
            {
                var scores = FinalCompetition.FinalScores.Where(s => s.Placement == placement).ToList();
                foreach (var score in scores)
                {
                    score.Follower = competitor;
                }
            }
        }

        private void RefreshItemSources()
        {
            UpdateCompetitors();
            UpdateJudges();
        }

        private void AddRow_Click(object sender, RoutedEventArgs e)
        {
            AddCouple();
        }
        private void AddColumn_Click(object sender, RoutedEventArgs e)
        {
            AddJudge();
        }
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshItemSources();
        }

        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
