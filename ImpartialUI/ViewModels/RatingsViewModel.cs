using Impartial;
using ImpartialUI.Commands;
using iText.StyledXmlParser.Jsoup.Parser;
using MongoDB.Bson.IO;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace ImpartialUI.ViewModels
{
    public class RatingsViewModel : BaseViewModel
    {
        private HttpClient _client;
        private List<Competition> _competitions = new List<Competition>();

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

                GuessWsdcId();
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

        private ObservableCollection<Competitor> _competitors = new ObservableCollection<Competitor>();
        public ObservableCollection<Competitor> Competitors
        {
            get { return _competitors; }
            set
            {
                _competitors = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<Competitor> LeadCompetitors => Util.RemoveWhere(_competitors, c => c.LeadStats.Rating == 1000).OrderByDescending(c => c.LeadStats.Rating);
        public IEnumerable<Competitor> FollowCompetitors => Util.RemoveWhere(_competitors, c => c.FollowStats.Rating == 1000).OrderByDescending(c => c.FollowStats.Rating);

        public ICommand AddCompetitorCommand { get; set; }
        public ICommand RefreshCompetitorsCommand { get; set; }
        public ICommand ResetRatingsCommand { get; set; }
        public ICommand CrunchRatingsCommand { get; set; }

        private IDatabaseProvider _databaseProvider;

        public RatingsViewModel()
        {
            _databaseProvider = App.DatabaseProvider;
            _client = new HttpClient();
            _client.BaseAddress = new Uri("https://points.worldsdc.com/");

            AddCompetitorCommand = new DelegateCommand(AddCompetitor);
            RefreshCompetitorsCommand = new DelegateCommand(RefreshCompetitors);
            ResetRatingsCommand = new DelegateCommand(ResetRatings);
            CrunchRatingsCommand = new DelegateCommand(CrunchRatings);

            Initialize();
            CrunchRatings();
        }

        private async void Initialize()
        {
            //_competitions = (await _databaseProvider.GetAllCompetitionsAsync()).ToList();
            //Competitors = new ObservableCollection<Competitor>(await _databaseProvider.GetAllCompetitorsAsync());
            
            //_competitions = _competitions.OrderBy(c => c.Date).ToList();
        }

        private async void CrunchRatings()
        {
            IEnumerable<Competition> competitions = await _databaseProvider.GetAllCompetitionsAsync();
            competitions = competitions.OrderBy(c => c.Date).ToList();

            List<Competitor> competitors = (await _databaseProvider.GetAllCompetitorsAsync()).ToList();

            foreach (Competition competition in competitions)
            {
                var leads = competition.FinalLeaders.ToList();
                foreach (var lead in leads)
                {
                    lead.LeadStats.Rating = competitors.Where(c => c.Id == lead.Id).FirstOrDefault().LeadStats.Rating;
                }
                var follows = competition.FinalFollowers.ToList();
                foreach (var follow in follows)
                {
                    follow.FollowStats.Rating = competitors.Where(c => c.Id == follow.Id).FirstOrDefault().FollowStats.Rating;
                }

                competition.UpdateRatings(leads, follows);
                EloRatingService.CalculateRatings(competition.Couples);

                leads = competition.FinalLeaders.ToList();
                foreach (var lead in leads)
                {
                    competitors.Where(c => c.Id == lead.Id).FirstOrDefault().LeadStats.Rating = lead.LeadStats.Rating;
                }
                follows = competition.FinalFollowers.ToList();
                foreach (var follow in follows)
                {
                    competitors.Where(c => c.Id == follow.Id).FirstOrDefault().FollowStats.Rating = follow.FollowStats.Rating;
                }
            }

            Competitors = new ObservableCollection<Competitor>(competitors);
            OnPropertyChanged(nameof(LeadCompetitors));
            OnPropertyChanged(nameof(FollowCompetitors));
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

            Competitors = new ObservableCollection<Competitor>(await _databaseProvider.GetAllCompetitorsAsync());
        }

        private async void RefreshCompetitors()
        {
            Competitors = new ObservableCollection<Competitor>(await _databaseProvider.GetAllCompetitorsAsync());
        }

        private async void ResetRatings()
        {
            foreach (Competitor competitor in Competitors)
            {
                competitor.LeadStats.Rating = 1000;
                competitor.FollowStats.Rating = 1000;
            }
        }

        private async void GuessWsdcId()
        {
            var response = await _client.PostAsync("/lookup/find?q=" + FirstName + "%20" + LastName, null);
            string sheet = await response.Content.ReadAsStringAsync();
            string idString = Regex.Match(sheet.Substring(sheet.IndexOf("wscid"), 20), @"\d+").Value;

            if (Int32.TryParse(idString, out int id))
            {
                WsdcId = id.ToString();
            }
        }
    }
}
