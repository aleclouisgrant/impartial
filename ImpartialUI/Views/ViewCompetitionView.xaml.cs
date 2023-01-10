using Impartial;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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

            RefreshCompetitions();
        }

        private async void RefreshCompetitions()
        {
            CompetitionComboBox.ItemsSource = (await App.DatabaseProvider.GetAllCompetitionsAsync()).OrderBy(c => c.Date);
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
            RefreshCompetitions();
        }
    }
}
