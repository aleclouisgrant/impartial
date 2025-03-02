﻿using Impartial;
using ImpartialUI.Enums;
using ImpartialUI.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ImpartialUI.Controls
{
    public partial class SearchTextBox : UserControl
    {
        #region DependencyProperties

        public static readonly DependencyProperty SelectedPersonProperty = DependencyProperty.Register(
            nameof(SelectedPerson),
            typeof(IUser),
            typeof(SearchTextBox),
            new FrameworkPropertyMetadata());
        public IUser SelectedPerson
        {
            get { return (IUser)GetValue(SelectedPersonProperty); }
            set { SetValue(SelectedPersonProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(SearchTextBox),
            new FrameworkPropertyMetadata(string.Empty, OnTextPropertyChanged));
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        private static void OnTextPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var control = (SearchTextBox)source;
            control.TextBlock1.Text = (string)e.NewValue;
            control.GuessPerson();
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable<IUser>),
            typeof(SearchTextBox),
            new FrameworkPropertyMetadata(null, OnItemsSourcePropertyChanged));
        public IEnumerable<IUser> ItemsSource
        {
            get { return (IEnumerable<IUser>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        private static void OnItemsSourcePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var control = (SearchTextBox)source;
            control.ComboBoxItems.ItemsSource = (IEnumerable<IUser>)e.NewValue;

            control.GuessPerson();
        }

        public static readonly DependencyProperty DatabaseProviderProperty = DependencyProperty.Register(
            nameof(DatabaseProvider),
            typeof(IDatabaseProvider),
            typeof(SearchTextBox),
            new FrameworkPropertyMetadata(null, OnDatabaseProviderPropertyChanged));
        public IDatabaseProvider DatabaseProvider
        {
            get { return (IDatabaseProvider)GetValue(DatabaseProviderProperty); }
            set { SetValue(DatabaseProviderProperty, value); }
        }
        private static void OnDatabaseProviderPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
        }

        public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(
            nameof(Mode),
            typeof(SearchBoxMode),
            typeof(SearchTextBox),
            new FrameworkPropertyMetadata(SearchBoxMode.Search, OnModePropertyChanged));
        public SearchBoxMode Mode
        {
            get { return (SearchBoxMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }
        private static void OnModePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var control = (SearchTextBox)source;
            var mode = (SearchBoxMode)e.NewValue;
            
            switch (mode)
            {
                case SearchBoxMode.Add:
                    control.AddMode();
                    break;
                default:
                case SearchBoxMode.Search:
                    control.SearchMode();
                    break;
            }
        }

        public static readonly DependencyProperty PlacementProperty = DependencyProperty.Register(
            nameof(Placement),
            typeof(int),
            typeof(SearchTextBox),
            new FrameworkPropertyMetadata(0));
        public int Placement
        {
            get { return (int)GetValue(PlacementProperty); }
            set { SetValue(PlacementProperty, value); }
        }
        #endregion

        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(
            "SelectionChanged", 
            RoutingStrategy.Bubble, 
            typeof(SearchTextBoxSelectionChangedEventHandler), 
            typeof(SearchTextBox));

        public event SearchTextBoxSelectionChangedEventHandler SelectionChanged;

        protected virtual void OnSelectionChanged(SearchTextBoxSelectionChangedEventArgs eventArgs)
        {
            SelectionChanged?.Invoke(this, eventArgs);
        }

        private string _cancelledPersonFirstName = "";
        private string _cancelledPersonLastName = "";
        private bool _wasCancelled = false;

        public SearchTextBox()
        {
            InitializeComponent();
        }

        public void AddMode(string firstName, string lastName, int wsdcId)
        {
            AddMode();

            FirstNameTextBox.Text = firstName;
            LastNameTextBox.Text = lastName;
            WsdcIdTextBox.Text = wsdcId.ToString();

            _cancelledPersonFirstName = firstName;
            _cancelledPersonLastName = lastName;
        }

        private void GuessPerson()
        {
            if (ComboBoxItems.ItemsSource == null)
                return;

            ComboBoxItems.SelectedItem = GetClosestPersonByFirstName(Text, ItemsSource.ToList());
        }

        private IUser GetClosestPersonByFirstName(string input, List<IUser> list)
        {
            int leastDistance = 10000;
            IUser match = null;

            foreach (IUser j in list)
            {
                int d = GetEditDistance(input, j.FirstName);
                if (d == 0)
                    return j;

                if (d < leastDistance)
                {
                    leastDistance = d;
                    match = j;
                }
            }

            return match;
        }
        private string GetClosestString(string input, List<string> list)
        {
            int leastDistance = 10000;
            string match = "";

            foreach (string s in list)
            {
                int d = GetEditDistance(input, s);
                if (d == 0)
                    return s;

                if (d < leastDistance)
                {
                    leastDistance = d;
                    match = s;
                }
            }

            return match;
        }
        public int GetEditDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0)
                return m;

            if (m == 0)
                return n;

            for (int i = 0; i <= n; d[i, 0] = i++) { }
            for (int j = 0; j <= m; d[0, j] = j++) { }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }

        private void AddMode()
        {
            Mode = SearchBoxMode.Add;
            AddPersonGrid.Visibility = Visibility.Visible;
            SelectPersonGrid.Visibility = Visibility.Collapsed;
        }

        private void SearchMode()
        {
            Mode = SearchBoxMode.Search;
            SelectPersonGrid.Visibility = Visibility.Visible;
            AddPersonGrid.Visibility = Visibility.Collapsed;
        }

        private async void AddToDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            //if (typeof(ComboBoxItems.SelectedItem) is Judge)
            //    DatabaseProvider.InsertJudge(new Judge(FirstNameTextBox.Text, LastNameTextBox.Text));
            //else if (typeof(ComboBoxItems.SelectedItem) is Competitor)
            //    DatabaseProvider.InsertCompetitor(new Competitor(FirstNameTextBox.Text, LastNameTextBox.Text));

            var competitor = new Competitor(FirstNameTextBox.Text, LastNameTextBox.Text, Int32.Parse(WsdcIdTextBox.Text));
            await App.DatabaseProvider.UpsertCompetitorAsync(competitor);

            App.CompetitorsDb.Add(competitor);
            App.CompetitorsDb = App.CompetitorsDb.OrderBy(c => c.FullName).ToList();
            ItemsSource = App.CompetitorsDb;

            SearchMode();
            ComboBoxItems.SelectedValue = ItemsSource.Where(u => u.UserId == competitor.UserId).FirstOrDefault();

            List<ICompetitor> removedItems = new List<ICompetitor>(){
                new Competitor(_cancelledPersonFirstName, _cancelledPersonLastName)
            };
            List<ICompetitor> addedItems = new List<ICompetitor>(){
                (ICompetitor)ComboBoxItems.SelectedValue
            };

            OnSelectionChanged(new SearchTextBoxSelectionChangedEventArgs(SearchTextBox.SelectionChangedEvent, removedItems, addedItems, Placement));
        }
        private void ShowAddPersonButton_Click(object sender, RoutedEventArgs e)
        {
            _wasCancelled = false;
            AddMode();
        }
        private void HideAddPersonButton_Click(object sender, RoutedEventArgs e)
        {
            _wasCancelled = true;
            SearchMode();
        }
        private void GuessButton_Click(object sender, RoutedEventArgs e)
        {
            GuessPerson();
        }

        private void ComboBoxItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SearchTextBoxSelectionChangedEventArgs eventArgs;

            if (_wasCancelled)
            {
                var removedItems = new List<IUser> {
                    new Competitor(_cancelledPersonFirstName, _cancelledPersonLastName) };

                eventArgs = new SearchTextBoxSelectionChangedEventArgs(SelectionChangedEvent, removedItems, e.AddedItems, Placement);
            }
            else 
            {
                eventArgs = new SearchTextBoxSelectionChangedEventArgs(SelectionChangedEvent, e.RemovedItems, e.AddedItems, Placement);
            }

            OnSelectionChanged(eventArgs);
        }
    }
}
