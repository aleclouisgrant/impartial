using Impartial;
using System.Windows;
using System.Windows.Controls;

namespace ImpartialUI.Views
{
    public partial class ViewCompetitionView : UserControl
    {
        private bool _editMode = false;

        public ViewCompetitionView()
        {
            InitializeComponent();
        }

        private void EditMode()
        {
            //TODO:
            //if (CompetitionComboBox.SelectedValue != null)
            //{
            //    _editMode = true;

            //    CompetitionViewerGrid.Visibility = Visibility.Collapsed;
            //    CompetitionEditorGrid.Visibility = Visibility.Visible;

            //    EditButton.Content = "View";
            //    RefreshButton.Visibility = Visibility.Collapsed;
            //    SaveButton.Visibility = Visibility.Visible;

            //    CompetitionComboBox.IsEnabled = false;

            //    if (((ICompetition)CompetitionComboBox.SelectedValue).HasRound(1))
            //    {
            //        PrelimsEditorGrid.Visibility = Visibility.Visible;
            //    }
            //    else
            //    {
            //        PrelimsEditorGrid.Visibility = Visibility.Collapsed;
            //    }

            //    if (((ICompetition)CompetitionComboBox.SelectedValue).HasRound(2))
            //    {
            //        SemisEditorGrid.Visibility = Visibility.Visible;
            //    }
            //    else
            //    {
            //        SemisEditorGrid.Visibility = Visibility.Collapsed;
            //    }
            //}
        }
        private void ViewMode()
        {
            _editMode = false;
            
            CompetitionEditorGrid.Visibility = Visibility.Collapsed;
            SemisEditorGrid.Visibility = Visibility.Collapsed;
            PrelimsEditorGrid.Visibility = Visibility.Collapsed;

            EditButton.Content = "Edit";
            RefreshButton.Visibility = Visibility.Visible;
            SaveButton.Visibility = Visibility.Collapsed;

            CompetitionComboBox.IsEnabled = true;
        }

        private void CompetitionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (e.AddedItems.Count > 0)
            //{
            //    CompetitionViewerGrid.Visibility = Visibility.Visible;
                
            //    if (((ICompetition)e.AddedItems[0]).HasRound(1))
            //    {
            //        PrelimsViewerGrid.Visibility = Visibility.Visible;
            //    }
            //    else
            //    {
            //        PrelimsViewerGrid.Visibility = Visibility.Collapsed;
            //    }

            //    if (((ICompetition)e.AddedItems[0]).HasRound(2))
            //    {
            //        SemisViewerGrid.Visibility = Visibility.Visible;
            //    }
            //    else
            //    {
            //        SemisViewerGrid.Visibility = Visibility.Collapsed;
            //    }
            //}
            //else
            //{
            //    CompetitionViewerGrid.Visibility = Visibility.Collapsed;
            //    PrelimsViewerGrid.Visibility = Visibility.Collapsed;
            //    SemisViewerGrid.Visibility = Visibility.Collapsed;
            //}
        }
        private void EditCompetitionButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_editMode)
            {
                EditMode();
            }
            else
            {
                ViewMode();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ViewMode();
        }
    }
}
