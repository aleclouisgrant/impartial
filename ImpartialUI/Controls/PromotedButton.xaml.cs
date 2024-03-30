using Impartial.Enums;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ImpartialUI.Controls
{
    public partial class PromotedButton : UserControl
    {
        public static readonly DependencyProperty CallbackScoreProperty = DependencyProperty.Register(
            nameof(CallbackScore),
            typeof(CallbackScore),
            typeof(PromotedButton),
            new FrameworkPropertyMetadata(CallbackScore.No, OnCallbackScorePropertyChanged));
        public CallbackScore CallbackScore
        {
            get { return (CallbackScore)GetValue(CallbackScoreProperty); }
            set { SetValue(CallbackScoreProperty, value); }
        }
        private static void OnCallbackScorePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var control = ((PromotedButton)source);
            var callbackScore = (CallbackScore)e.NewValue;
            control.SetScore(callbackScore);
        }

        public static readonly DependencyProperty EditableProperty = DependencyProperty.Register(
            nameof(SetEditable),
            typeof(bool),
            typeof(PromotedButton),
            new FrameworkPropertyMetadata(true, OnEditablePropertyChanged));
        public bool Editable
        {
            get { return (bool)GetValue(EditableProperty); }
            set { SetValue(EditableProperty, value); }
        }
        private static void OnEditablePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var control = (PromotedButton)source;
            var value = (bool)e.NewValue;
            control.SetEditable(value);
        }

        public PromotedButton()
        {
            InitializeComponent();
            SetScore(CallbackScore);
            SetEditable(Editable);
        }

        private void MainButton_Click(object sender, RoutedEventArgs e)
        {
            Toggle();
        }

        private void Toggle()
        {
            switch (CallbackScore)
            {
                case CallbackScore.Yes:
                    CallbackScore = CallbackScore.Alt1;
                    break;
                case CallbackScore.Alt1:
                    CallbackScore = CallbackScore.Alt2;
                    break;
                case CallbackScore.Alt2:
                    CallbackScore = CallbackScore.Alt3;
                    break;
                case CallbackScore.Alt3:
                    CallbackScore = CallbackScore.No;
                    break;
                case CallbackScore.No:
                    CallbackScore = CallbackScore.Yes;
                    break;
            }
        }

        private void SetScore(CallbackScore callbackScore) 
        {
            switch (callbackScore)
            {
                case CallbackScore.Yes:
                    MainButton.Background = Application.Current.Resources["CallbackYesColor"] as SolidColorBrush;
                    break;
                case CallbackScore.Alt1:
                    MainButton.Background = Application.Current.Resources["CallbackAlt1Color"] as SolidColorBrush;
                    break;
                case CallbackScore.Alt2:
                    MainButton.Background = Application.Current.Resources["CallbackAlt2Color"] as SolidColorBrush;
                    break;
                case CallbackScore.Alt3:
                    MainButton.Background = Application.Current.Resources["CallbackAlt3Color"] as SolidColorBrush;
                    break;
                default:
                case CallbackScore.No:
                    MainButton.Background = Application.Current.Resources["CallbackNoColor"] as SolidColorBrush;
                    break;
            }
        }

        private void SetEditable(bool value)
        {
            if (value)
            {
                MainButton.Click += MainButton_Click;
            }
            else
            {
                MainButton.Click -= MainButton_Click;
            }
        }
    }
}
