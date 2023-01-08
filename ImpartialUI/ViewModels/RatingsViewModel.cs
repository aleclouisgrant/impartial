using Impartial;
using ImpartialUI.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace ImpartialUI.ViewModels
{
    public class RatingsViewModel : BaseViewModel
    {
        private string _firstName;
        public string FirstName
        {
            get { return _firstName; }
            set
            {
                _firstName = value;
                OnPropertyChanged();
            }
        }
        private string _lastName;
        public string LastName
        {
            get { return _lastName; }
            set
            {
                _lastName = value;
                OnPropertyChanged();
            }
        }

        private string _wsdcId;
        public string WsdcId
        {
            get { return _wsdcId; }
            set
            {
                _wsdcId = value;
                OnPropertyChanged();
            }
        }

        private List<Competitor> _competitors = new List<Competitor>();
        public List<Competitor> Competitors
        {
            get { return _competitors; }
            set
            {
                _competitors = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddCompetitorCommand { get; set; }
        public ICommand RefreshCompetitorsCommand { get; set; }
        public ICommand CrunchRatingsCommand { get; set; }

        private IDatabaseProvider _databaseProvider;

        public RatingsViewModel()
        {
            _databaseProvider = App.DatabaseProvider;

            AddCompetitorCommand = new DelegateCommand(AddCompetitor);
            RefreshCompetitorsCommand = new DelegateCommand(RefreshCompetitors);
            CrunchRatingsCommand = new DelegateCommand(CrunchRatings);
        }
        private async void CrunchRatings()
        {

            //IEnumerable<Competition> competitions = await _databaseProvider.GetAllCompetitionsAsync();
            //competitions = competitions.OrderBy(c => c.Date).ToList();
            
            //foreach (Competition competition in competitions)
            //{
            //    var couples = EloRatingService.CalculateRatings(competition.Couples);

            //    foreach (Couple couple in couples)
            //    {
            //        await App.DatabaseProvider.UpdateCompetitorAsync(couple.Leader.Id, couple.Leader);
            //        await App.DatabaseProvider.UpdateCompetitorAsync(couple.Follower.Id, couple.Follower);
            //    }
            //}
        }

        private async void AddCompetitor()
        {
            if (int.TryParse(WsdcId, out int id))
            {
                var newCompetitor = new Competitor(FirstName, LastName, int.Parse(WsdcId));

                await _databaseProvider.UpsertCompetitorAsync(newCompetitor);

                FirstName = string.Empty;
                LastName = string.Empty;
                WsdcId = string.Empty;
            }

            Competitors = (await _databaseProvider.GetAllCompetitorsAsync()).ToList();
        }

        private async void RefreshCompetitors()
        {
            Competitors = (await _databaseProvider.GetAllCompetitorsAsync()).ToList();
        }
    }
}
