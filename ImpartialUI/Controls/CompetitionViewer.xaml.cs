using Impartial;
using System.Windows;
using System.Windows.Controls;

namespace ImpartialUI.Controls
{
    public partial class CompetitionViewer : UserControl
    {
        #region DependencyProperties

        public static readonly DependencyProperty CompetitionProperty = DependencyProperty.Register(
            nameof(Competition),
            typeof(Competition),
            typeof(CompetitionViewer),
            new FrameworkPropertyMetadata(false, OnCompetitionPropertyChanged));
        public Competition Competition
        {
            get { return (Competition)GetValue(CompetitionProperty); }
            set { SetValue(CompetitionProperty, value); }
        }
        private static void OnCompetitionPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            CompetitionViewer viewer = (CompetitionViewer)source;
        }

        #endregion
        public CompetitionViewer()
        {
            InitializeComponent();
        }
    }
}
