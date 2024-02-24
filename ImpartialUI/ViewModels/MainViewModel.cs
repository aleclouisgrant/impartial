using Impartial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpartialUI.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private List<ICompetition> _competitionsCache = new List<ICompetition>();
        public List<ICompetition> CompetitionsCache
        {
            get { return _competitionsCache; }
            set
            {
                _competitionsCache = value;
                OnPropertyChanged();
            }
        }

        public List<ICompetitor> _competitorsCache = new List<ICompetitor>();
        public List<ICompetitor> CompetitorsCache
        {
            get { return _competitorsCache; }
            set
            {
                _competitorsCache = value;
                OnPropertyChanged();
            }
        }

        private List<IJudge> _judgeCache = new List<IJudge>();
        public List<IJudge> JudgesCache
        {
            get { return _judgeCache; }
            set
            {
                _judgeCache = value;
                OnPropertyChanged();
            }
        }

        public ViewCompetitionViewModel ViewCompetitionViewModel { get; set; }
        public AddCompetitionViewModel AddCompetitionViewModel { get; set; }
        public RatingsViewModel RatingsViewModel { get; set; }

        public MainViewModel()
        {
            //PopulateCaches();

            ViewCompetitionViewModel = new ViewCompetitionViewModel(CompetitionsCache);
            ViewCompetitionViewModel.PropertyChanged += ExceptionPropertyChanged;

            AddCompetitionViewModel = new AddCompetitionViewModel(CompetitorsCache, JudgesCache);
            AddCompetitionViewModel.PropertyChanged += ExceptionPropertyChanged;

            RatingsViewModel = new RatingsViewModel(CompetitorsCache, CompetitionsCache);
            RatingsViewModel.PropertyChanged += ExceptionPropertyChanged;
        }

        private void ExceptionPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Exception))
                Exception = ((BaseViewModel)sender).Exception;
        }

        private async void PopulateCaches()
        {
            CompetitionsCache = (await App.DatabaseProvider.GetAllCompetitionsAsync()).OrderBy(c => c.Date).ToList();
            CompetitorsCache = (await App.DatabaseProvider.GetAllCompetitorsAsync()).ToList();
            JudgesCache = (await App.DatabaseProvider.GetAllJudgesAsync()).ToList();
        }

    }
}
