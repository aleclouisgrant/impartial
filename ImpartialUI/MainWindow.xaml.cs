using ImpartialUI.ViewModels;
using System.Windows;

namespace ImpartialUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
