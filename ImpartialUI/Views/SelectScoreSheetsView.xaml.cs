using ImpartialUI.ViewModels;
using System.Windows.Controls;

namespace ImpartialUI.Views
{
    public partial class SelectScoreSheetsView : UserControl
    {
        public SelectScoreSheetsView()
        {
            InitializeComponent();
            DataContext = new SelectScoreSheetsViewModel();
        }
    }
}
