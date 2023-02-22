using Impartial;
using ImpartialUI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

        private double _crunchProgressPercentage;
        public double CrunchProgressPercentage
        {
            get { return _crunchProgressPercentage; }
            set
            {
                _crunchProgressPercentage = value;
                OnPropertyChanged();
            }
        }

        private IProgress<double> _crunchProgress;

        private bool _crunchEnabled;
        public bool CrunchEnabled
        {
            get { return _crunchEnabled; }
            set
            {
                _crunchEnabled = value;
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
                OnPropertyChanged(nameof(LeadCompetitors));
                OnPropertyChanged(nameof(FollowCompetitors));
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
        }

        public RatingsViewModel(List<Competitor> competitors, List<Competition> competitions)
        {
            _databaseProvider = App.DatabaseProvider;
            _client = new HttpClient();
            _client.BaseAddress = new Uri("https://points.worldsdc.com/");

            AddCompetitorCommand = new DelegateCommand(AddCompetitor);
            RefreshCompetitorsCommand = new DelegateCommand(RefreshCompetitors);
            ResetRatingsCommand = new DelegateCommand(ResetRatings);
            CrunchRatingsCommand = new DelegateCommand(new Action(async () => 
            {
                await Task.Run(CrunchRatings);
            }));

            Competitors = new ObservableCollection<Competitor>(competitors);
            _competitions = competitions;

            _crunchProgress = new Progress<double>(ReportProgress);
            CrunchEnabled = true;

            Initialize();
        }

        private async void Initialize()
        {
            _competitions = (await _databaseProvider.GetAllCompetitionsAsync()).OrderBy(c => c.Date).ToList();
            Competitors = new ObservableCollection<Competitor>(await _databaseProvider.GetAllCompetitorsAsync());
        }

        private void ReportProgress(double progress)
        {
            Trace.WriteLine("progress: " + progress);
            CrunchProgressPercentage = progress;
        }

        private void CrunchRatings()
        {
            CrunchEnabled = false;
            ResetRatings();
            _crunchProgress.Report(0);
            double totalProgress = _competitions.Count;
            int count = 1;

            foreach (var competition in _competitions)
            {
                // prelim rounds
                foreach (int round in competition.Rounds)
                {
                    if (competition.LeaderPrelimScores.Count > 0 && competition.FollowerPrelimScores.Count > 0)
                    {
                        // update the ratings of all competitors in the scores
                        foreach (var score in competition.LeaderPrelimScores.Where(s => s.Round == round))
                        {
                            score.Competitor = Competitors.Where(c => c.Id == score.Competitor.Id).FirstOrDefault();
                        }
                        foreach (var score in competition.FollowerPrelimScores.Where(s => s.Round == round))
                        {
                            score.Competitor = Competitors.Where(c => c.Id == score.Competitor.Id).FirstOrDefault();
                        }

                        // calculating the new ratings
                        EloRatingService.PrelimRatings(competition.LeaderPrelimScores.Where(s => s.Round == round).ToList(), Role.Leader, competition.PrelimLeaderJudges(round));
                        EloRatingService.PrelimRatings(competition.FollowerPrelimScores.Where(s => s.Round == round).ToList(), Role.Follower, competition.PrelimFollowerJudges(round));
                    }
                }

                // finals
                if (competition.Scores.Count > 0)
                {
                    // update the ratings of all competitors in the scores
                    foreach (var score in competition.Scores)
                    {
                        score.Leader = Competitors.Where(c => c.Id == score.Leader.Id).FirstOrDefault();
                        score.Follower = Competitors.Where(c => c.Id == score.Follower.Id).FirstOrDefault();
                    }

                    // calculate the new ratings
                    EloRatingService.CalculateFinalsRating(competition.Couples);
                }

                _crunchProgress.Report(Math.Round((double)count / totalProgress * 100));
                count++;
            }

            OnPropertyChanged(nameof(Competitors));
            OnPropertyChanged(nameof(LeadCompetitors));
            OnPropertyChanged(nameof(FollowCompetitors));
            _crunchProgress.Report(0);
            CrunchEnabled = true;
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
