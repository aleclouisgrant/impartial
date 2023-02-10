using Impartial;
using System.Collections.Generic;
using System.Linq;

namespace ImpartialUI.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private List<Competition> _competitionsCache = new List<Competition>();
        public List<Competition> CompetitionsCache
        {
            get { return _competitionsCache; }
            set
            {
                _competitionsCache = value;
                OnPropertyChanged();
            }
        }

        public List<Competitor> _competitorsCache = new List<Competitor>();
        public List<Competitor> CompetitorsCache
        {
            get { return _competitorsCache; }
            set
            {
                _competitorsCache = value;
                OnPropertyChanged();
            }
        }

        private List<Judge> _judgeCache = new List<Judge>();
        public List<Judge> JudgesCache
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
            AddCompetitionViewModel = new AddCompetitionViewModel(CompetitorsCache, JudgesCache);
            RatingsViewModel = new RatingsViewModel(CompetitorsCache, CompetitionsCache);
        }

        private async void PopulateCaches()
        {
            CompetitionsCache = (await App.DatabaseProvider.GetAllCompetitionsAsync()).OrderBy(c => c.Date).ToList();
            CompetitorsCache = (await App.DatabaseProvider.GetAllCompetitorsAsync()).ToList();
            JudgesCache = (await App.DatabaseProvider.GetAllJudgesAsync()).ToList();
        }

    }
}
