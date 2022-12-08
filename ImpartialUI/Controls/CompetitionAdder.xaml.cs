using Impartial;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
        private static void OnCompetitionPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var viewer = (CompetitionAdder)source;
            var competition = (Competition)e.NewValue;

            var judges = competition.Judges;
            var couples = competition.Couples;

            foreach (var score in competition.Scores)
            {
                if (score.Judge.Scores == null)
                    score.Judge.Scores = new List<Score>();

                score.Judge.Accuracy = Math.Round(score.Judge.Scores.Sum(s => s.Accuracy) / score.Judge.Scores.Count, 2);
                score.Judge.Top5Accuracy = Math.Round(score.Judge.Scores.FindAll(s => s.ActualPlacement <= 5).Sum(s => s.Accuracy) / 5, 2);
            }

            // judge names
            foreach (var judge in judges)
            {
                viewer.ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

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

                //var searchText = new SearchTextBox()
                //{
                //    Margin = new Thickness(2, 0, 2, 0),
                //    DatabaseProvider = App.DatabaseProvider, //TODO: shouldn't do this
                //    Text = judge.FullName,
                //    ItemsSource = App.DatabaseProvider.GetAllJudges() //TODO: also shouldnt do this
                //};
                //border.Child = searchText;

                viewer.ScoreGrid.Children.Add(border);
                Grid.SetColumn(border, viewer.ScoreGrid.Children.Count - 2);
                Grid.SetRow(border, 0);
            }

            foreach (var couple in couples)
            {
                viewer.ScoreGrid.RowDefinitions.Add(new RowDefinition());

                // placement
                var placementBorder = new Border()
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(1)
                };

                var placementTextBlock = new TextBlock()
                {
                    Text = couple.ActualPlacement.ToString(),
                    Margin = new Thickness(1)
                };

                placementBorder.Child = placementTextBlock;

                viewer.ScoreGrid.Children.Add(placementBorder);
                Grid.SetColumn(placementBorder, 0);
                Grid.SetRow(placementBorder, couple.ActualPlacement);

                // names
                var nameBorder = new Border()
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(1)
                };

                var nameTextBlock = new TextBlock()
                {
                    Text = couple.Leader.FullName + " and " + couple.Follower.FullName,
                    Margin = new Thickness(1)
                };

                nameBorder.Child = nameTextBlock;

                viewer.ScoreGrid.Children.Add(nameBorder);
                Grid.SetColumn(nameBorder, 1);
                Grid.SetRow(nameBorder, couple.ActualPlacement);

                // scores
                for (int i = 0; i < couple.Scores.Count; i++)
                {
                    var score = couple.Scores[i];

                    var border = new Border()
                    {
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(1),
                        Margin = new Thickness(1)
                    };

                    var textBlock = new TextBlock()
                    {
                        Margin = new Thickness(1)
                    };

                    if (score.Placement == score.ActualPlacement){
                        textBlock.Text = score.Placement.ToString();
                    }
                    else
                    {
                        textBlock.Inlines.Add(new Run()
                        {
                            Text = score.Placement.ToString()
                        });
                        textBlock.Inlines.Add(new Run()
                        {
                            Text = " (" + (-1 * Math.Abs(score.Placement - score.ActualPlacement)).ToString() + ")",
                            Foreground = Brushes.Red
                        });
                    }

                    border.Child = textBlock;

                    viewer.ScoreGrid.Children.Add(border);
                    Grid.SetColumn(border, i + 2);
                    Grid.SetRow(border, couple.ActualPlacement);
                }
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

        public ObservableCollection<Couple> Couples { get; set; } = new ObservableCollection<Couple>();

        public CompetitionAdder()
        {
            Competitors = App.DatabaseProvider.GetAllCompetitors();
            InitializeComponent();

            AddRow();
        }

        private void UpdateCompetitors()
        {
            Competitors = App.DatabaseProvider.GetAllCompetitors();
        }

        private void AddRow()
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

            followerBorder.Child = followerSearchBox;
            followerSearchBox.SelectionChanged += (s, e) =>
            {
                UpdateCompetition();
            };
            
            ScoreGrid.Children.Add(followerBorder);
            Grid.SetColumn(followerBorder, 2);
            Grid.SetRow(followerBorder, ScoreGrid.RowDefinitions.Count - 2);

            // scores
            //for (int i = 0; i < couple.Scores.Count; i++)
            //{
            //    var score = couple.Scores[i];

            //    var border = new Border()
            //    {
            //        BorderBrush = Brushes.Gray,
            //        BorderThickness = new Thickness(1),
            //        Margin = new Thickness(1)
            //    };

            //    var textBlock = new TextBlock()
            //    {
            //        Margin = new Thickness(1)
            //    };

            //    if (score.Placement == score.ActualPlacement)
            //    {
            //        textBlock.Text = score.Placement.ToString();
            //    }
            //    else
            //    {
            //        textBlock.Inlines.Add(new Run()
            //        {
            //            Text = score.Placement.ToString()
            //        });
            //        textBlock.Inlines.Add(new Run()
            //        {
            //            Text = " (" + (-1 * Math.Abs(score.Placement - score.ActualPlacement)).ToString() + ")",
            //            Foreground = Brushes.Red
            //        });
            //    }

            //    border.Child = textBlock;

            //    ScoreGrid.Children.Add(border);
            //    Grid.SetColumn(border, i + 2);
            //    Grid.SetRow(border, couple.ActualPlacement);
            //}

            Grid.SetRow(AddRowBorder, ScoreGrid.RowDefinitions.Count() - 1);

            Couples.Add(new Couple(
                (Competitor)leaderSearchBox.SelectedPerson, 
                (Competitor)followerSearchBox.SelectedPerson, 
                ScoreGrid.RowDefinitions.Count - 2));
        }

        private void UpdateCompetition()
        {
            Competition.Clear();

            for (int placement = 1; placement < ScoreGrid.RowDefinitions.Count - 1; placement++)
            {
                var leaderSearchBox = (SearchTextBox)ScoreGrid.Children.Cast<Border>().
                    First(c => Grid.GetRow(c) == placement && Grid.GetColumn(c) == 1).Child;
                var followerSearchBox = (SearchTextBox)ScoreGrid.Children.Cast<Border>().
                    First(c => Grid.GetRow(c) == placement && Grid.GetColumn(c) == 2).Child;

                Competition.Couples.Add(new Couple(
                    (Competitor)leaderSearchBox.SelectedPerson, 
                    (Competitor)followerSearchBox.SelectedPerson, 
                    placement));
            }

            OnPropertyChanged(nameof(Competition));
        }

        private void AddRow_Click(object sender, RoutedEventArgs e)
        {
            AddRow();
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
    }
}
