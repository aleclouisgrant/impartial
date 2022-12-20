using Impartial;
using ImpartialUI.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;

namespace ImpartialUI.ViewModels
{
    public class RankingsViewModel : BaseViewModel
    {
        private IDatabaseProvider _databaseProvider;

        private List<Competitor> _competitors = new List<Competitor>();
        public List<Competitor> Competitors
        {
            get { return _competitors; }
            set { 
                _competitors = value; 
                OnPropertyChanged(); 
            }
        }

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

        private Competition _competition;
        public Competition Competition
        {
            get { return _competition; }
            set
            {
                _competition = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddCompetitorCommand { get; set; }
        public ICommand AddCompetitionCommand { get; set; }

        public RankingsViewModel()
        {
            AddCompetitorCommand = new DelegateCommand(AddCompetitor);
            AddCompetitionCommand = new DelegateCommand(AddCompetition);
            
            _databaseProvider = App.DatabaseProvider;
            Competition = new Competition(Division.AllStar);
        }

        private void AddCompetitor()
        {
            if (int.TryParse(WsdcId, out int id)) {
                var newCompetitor = new Competitor(FirstName, LastName, int.Parse(WsdcId));
                _databaseProvider.InsertCompetitor(newCompetitor);

                FirstName = string.Empty;
                LastName = string.Empty;
                WsdcId = string.Empty;
            }
        }

        private void AddCompetition()
        {
            //Trace.WriteLine(Competition.ToLongString());

            _databaseProvider.InsertCompetition(Competition);

            Competition = new Competition(Division.AllStar);
        }
    }
}
