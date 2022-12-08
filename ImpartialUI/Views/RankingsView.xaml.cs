using ImpartialUI.ViewModels;
using System.Windows.Controls;

namespace ImpartialUI.Views
{
    public partial class RankingsView : UserControl
    {
        public RankingsView()
        {
            InitializeComponent();
            DataContext = new RankingsViewModel();
        }
    }
}
