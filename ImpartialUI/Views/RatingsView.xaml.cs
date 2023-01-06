using ImpartialUI.ViewModels;
using System.Windows.Controls;

namespace ImpartialUI.Views
{
    public partial class RatingsView : UserControl
    {
        public RatingsView()
        {
            InitializeComponent();
            DataContext = new RatingsViewModel();
        }
    }
}
