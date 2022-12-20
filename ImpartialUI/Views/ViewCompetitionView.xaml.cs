using Impartial;
using System;
using System.Collections.Generic;
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

namespace ImpartialUI.Views
{
    /// <summary>
    /// Interaction logic for ViewCompetitionView.xaml
    /// </summary>
    public partial class ViewCompetitionView : UserControl
    {
        public ViewCompetitionView()
        {
            InitializeComponent();

            CompetitionComboBox.ItemsSource = App.DatabaseProvider.GetAllCompetitions();
        }

        private void CompetitionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                CompetitionViewerBox.Visibility = Visibility.Visible;
                CompetitionViewerBox.Competition = CompetitionComboBox.SelectedItem as Competition;
            }
            else
            {
                CompetitionViewerBox.Visibility = Visibility.Collapsed;
                CompetitionViewerBox.Competition = null;
            }

        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            CompetitionComboBox.ItemsSource = App.DatabaseProvider.GetAllCompetitions();
        }
    }
}
