using Impartial;
using Impartial.Enums;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ImpartialUI.Controls
{
    public partial class CallbackScoreViewer : UserControl
    {
        public static readonly DependencyProperty CallbackScoreProperty = DependencyProperty.Register(
            nameof(CallbackScore),
            typeof(CallbackScore),
            typeof(CallbackScoreViewer),
            new FrameworkPropertyMetadata(CallbackScore.No, OnCallbackScorePropertyChanged));
        public CallbackScore CallbackScore
        {
            get { return (CallbackScore)GetValue(CallbackScoreProperty); }
            set { SetValue(CallbackScoreProperty, value); }
        }
        private static void OnCallbackScorePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var control = (CallbackScoreViewer)source;
            var callbackScore = (CallbackScore)e.NewValue;
            control.SetScore(callbackScore);
        }

        public CallbackScoreViewer()
        {
            InitializeComponent();
            SetScore(CallbackScore);
        }

        private void SetScore(CallbackScore callbackScore)
        {
            switch (callbackScore)
            {
                case CallbackScore.Yes:
                    MainBorder.Background = Application.Current.Resources["CallbackYesColor"] as SolidColorBrush;
                    break;
                case CallbackScore.Alt1:
                    MainBorder.Background = Application.Current.Resources["CallbackAlt1Color"] as SolidColorBrush;
                    break;
                case CallbackScore.Alt2:
                    MainBorder.Background = Application.Current.Resources["CallbackAlt2Color"] as SolidColorBrush;
                    break;
                case CallbackScore.Alt3:
                    MainBorder.Background = Application.Current.Resources["CallbackAlt3Color"] as SolidColorBrush;
                    break;
                default:
                case CallbackScore.No:
                    MainBorder.Background = Application.Current.Resources["CallbackNoColor"] as SolidColorBrush;
                    break;
            }

            MainTextBlock.Text = Util.CallbackScoreToString(callbackScore);
        }
    }
}
