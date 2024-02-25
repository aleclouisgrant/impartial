namespace ImpartialUI.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public ViewCompetitionViewModel ViewCompetitionViewModel { get; set; }
        public AddCompetitionViewModel AddCompetitionViewModel { get; set; }
        public RatingsViewModel RatingsViewModel { get; set; }

        public MainViewModel()
        {
            ViewCompetitionViewModel = new ViewCompetitionViewModel();
            ViewCompetitionViewModel.PropertyChanged += ExceptionPropertyChanged;

            AddCompetitionViewModel = new AddCompetitionViewModel();
            AddCompetitionViewModel.PropertyChanged += ExceptionPropertyChanged;

            RatingsViewModel = new RatingsViewModel();
            RatingsViewModel.PropertyChanged += ExceptionPropertyChanged;
        }

        private void ExceptionPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Exception))
                Exception = ((BaseViewModel)sender).Exception;
        }
    }
}
