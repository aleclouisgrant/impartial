﻿using Impartial;
using ImpartialUI.Commands;
using ImpartialUI.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ImpartialUI.ViewModels
{
    public class RatingsViewModel : BaseViewModel
    {
        private List<CompetitorDataModel> _compDm;
        public List<CompetitorDataModel> CompDm
        {
            get { return _compDm; }
            set 
            { 
                _compDm = value;
                OnPropertyChanged();
            }
        }

        private double _plotProgressPercentage;
        public double PlotProgressPercentage
        {
            get { return _plotProgressPercentage; }
            set
            {
                _plotProgressPercentage = value;
                OnPropertyChanged();
            }
        }

        private IProgress<double> _plotProgress;
        private bool _plotEnabled;
        public bool PlotEnabled
        {
            get { return _plotEnabled; }
            set
            {
                _plotEnabled = value;
                OnPropertyChanged();
            }
        }

        private HttpClient _client;
        private List<ICompetition> _competitions = new List<ICompetition>();

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

        private ObservableCollection<ICompetitor> _competitors = new ObservableCollection<ICompetitor>();
        public ObservableCollection<ICompetitor> Competitors
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

        private CompetitorDataModel _selectedCompetitor;
        public CompetitorDataModel SelectedCompetitor
        {
            get { return _selectedCompetitor; }
            set
            {
                if (_selectedCompetitor != value)
                {
                    _selectedCompetitor = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedCompetitor.Competitor));
                }
            }
        }

        public IEnumerable<CompetitorDataModel> LeadCompetitors
        {
            get
            {
                if (_compDm?.FirstOrDefault().Competitor == null)
                    return Enumerable.Empty<CompetitorDataModel>();
                
                return Util.RemoveWhere(_compDm, c => c.Competitor.LeadStats.Rating == 1000).OrderByDescending(c => c.Competitor.LeadStats.Rating);
            }
        }
        public IEnumerable<CompetitorDataModel> FollowCompetitors {
            get
            {
                if (_compDm?.FirstOrDefault().Competitor == null)
                    return Enumerable.Empty<CompetitorDataModel>();

                return Util.RemoveWhere(_compDm, c => c.Competitor.FollowStats.Rating == 1000).OrderByDescending(c => c.Competitor.FollowStats.Rating);
            }
        }
        
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
            CrunchRatingsCommand = new DelegateCommand(new Action(async () =>
            {
                await Task.Run(CrunchRatings);
            }));

            Competitors = new ObservableCollection<ICompetitor>(App.CompetitorsDb);
            //_competitions = competitions;

            _crunchProgress = new Progress<double>(ReportProgress);
            CrunchEnabled = true;

            _plotProgress = new Progress<double>(ReportPlotProgress);
            PlotEnabled = false;

            Initialize();
        }

        private async void Initialize()
        {
            _competitions = (await _databaseProvider.GetAllCompetitionsAsync()).OrderBy(c => c.Date).ToList();
            Competitors = new ObservableCollection<ICompetitor>(await _databaseProvider.GetAllCompetitorsAsync());
        }

        private void ReportProgress(double progress)
        {
            CrunchProgressPercentage = progress;
        }

        private void ReportPlotProgress(double progress)
        {
            PlotProgressPercentage = progress;
            if (progress == 100)
            {
                PlotEnabled = true;
            }
        }

        private async void CrunchRatings()
        {
            //TODO:
            //CrunchEnabled = false;
            //ResetRatings();
            //_crunchProgress.Report(0);
            //double totalProgress = _competitions.Count;
            //int count = 1;

            //CompDm = new();

            //foreach (var competition in _competitions)
            //{
            //    // prelim rounds
            //    foreach (var pairedPrelimCompetition in competition.PairedPrelimCompetitions)
            //    {
            //            // update the ratings of all competitors in the scores
            //            foreach (var score in pairedPrelimCompetition.LeaderPrelimCompetition.PrelimScores)
            //            {
            //                var competitor = Competitors.Where(c => c.Id == score.Competitor.Id).FirstOrDefault();
            //                score.Competitor competitor;

            //                var compDm = CompDm.Where(c => c.CompetitorId == competitor.Id).FirstOrDefault();
            //                if (compDm == null)
            //                {
            //                    compDm = new CompetitorDataModel(competitor);
            //                    CompDm.Add(compDm);
            //                }

            //                if (!compDm.CompetitionHistory.Exists(c => c.CompetitionName == competition.Name && c.CompetitionDate == competition.Date && c.Round == score.Round))
            //                    compDm.CompetitionHistory.Add(new CompetitionHistory(competition.Name, competition.Date, competitor.LeadStats.Rating, 0, score.Round, score.RawScore, competition.PrelimLeaders(round).Count));
            //            }
            //            foreach (var score in pairedPrelimCompetition.FollowerPrelimCompetition.PrelimScores.Where(s => s.Round == round))
            //            {
            //                var competitor = Competitors.Where(c => c.Id == score.Competitor.Id).FirstOrDefault();
            //                score.Competitor = competitor;

            //                var compDm = CompDm.Where(c => c.CompetitorId == competitor.Id).FirstOrDefault();
            //                if (compDm == null)
            //                {
            //                    compDm = new CompetitorDataModel(competitor);
            //                    CompDm.Add(compDm);
            //                }

            //                if (!compDm.CompetitionHistory.Exists(c => c.CompetitionName == competition.Name && c.CompetitionDate == competition.Date && c.Round == score.Round))
            //                    compDm.CompetitionHistory.Add(new CompetitionHistory(competition.Name, competition.Date, competitor.FollowStats.Rating, 0, score.Round, score.RawScore, competition.PrelimFollowers(round).Count));
            //            }

            //            // calculating the new ratings
            //            var leaders = EloRatingService.PrelimRatings(pairedPrelimCompetition.LeaderPrelimCompetition.PrelimScores.Where(s => s.Round == round).ToList(), Role.Leader, competition.PrelimLeaderJudges(round));

            //            foreach (var leader in leaders)
            //            {
            //                var compDmLeader = CompDm.Where(c => c.CompetitorId == leader.Id)?.FirstOrDefault();
            //                if (compDmLeader != null)
            //                {
            //                    compDmLeader.CompetitionHistory.Where(h => h.CompetitionName == competition.Name && h.CompetitionDate == competition.Date && h.Round == round).FirstOrDefault()
            //                    .RatingAfter = leader.LeadStats.Rating;
            //                }
            //                else
            //                {
            //                    compDmLeader = new CompetitorDataModel(leader.Id);
            //                }
            //            }

            //            var followers = EloRatingService.PrelimRatings(pairedPrelimCompetition.FollowerPrelimCompetition.PrelimScores.Where(s => s.Round == round).ToList(), Role.Follower, competition.PrelimFollowerJudges(round));

            //            foreach (var follower in followers)
            //            {
            //                var compDmFollower = CompDm.Where(c => c.CompetitorId == follower.Id)?.FirstOrDefault();
            //                if (compDmFollower != null)
            //                {
            //                    compDmFollower.CompetitionHistory.Where(h => h.CompetitionName == competition.Name && h.CompetitionDate == competition.Date && h.Round == round).FirstOrDefault()
            //                    .RatingAfter = follower.FollowStats.Rating;
            //                }
            //            }
            //    }

            //    // finals
            //    if (competition.FinalCompetition.FinalScores.Count > 0)
            //    {
            //        // update the ratings of all competitors in the scores
            //        foreach (var score in competition.FinalCompetition.FinalScores)
            //        {
            //            score.Leader = Competitors.Where(c => c.Id == score.Leader.Id).FirstOrDefault();
            //            score.Follower = Competitors.Where(c => c.Id == score.Follower.Id).FirstOrDefault();
            //        }

            //        foreach (var couple in competition.FinalCompetition.Couples)
            //        {
            //            var leader = Competitors.Where(c => c.Id == couple.Leader.Id).FirstOrDefault();
            //            var compDmLeader = CompDm.Where(c => c.CompetitorId == leader.Id)?.FirstOrDefault();
            //            if (compDmLeader == null)
            //            {
            //                compDmLeader = new CompetitorDataModel(leader);
            //                CompDm.Add(compDmLeader);
            //            }
            //            compDmLeader.CompetitionHistory.Add(new CompetitionHistory(competition.Name, competition.Date, leader.LeadStats.Rating, 0, 0, couple.ActualPlacement, competition.FinalCompetition.Couples.Count));

            //            var follower = Competitors.Where(c => c.Id == couple.Follower.Id).FirstOrDefault();
            //            var compDmFollower = CompDm.Where(c => c.CompetitorId == follower.Id)?.FirstOrDefault();
            //            if (compDmFollower == null)
            //            {
            //                compDmFollower = new CompetitorDataModel(follower);
            //                CompDm.Add(compDmFollower);
            //            }
            //            compDmFollower.CompetitionHistory.Add(new CompetitionHistory(competition.Name, competition.Date, follower.FollowStats.Rating, 0, 0, couple.ActualPlacement, competition.FinalCompetition.Couples.Count));
            //        }

            //        // calculate the new ratings
            //        var couples = EloRatingService.CalculateFinalsRating(competition.FinalCompetition.Couples, !competition.HasRound(1));

            //        foreach (var couple in couples)
            //        {
            //            var compDmLeader = CompDm.Where(c => c.CompetitorId == couple.Leader.Id)?.FirstOrDefault();
            //            if (compDmLeader != null)
            //            {
            //                compDmLeader.CompetitionHistory.Where(h => h.CompetitionName == competition.Name && h.CompetitionDate == competition.Date && h.Round == 0).FirstOrDefault()
            //                .RatingAfter = couple.Leader.LeadStats.Rating;
            //            }

            //            var compDmFollower = CompDm.Where(c => c.CompetitorId == couple.Follower.Id)?.FirstOrDefault();
            //            if (compDmFollower != null)
            //            {
            //                compDmFollower.CompetitionHistory.Where(h => h.CompetitionName == competition.Name && h.CompetitionDate == competition.Date && h.Round == 0).FirstOrDefault()
            //                .RatingAfter = couple.Follower.FollowStats.Rating;
            //            }
            //        }
            //    }

            //    _crunchProgress.Report(Math.Round((double)count / totalProgress * 100));
            //    count++;
            //}

            //foreach (var compDm in CompDm)
            //{
            //    compDm.Competitor = Competitors.Where(c => c.Id == compDm.CompetitorId)?.FirstOrDefault();
            //}

            //CompDm = CompDm.OrderBy(c => c.Competitor.FullName).ToList();

            //OnPropertyChanged(nameof(CompDm));
            //OnPropertyChanged(nameof(Competitors));
            //OnPropertyChanged(nameof(LeadCompetitors));
            //OnPropertyChanged(nameof(FollowCompetitors));
            //_crunchProgress.Report(0);
            //CrunchEnabled = true;
            //PlotEnabled = true;
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

            Competitors = new ObservableCollection<ICompetitor>(await _databaseProvider.GetAllCompetitorsAsync());
        }

        private async void RefreshCompetitors()
        {
            Competitors = new ObservableCollection<ICompetitor>(await _databaseProvider.GetAllCompetitorsAsync());
        }

        private async void ResetRatings()
        {
            foreach (ICompetitor competitor in Competitors)
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

        private async Task<int> GetWsdcPoints(ICompetitor competitor)
        {
            var response = await _client.PostAsync("/lookup/find?q=" + competitor.WsdcId, null);
            string sheet = await response.Content.ReadAsStringAsync();

            var doc = JObject.Parse(sheet);

            try
            {
                return doc.SelectToken("placements.['West Coast Swing'].ALS.total_points").Value<int>();
            }
            catch
            {
                return 0;
            }
        }
    }
}
