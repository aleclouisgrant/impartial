using Impartial;
using System.Windows;
using System.Windows.Controls;

namespace ImpartialUI.Views
{
    public partial class ViewCompetitionView : UserControl
    {
        public ViewCompetitionView()
        {
            InitializeComponent();
        }

        private void CompetitionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                CompetitionViewerGrid.Visibility = Visibility.Visible;
                if (((Competition)e.AddedItems[0]).FollowerPrelimScores.Count > 0)
                    PrelimsGrid.Visibility = Visibility.Visible;
            }
            else
            {
                CompetitionViewerGrid.Visibility = Visibility.Collapsed;
                PrelimsGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void EditCompetitionButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
