using Impartial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImpartialUI.Controls
{
    public partial class SearchTextBox : UserControl 
    {
        #region DependencyProperties

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
            control.TextBlock2.Text = (string)e.NewValue;

            control.GuessJudge();
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable<Judge>),
            typeof(SearchTextBox),
            new FrameworkPropertyMetadata(null, OnItemsSourcePropertyChanged));
        public IEnumerable<Judge> ItemsSource
        {
            get { return (IEnumerable<Judge>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        private static void OnItemsSourcePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var control = (SearchTextBox)source;
            control.ComboBoxItems.ItemsSource = (IEnumerable<Judge>)e.NewValue;

            control.GuessJudge();
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
            var control = (SearchTextBox)source;
        }

        #endregion

        public SearchTextBox()
        {
            InitializeComponent();
        }

        private void GuessJudge()
        {
            if (ComboBoxItems.ItemsSource == null)
                return;

            ComboBoxItems.SelectedItem = GetClosestJudgeByFirstName(Text, ItemsSource.ToList());
        }

        private Judge GetClosestJudgeByFirstName(string input, List<Judge> list)
        {
            int leastDistance = 10000;
            Judge match = null;

            foreach (Judge j in list)
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

        private void AddToDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            DatabaseProvider.InsertJudge(new Judge(FirstNameTextBox.Text, LastNameTextBox.Text));

            SelectPersonGrid.Visibility = Visibility.Visible;
            AddPersonGrid.Visibility = Visibility.Collapsed;
        }
        private void ShowAddPersonButton_Click(object sender, RoutedEventArgs e)
        {
            AddPersonGrid.Visibility = Visibility.Visible;
            SelectPersonGrid.Visibility = Visibility.Collapsed;
        }
        private void HideAddPersonButton_Click(object sender, RoutedEventArgs e)
        {
            SelectPersonGrid.Visibility = Visibility.Visible;
            AddPersonGrid.Visibility = Visibility.Collapsed;
        }
        private void GuessButton_Click(object sender, RoutedEventArgs e)
        {
            GuessJudge();
        }
    }
}
