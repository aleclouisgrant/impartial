using Impartial;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace ImpartialUI.Controls
{
    public partial class CompetitionAdder : UserControl
    {
        #region DependencyProperties

        public static readonly DependencyProperty CompetitionProperty = DependencyProperty.Register(
            nameof(Competition),
            typeof(Competition),
            typeof(CompetitionAdder),
            new FrameworkPropertyMetadata(new Competition(), OnCompetitionPropertyChanged));
        public Competition Competition
        {
            get { return (Competition)GetValue(CompetitionProperty); }
            set { SetValue(CompetitionProperty, value); }
        }
        private static async void OnCompetitionPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var viewer = (CompetitionAdder)source;
            var competition = (Competition)e.NewValue;

            var judges = competition.Judges;
            var couples = competition.Couples;

            viewer.Clear();

            foreach (var judge in judges)
            {
                viewer.AddJudge(judge);
            }

            foreach (var couple in couples)
            {
                viewer.AddCouple(couple);
            }
        }
        #endregion

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

        public ObservableCollection<Couple> Couples { get; set; } = new ObservableCollection<Couple>();

        private Border _addRowBorder;
        private Button _addColumnButton;

        public CompetitionAdder()
        {
            InitializeComponent();

            CompDatePicker.DisplayDate = DateTime.Now;

            UpdateCompetitors();
            UpdateJudges();
        }

        private async void UpdateCompetitors()
        {
            Competitors = (await App.DatabaseProvider.GetAllCompetitorsAsync()).ToList();
            foreach (SearchTextBox searchTextBox in _competitorBoxes)
            {
                searchTextBox.ItemsSource = Competitors;
            }
        }
        private async void UpdateJudges()
        {
            Judges = (await App.DatabaseProvider.GetAllJudgesAsync()).ToList();
            foreach (SearchTextBox searchTextBox in _judgeBoxes)
            {
                searchTextBox.ItemsSource = Judges;
            }
        }

        private void AddCouple(Couple couple = null)
        {
            ScoreGrid.RowDefinitions.Add(new RowDefinition());

            // placement
            var placementBorder = new Border()
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(1)
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
                Margin = new Thickness(1)
            };

            var leaderSearchBox = new SearchTextBox()
            {
                Margin = new Thickness(1),
                ItemsSource = Competitors
            };

            if (couple?.Leader != null)
            {
                leaderSearchBox.SelectedPerson = leaderSearchBox.ItemsSource.Where(c => c.Id == couple.Leader.Id).FirstOrDefault();
            }

            leaderBorder.Child = leaderSearchBox;
            leaderSearchBox.SelectionChanged += (s, e) =>
            {
                UpdateCompetition();
            };

            ScoreGrid.Children.Add(leaderBorder);
            Grid.SetColumn(leaderBorder, 1);
            Grid.SetRow(leaderBorder, ScoreGrid.RowDefinitions.Count - 2);

            var followerBorder = new Border()
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(1)
            };

            var followerSearchBox = new SearchTextBox()
            {
                Margin = new Thickness(1),
                ItemsSource = Competitors
            };

            if (couple?.Follower != null)
            {
                followerSearchBox.SelectedPerson = followerSearchBox.ItemsSource.Where(c => c.Id == couple.Follower.Id).FirstOrDefault();
            }

            followerBorder.Child = followerSearchBox;
            followerSearchBox.SelectionChanged += (s, e) =>
            {
                UpdateCompetition();
            };
            
            ScoreGrid.Children.Add(followerBorder);
            Grid.SetColumn(followerBorder, 2);
            Grid.SetRow(followerBorder, ScoreGrid.RowDefinitions.Count - 2);

            _competitorBoxes.Add(leaderSearchBox);
            _competitorBoxes.Add(followerSearchBox);

            // scores
            for (int i = 1; i < ScoreGrid.ColumnDefinitions.Count - 3; i++)
            {
                Border scoreBorder = new Border()
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(1)
                };

                TextBox scoreTextBox = new TextBox()
                {
                    Margin = new Thickness(1)
                };

                if (couple?.Scores != null)
                {
                    scoreTextBox.Text = couple.Scores[i - 1].Placement.ToString();
                }

                scoreBorder.Child = scoreTextBox;

                ScoreGrid.Children.Add(scoreBorder);
                Grid.SetRow(scoreBorder, ScoreGrid.RowDefinitions.Count() - 2);
                Grid.SetColumn(scoreBorder, i + 2);
            }

            Grid.SetRow(_addRowBorder, ScoreGrid.RowDefinitions.Count() - 1);

            Couples.Add(new Couple(
                (Competitor)leaderSearchBox.SelectedPerson, 
                (Competitor)followerSearchBox.SelectedPerson, 
                ScoreGrid.RowDefinitions.Count - 2));
        }
        private void AddJudge(Judge judge = null)
        {
            ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto } );
            //ScoreGrid.ColumnDefinitions[ScoreGrid.ColumnDefinitions.Count - 2].Width = new GridLength(1.0, GridUnitType.Star);

            Border judgeBorder = new Border()
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(1)
            };

            SearchTextBox judgeSearchBox = new SearchTextBox()
            {
                ItemsSource = Judges
            };

            if (judge != null)
            {
                judgeSearchBox.SelectedPerson = judgeSearchBox.ItemsSource.Where(j => j.Id == judge.Id).FirstOrDefault();
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
                    Margin = new Thickness(1)
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

            CompDatePicker.SelectedDate = DateTime.Now;

            var placeBorder = new Border()
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(1)
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
                Margin = new Thickness(1)
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
                Margin = new Thickness(1)
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
                Margin = new Thickness(1)
            };
            addColumnButton.Click += AddColumn_Click;

            ScoreGrid.Children.Add(addColumnButton);
            Grid.SetRow(addColumnButton, 0);
            Grid.SetColumn(addColumnButton, 3);

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

        private void UpdateCompetition()
        {
            Competition.Clear();

            Competition.Name = NameTextBox.Text;
            Competition.Date = CompDatePicker.DisplayDate;

            for (int placement = 1; placement <= ScoreGrid.RowDefinitions.Count - 2; placement++)
            {
                var leader = (Competitor)((SearchTextBox)ScoreGrid.Children.OfType<Border>().
                    First(c => Grid.GetRow(c) == placement && Grid.GetColumn(c) == 1).Child).SelectedPerson;
                var follower = (Competitor)((SearchTextBox)ScoreGrid.Children.OfType<Border>().
                    First(c => Grid.GetRow(c) == placement && Grid.GetColumn(c) == 2).Child).SelectedPerson;

                Competition.Couples.Add(new Couple(leader, follower, placement));

                for (int i = 3; i < ScoreGrid.ColumnDefinitions.Count - 1; i++)
                {
                    var judge = (Judge)((SearchTextBox)ScoreGrid.Children.OfType<Border>().
                        First(c => Grid.GetRow(c) == 0 && Grid.GetColumn(c) == i).Child).SelectedPerson;
                    var scoreBox = (TextBox)ScoreGrid.Children.OfType<Border>().
                        First(c => Grid.GetRow(c) == placement && Grid.GetColumn(c) == i).Child;

                    Competition.Scores.Add(new Score(Competition, judge, leader, follower, Int32.Parse(scoreBox.Text), placement));
                }
            }

            OnPropertyChanged(nameof(Competition));
            Trace.WriteLine(Competition.ToLongString());
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

        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // need to delete this once the selectionchanged event raises properly
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UpdateCompetition();
        }
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshItemSources();
        }
    }
}
