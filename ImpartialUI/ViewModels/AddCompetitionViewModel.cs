using Impartial;
using ImpartialUI.Commands;
using ImpartialUI.Enums;
using ImpartialUI.Models;
using ImpartialUI.Services.ScoresheetParser;
using Microsoft.Win32;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ImpartialUI.ViewModels
{
    public class AddCompetitionViewModel : BaseViewModel
    {
        private IScoresheetParser _scoresheetParser;
        private HttpClient _client;

        private List<ICompetitor> _competitors = new List<ICompetitor>();
        public List<ICompetitor> Competitors
        {
            get { return _competitors; }
            set { 
                _competitors = value; 
                OnPropertyChanged(); 
            }
        }
        
        private List<IJudge> _judges = new List<IJudge>();
        public List<IJudge> Judges
        {
            get { return _judges; }
            set
            {
                _judges = value;
                OnPropertyChanged();
            }
        }

        private string prelimsPath;
        public string PrelimsPath
        {
            get { return prelimsPath; }
            set
            {
                prelimsPath = value;
                OnPropertyChanged();
            }
        }

        private string semisPath;
        public string SemisPath
        {
            get { return semisPath; }
            set
            {
                semisPath = value;
                OnPropertyChanged();
            }
        }

        private string finalsPath;
        public string FinalsPath
        {
            get { return finalsPath; }
            set
            {
                finalsPath = value;
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

        private string _judgeFirstName;
        public string JudgeFirstName
        {
            get { return _judgeFirstName; }
            set
            {
                _judgeFirstName = value;
                OnPropertyChanged();
            }
        }
        private string _judgeLastName;
        public string JudgeLastName
        {
            get { return _judgeLastName; }
            set
            {
                _judgeLastName = value;
                OnPropertyChanged();
            }
        }

        private ScoresheetSelector _scoresheetSelector;
        public ScoresheetSelector ScoresheetSelector
        {
            get { return _scoresheetSelector; }
            set
            {
                _scoresheetSelector = value;
                OnPropertyChanged();
            }
        }

        private ICompetition _competition;
        public ICompetition Competition
        {
            get { return _competition; }
            set
            {
                _competition = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LeaderPrelims));
                OnPropertyChanged(nameof(FollowerPrelims));
                OnPropertyChanged(nameof(LeaderSemis));
                OnPropertyChanged(nameof(FollowerSemis));
                OnPropertyChanged(nameof(FinalCompetition));
            }
        }

        private IDanceConvention _danceConvention;
        public IDanceConvention DanceConvention
        {
            get { return _danceConvention; }
            set
            {
                _danceConvention = value;
                OnPropertyChanged();
            }
        }

        public IPrelimCompetition LeaderPrelims => Competition?.PairedPrelimCompetitions.Where(c => c.Round == Round.Prelims).FirstOrDefault()?.LeaderPrelimCompetition;
        public IPrelimCompetition FollowerPrelims => Competition?.PairedPrelimCompetitions.Where(c => c.Round == Round.Prelims).FirstOrDefault()?.FollowerPrelimCompetition;

        public IPrelimCompetition LeaderSemis => Competition?.PairedPrelimCompetitions.Where(c => c.Round == Round.Semifinals).FirstOrDefault()?.LeaderPrelimCompetition;
        public IPrelimCompetition FollowerSemis => Competition?.PairedPrelimCompetitions.Where(c => c.Round == Round.Semifinals).FirstOrDefault()?.FollowerPrelimCompetition;

        public IFinalCompetition FinalCompetition => Competition?.FinalCompetition;


        private double _parseProgressPercentage;
        public double ParseProgressPercentage
        {
            get { return _parseProgressPercentage; }
            set
            {
                _parseProgressPercentage = value;
                OnPropertyChanged();
            }
        }

        private IProgress<double> _parseProgress;

        public ICommand AddCompetitorCommand { get; set; }
        public ICommand AddJudgeCommand { get; set; }
        public ICommand AddCompetitionCommand { get; set; }
        public ICommand ParseScoreSheetsCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand SelectPrelimsPathCommand { get; set; }
        public ICommand SelectSemisPathCommand { get; set; }
        public ICommand SelectFinalsPathCommand { get; set; }
        public ICommand RefreshCacheCommand { get; set; }

        public AddCompetitionViewModel()
        {
            AddCompetitorCommand = new DelegateCommand(AddCompetitor);
            AddJudgeCommand = new DelegateCommand(AddJudge);
            AddCompetitionCommand = new DelegateCommand(AddCompetition);
            SelectPrelimsPathCommand = new DelegateCommand(SelectPrelimsPath);
            SelectFinalsPathCommand = new DelegateCommand(SelectFinalsPath);
            ParseScoreSheetsCommand = new DelegateCommand(ParseScoreSheets);
            CancelCommand = new DelegateCommand(Clear);

            _client = new HttpClient();
            _client.BaseAddress = new Uri("https://points.worldsdc.com/");

            Competition = new Competition()
            { 
                Division = Division.AllStar 
            };
            ScoresheetSelector = ScoresheetSelector.Auto;

            _parseProgress = new Progress<double>(ReportProgress);
        }
        public AddCompetitionViewModel(List<ICompetitor> competitors, List<IJudge> judges)
        {
            AddCompetitorCommand = new DelegateCommand(AddCompetitor);
            AddJudgeCommand = new DelegateCommand(AddJudge);
            AddCompetitionCommand = new DelegateCommand(AddCompetition);
            SelectPrelimsPathCommand = new DelegateCommand(SelectPrelimsPath);
            SelectSemisPathCommand = new DelegateCommand(SelectSemisPath);
            SelectFinalsPathCommand = new DelegateCommand(SelectFinalsPath);
            ParseScoreSheetsCommand = new DelegateCommand(ParseScoreSheets);
            RefreshCacheCommand = new DelegateCommand(RefreshCache);
            CancelCommand = new DelegateCommand(Clear);

            _client = new HttpClient();
            _client.BaseAddress = new Uri("https://points.worldsdc.com/");

            Competition = new Competition()
            {
                Division = Division.AllStar
            };
            ScoresheetSelector = ScoresheetSelector.Auto;

            _parseProgress = new Progress<double>(ReportProgress);

            Competitors = competitors;
            Judges = judges;

            //ScoresheetSelector = ScoresheetSelector.StepRightSolutions;
            //PrelimsPath = @"C:\Users\Alec\source\Impartial\ImpartialUI\Scoresheets\2023-04-01 city of angels\prelims.html";
            //SemisPath = @"C:\Users\Alec\source\Impartial\ImpartialUI\Scoresheets\2022-10-08 boogie by the bay\semis.html";
            //FinalsPath = @"C:\Users\Alec\source\Impartial\ImpartialUI\Scoresheets\2023-04-01 city of angels\finals.html";
            //ParseScoreSheets();
        }

        private async void RefreshCache()
        {
            //Competitors = (await App.DatabaseProvider.GetAllCompetitorsAsync()).ToList();
            //Judges = (await App.DatabaseProvider.GetAllJudgesAsync()).ToList();
        }

        private void Clear()
        {
            Competition = new Competition() { Division = Division.AllStar };
            PrelimsPath = string.Empty;
            SemisPath = string.Empty;
            FinalsPath = string.Empty;
            ScoresheetSelector = ScoresheetSelector.Auto;

            ClearException();
        }

        private async void AddCompetitor()
        {
            if (int.TryParse(WsdcId, out int id)) {
                var newCompetitor = new Competitor(FirstName, LastName, int.Parse(WsdcId));

                await App.DatabaseProvider.UpsertCompetitorAsync(newCompetitor);
                Competitors.Add(newCompetitor);

                FirstName = string.Empty;
                LastName = string.Empty;
                WsdcId = string.Empty;
            }
        }
        private async void AddJudge()
        {
            var newJudge = new Judge(JudgeFirstName, JudgeLastName);

            await App.DatabaseProvider.UpsertJudgeAsync(newJudge);
            Judges.Add(newJudge);

            JudgeFirstName = string.Empty;
            JudgeLastName = string.Empty;
        }

        private void SelectPrelimsPath()
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Select Prelims Score Sheet",

            };
            openFileDialog.ShowDialog();

            PrelimsPath = openFileDialog.FileName;
        }
        private void SelectSemisPath()
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Select Semis Score Sheet",

            };
            openFileDialog.ShowDialog();

            SemisPath = openFileDialog.FileName;
        }
        private void SelectFinalsPath()
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Select Finals Score Sheet",

            };
            openFileDialog.ShowDialog();

            FinalsPath = openFileDialog.FileName;
        }

        private async void ParseScoreSheets()
        {
            ClearException();
            _parseProgress.Report(0);

            if (ScoresheetSelector == ScoresheetSelector.Auto)
            {
                ScoresheetSelector = SelectScoresheetParser(finalsPath);
            }

            switch (ScoresheetSelector)
            {
                case ScoresheetSelector.EEPro:
                    _scoresheetParser = new EEProParser(prelimsPath, finalsPath);
                    break;
                case ScoresheetSelector.DanceConvention:
                    _scoresheetParser = new DanceConventionParser(prelimsPath, finalsPath);
                    break;
                //TODO: readd this
                //case ScoresheetSelector.StepRightSolutions:
                //    _scoresheetParser = new StepRightSolutionsParser(prelimsPath, semisPath, finalsPath);
                //    break;
                case ScoresheetSelector.WorldDanceRegistry:
                    _scoresheetParser = new WorldDanceRegistryParser(prelimsPath, finalsPath);
                    break;
                case ScoresheetSelector.Other:
                default:
                    return;
            }

            ICompetition comp;
            try
            {
                comp = _scoresheetParser.GetCompetition(Division.AllStar);
            } catch (DivisionNotFoundException e)
            {
                Exception = e;
                return;
            }

            // prelims rounds
            foreach (var pairedPrelimCompetition in Competition.PairedPrelimCompetitions)
            {
                foreach (var competitor in pairedPrelimCompetition.LeaderPrelimCompetition.Competitors)
                {
                    var serverCompetitor = Util.FindCompetitorInCache(competitor.FirstName, competitor.LastName, Competitors);
                    if (serverCompetitor == null)
                    {
                        serverCompetitor = await App.DatabaseProvider.GetCompetitorAsync(competitor.FirstName, competitor.LastName);
                    }

                    if (serverCompetitor != null)
                    {
                        serverCompetitor = new Competitor(
                            id: serverCompetitor.Id, 
                            wsdcId: serverCompetitor.WsdcId,
                            firstName: serverCompetitor.FirstName, 
                            lastName: serverCompetitor.LastName,
                            leaderRating: serverCompetitor.LeadStats.Rating, 
                            leaderVariance: serverCompetitor.LeadStats.Variance,
                            followerRating: serverCompetitor.FollowStats.Rating, 
                            followerVariance: serverCompetitor.FollowStats.Variance);

                        var scores = pairedPrelimCompetition.LeaderPrelimCompetition.PrelimScores.Where(s => s.Competitor.FullName == competitor.FullName).ToList();
                        foreach (var score in scores)
                        {
                            score.SetCompetitor(serverCompetitor.Id);
                        }
                    }
                    else
                    {
                        int wsdcId = await GuessWsdcId(competitor.FirstName, competitor.LastName);

                        var scores = pairedPrelimCompetition.LeaderPrelimCompetition.PrelimScores.Where(s => s.Competitor.FullName == competitor.FullName).ToList();
                        foreach (var score in scores)
                        {
                            score.Competitor.WsdcId = wsdcId;
                        }
                    }
                }

                foreach (var competitor in pairedPrelimCompetition.FollowerPrelimCompetition.Competitors)
                {
                    var serverCompetitor = Util.FindCompetitorInCache(competitor.FirstName, competitor.LastName, Competitors);
                    if (serverCompetitor == null)
                    {
                        serverCompetitor = await App.DatabaseProvider.GetCompetitorAsync(competitor.FirstName, competitor.LastName);
                    }

                    if (serverCompetitor != null)
                    {
                        serverCompetitor = new Competitor(
                            id: serverCompetitor.Id,
                            wsdcId: serverCompetitor.WsdcId,
                            firstName: serverCompetitor.FirstName,
                            lastName: serverCompetitor.LastName,
                            leaderRating: serverCompetitor.LeadStats.Rating,
                            leaderVariance: serverCompetitor.LeadStats.Variance,
                            followerRating: serverCompetitor.FollowStats.Rating,
                            followerVariance: serverCompetitor.FollowStats.Variance);

                        var scores = pairedPrelimCompetition.FollowerPrelimCompetition.PrelimScores.Where(s => s.Competitor.FullName == competitor.FullName).ToList();
                        foreach (var score in scores)
                        {
                            score.SetCompetitor(serverCompetitor.Id);
                        }
                    }
                    else
                    {
                        int wsdcId = await GuessWsdcId(competitor.FirstName, competitor.LastName);

                        var scores = pairedPrelimCompetition.FollowerPrelimCompetition.PrelimScores.Where(s => s.Competitor.FullName == competitor.FullName).ToList();
                        foreach (var score in scores)
                        {
                            score.Competitor.WsdcId = wsdcId;
                        }
                    }
                }

                //foreach (var judge in comp.PrelimLeaderJudges(round))
                //{
                //    var serverJudge = Util.FindJudgeInCache(judge.FirstName, judge.LastName, Judges);
                //    if (serverJudge != null)
                //    {
                //        serverJudge = new Judge(serverJudge.Id, serverJudge.FirstName, serverJudge.LastName, serverJudge.Accuracy, serverJudge.Top5Accuracy);

                //        var scores = comp.LeaderPrelimScores.Where(s => s.Judge.FullName == judge.FullName && s.Round == round).ToList();
                //        foreach (var score in scores)
                //        {
                //            score.Judge = serverJudge;
                //        }
                //    }
                //}

                //foreach (var judge in comp.PrelimFollowerJudges(round))
                //{
                //    var serverJudge = Util.FindJudgeInCache(judge.FirstName, judge.LastName, Judges);
                //    if (serverJudge != null)
                //    {
                //        serverJudge = new Judge(serverJudge.Id, serverJudge.FirstName, serverJudge.LastName, serverJudge.Accuracy, serverJudge.Top5Accuracy);

                //        var scores = comp.FollowerPrelimScores.Where(s => s.Judge.FullName == judge.FullName && s.Round == round).ToList();
                //        foreach (var score in scores)
                //        {
                //            score.Judge = serverJudge;
                //        }
                //    }
                //}
            }

            // finals
            foreach (var judge in comp.FinalCompetition.Judges)
            {
                var serverJudge = Util.FindJudgeInCache(judge.FirstName, judge.LastName, Judges);
                if (serverJudge == null)
                {
                    serverJudge = await App.DatabaseProvider.GetJudgeAsync(judge.FirstName, judge.LastName);
                }

                if (serverJudge != null)
                {
                    serverJudge = new Judge(serverJudge.FirstName, serverJudge.LastName, serverJudge.Id)
                    {
                        Scores = judge.Scores
                    };

                    var scores = comp.FinalCompetition.FinalScores.Where(s => s.Judge.FullName == judge.FullName).ToList();
                    foreach (var score in scores)
                    {
                        score.Judge = serverJudge;
                    }
                }
            }

            foreach (var couple in comp.FinalCompetition.Couples)
            {
                var serverLeader = Util.FindCompetitorInCache(couple.Leader.FirstName, couple.Leader.LastName, Competitors);
                if (serverLeader == null)
                {
                    serverLeader = await App.DatabaseProvider.GetCompetitorAsync(couple.Leader.FirstName, couple.Leader.LastName);
                }

                if (serverLeader != null)
                {
                    serverLeader = new Competitor(
                        serverLeader.Id, 
                        serverLeader.WsdcId,
                        serverLeader.FirstName, 
                        serverLeader.LastName,
                        serverLeader.LeadStats.Rating, 
                        serverLeader.LeadStats.Variance,
                        serverLeader.FollowStats.Rating, 
                        serverLeader.FollowStats.Variance);

                    var scores = comp.FinalCompetition.FinalScores.Where(s => s.Leader.FullName == couple.Leader.FullName).ToList();
                    foreach (var score in scores)
                    {
                        score.Leader = serverLeader;
                    }
                }
                else
                {
                    int wsdcId = await GuessWsdcId(couple.Leader.FirstName, couple.Leader.LastName);

                    var scores = comp.FinalCompetition.FinalScores.Where(s => s.Leader.FullName == couple.Leader.FullName).ToList();
                    foreach (var score in scores)
                    {
                        score.Leader.WsdcId = wsdcId;
                    }
                }

                var serverFollower = Util.FindCompetitorInCache(couple.Follower.FirstName, couple.Follower.LastName, Competitors);
                if (serverFollower == null)
                {
                    serverFollower = await App.DatabaseProvider.GetCompetitorAsync(couple.Follower.FirstName, couple.Follower.LastName);
                }

                if (serverFollower != null)
                {
                    serverFollower = new Competitor(
                        serverFollower.Id, 
                        serverFollower.WsdcId,
                        serverFollower.FirstName, 
                        serverFollower.LastName,
                        serverFollower.LeadStats.Rating, 
                        serverFollower.LeadStats.Variance,
                        serverFollower.FollowStats.Rating, 
                        serverFollower.FollowStats.Variance);

                    var scores = comp.FinalCompetition.FinalScores.Where(s => s.Follower.FullName == couple.Follower.FullName).ToList();
                    foreach (var score in scores)
                    {
                        score.Follower = serverFollower;
                    }
                }
                else
                {
                    int wsdcId = await GuessWsdcId(couple.Follower.FirstName, couple.Follower.LastName);

                    var scores = comp.FinalCompetition.FinalScores.Where(s => s.Follower.FullName == couple.Follower.FullName).ToList();
                    foreach (var score in scores)
                    {
                        score.Follower.WsdcId = wsdcId;
                    }
                }
            }

            Competition = comp;

            _parseProgress.Report(0);
        }
        private void AddCompetition()
        {
            Trace.WriteLine(Competition.ToLongString());
            App.DatabaseProvider.UpsertCompetitionAsync(Competition, DanceConvention.Id);
            Clear();
        }

        private async Task<int> GuessWsdcId(string firstName, string lastName)
        {
            try
            {
                var response = await _client.PostAsync("/lookup/find?q=" + firstName + "%20" + lastName, null);
                string sheet = await response.Content.ReadAsStringAsync();
                string idString = Regex.Match(sheet.Substring(sheet.IndexOf("wscid"), 20), @"\d+").Value;
                return Int32.TryParse(idString, out int id) ? id : -1;
            } 
            catch
            {
                return -1;
            }
        }
        private void ReportProgress(double progress)
        {
            Trace.WriteLine("progress: " + progress);
            ParseProgressPercentage = progress;
        }

        public ScoresheetSelector SelectScoresheetParser(string finalsPath)
        {
            var finalsSheetDoc = File.ReadAllText(finalsPath).Replace("\n", "").Replace("\r", "");
            var parserString = Util.GetSubString(finalsSheetDoc, "<!-- saved from", " -->");

            if (parserString.Contains("worlddanceregistry"))
                return ScoresheetSelector.WorldDanceRegistry;
            else if (parserString.Contains("steprightsolutions"))
                return ScoresheetSelector.StepRightSolutions;
            else if (parserString.Contains("eepro"))
                return ScoresheetSelector.EEPro;
            else
                return ScoresheetSelector.Other;
        }
        
    }
}
