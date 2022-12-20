﻿using Impartial;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
            //var viewer = (CompetitionAdder)source;
            //var competition = (Competition)e.NewValue;

            //var judges = competition.Judges;
            //var couples = competition.Couples;

            //foreach (var score in competition.Scores)
            //{
            //    if (score.Judge.Scores == null)
            //        score.Judge.Scores = new List<Score>();

            //    score.Judge.Accuracy = Math.Round(score.Judge.Scores.Sum(s => s.Accuracy) / score.Judge.Scores.Count, 2);
            //    score.Judge.Top5Accuracy = Math.Round(score.Judge.Scores.FindAll(s => s.ActualPlacement <= 5).Sum(s => s.Accuracy) / 5, 2);
            //}

            //// judge names
            //foreach (var judge in judges)
            //{
            //    viewer.ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            //    var border = new Border()
            //    {
            //        BorderBrush = Brushes.Gray,
            //        BorderThickness = new Thickness(1),
            //        Margin = new Thickness(1)
            //    };

            //    var textBlock = new TextBlock()
            //    {
            //        Text = judge.FullName + " (" + judge.Accuracy.ToString() + ")" + "(" + judge.Top5Accuracy.ToString() + ")",
            //        FontWeight = FontWeights.Bold,
            //        FontStyle = FontStyles.Italic,
            //        Margin = new Thickness(1)
            //    };
            //    border.Child = textBlock;

            //    //var searchText = new SearchTextBox()
            //    //{
            //    //    Margin = new Thickness(2, 0, 2, 0),
            //    //    DatabaseProvider = App.DatabaseProvider, //TODO: shouldn't do this
            //    //    Text = judge.FullName,
            //    //    ItemsSource = App.DatabaseProvider.GetAllJudges() //TODO: also shouldnt do this
            //    //};
            //    //border.Child = searchText;

            //    viewer.ScoreGrid.Children.Add(border);
            //    Grid.SetColumn(border, viewer.ScoreGrid.Children.Count - 2);
            //    Grid.SetRow(border, 0);
            //}

            //foreach (var couple in couples)
            //{
            //    viewer.ScoreGrid.RowDefinitions.Add(new RowDefinition());

            //    // placement
            //    var placementBorder = new Border()
            //    {
            //        BorderBrush = Brushes.Gray,
            //        BorderThickness = new Thickness(1),
            //        Margin = new Thickness(1)
            //    };

            //    var placementTextBlock = new TextBlock()
            //    {
            //        Text = couple.ActualPlacement.ToString(),
            //        Margin = new Thickness(1)
            //    };

            //    placementBorder.Child = placementTextBlock;

            //    viewer.ScoreGrid.Children.Add(placementBorder);
            //    Grid.SetColumn(placementBorder, 0);
            //    Grid.SetRow(placementBorder, couple.ActualPlacement);

            //    // names
            //    var nameBorder = new Border()
            //    {
            //        BorderBrush = Brushes.Gray,
            //        BorderThickness = new Thickness(1),
            //        Margin = new Thickness(1)
            //    };

            //    var nameTextBlock = new TextBlock()
            //    {
            //        Text = couple.Leader.FullName + " and " + couple.Follower.FullName,
            //        Margin = new Thickness(1)
            //    };

            //    nameBorder.Child = nameTextBlock;

            //    viewer.ScoreGrid.Children.Add(nameBorder);
            //    Grid.SetColumn(nameBorder, 1);
            //    Grid.SetRow(nameBorder, couple.ActualPlacement);

            //    // scores
            //    for (int i = 0; i < couple.Scores.Count; i++)
            //    {
            //        var score = couple.Scores[i];

            //        var border = new Border()
            //        {
            //            BorderBrush = Brushes.Gray,
            //            BorderThickness = new Thickness(1),
            //            Margin = new Thickness(1)
            //        };

            //        var textBlock = new TextBlock()
            //        {
            //            Margin = new Thickness(1)
            //        };

            //        if (score.Placement == score.ActualPlacement){
            //            textBlock.Text = score.Placement.ToString();
            //        }
            //        else
            //        {
            //            textBlock.Inlines.Add(new Run()
            //            {
            //                Text = score.Placement.ToString()
            //            });
            //            textBlock.Inlines.Add(new Run()
            //            {
            //                Text = " (" + (-1 * Math.Abs(score.Placement - score.ActualPlacement)).ToString() + ")",
            //                Foreground = Brushes.Red
            //            });
            //        }

            //        border.Child = textBlock;

            //        viewer.ScoreGrid.Children.Add(border);
            //        Grid.SetColumn(border, i + 2);
            //        Grid.SetRow(border, couple.ActualPlacement);
            //    }
            //}
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

        public ObservableCollection<Couple> Couples { get; set; } = new ObservableCollection<Couple>();

        public CompetitionAdder()
        {
            Competitors = App.DatabaseProvider.GetAllCompetitors();
            Judges = App.DatabaseProvider.GetAllJudges();

            InitializeComponent();

            CompDatePicker.DisplayDateStart = DateTime.Now;
            CompDatePicker.DisplayDate = DateTime.Now;

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

                scoreBorder.Child = scoreTextBox;

                ScoreGrid.Children.Add(scoreBorder);
                Grid.SetRow(scoreBorder, ScoreGrid.RowDefinitions.Count() - 2);
                Grid.SetColumn(scoreBorder, i + 2);
            }

            Grid.SetRow(AddRowBorder, ScoreGrid.RowDefinitions.Count() - 1);

            Couples.Add(new Couple(
                (Competitor)leaderSearchBox.SelectedPerson, 
                (Competitor)followerSearchBox.SelectedPerson, 
                ScoreGrid.RowDefinitions.Count - 2));
        }
        private void AddColumn()
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

            judgeBorder.Child = judgeSearchBox;

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

            Grid.SetColumn(AddColumnButton, ScoreGrid.ColumnDefinitions.Count() - 1);

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

                    Competition.Scores.Add(new Score(judge, leader, follower, Int32.Parse(scoreBox.Text), placement));
                }
            }

            OnPropertyChanged(nameof(Competition));
            Trace.WriteLine(Competition.ToLongString());
        }

        private void AddRow_Click(object sender, RoutedEventArgs e)
        {
            AddRow();
        }
        private void AddColumn_Click(object sender, RoutedEventArgs e)
        {
            AddColumn();
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
