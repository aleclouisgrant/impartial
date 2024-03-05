using Impartial;
using ImpartialUI.Commands;
using ImpartialUI.Enums;
using ImpartialUI.Models;
using ImpartialUI.Services.ScoresheetParser;
using Microsoft.Win32;
using Npgsql;
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

        private List<ICompetitor> _competitors = new();
        public List<ICompetitor> Competitors
        {
            get { return _competitors; }
            set { 
                _competitors = value; 
                OnPropertyChanged(); 
            }
        }
        
        private List<IJudge> _judges = new();
        public List<IJudge> Judges
        {
            get { return _judges; }
            set
            {
                _judges = value;
                OnPropertyChanged();
            }
        }

        private List<IDanceConvention> _danceConventions = new();
        public List<IDanceConvention> DanceConventions
        {
            get { return _danceConventions; }
            set
            {
                _danceConventions = value;
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

        private string _newDanceConventionName;
        public string NewDanceConventionName
        {
            get { return _newDanceConventionName; }
            set
            {
                _newDanceConventionName = value;
                OnPropertyChanged();
            }
        }

        private DateTime _newDanceConventionDate;
        public DateTime NewDanceConventionDate
        {
            get { return _newDanceConventionDate; }
            set
            {
                _newDanceConventionDate = value;
                OnPropertyChanged();
            }
        }

        public ICompetitor _selectedCompetitor;
        public ICompetitor SelectedCompetitor
        {
            get { return _selectedCompetitor; }
            set
            {
                _selectedCompetitor = value;
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

        private IDanceConvention _selectedDanceConvention;
        public IDanceConvention SelectedDanceConvention
        {
            get { return _selectedDanceConvention; }
            set
            {
                _selectedDanceConvention = value;
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
                OnPropertyChanged(nameof(LeaderQuarters));
                OnPropertyChanged(nameof(FollowerQuarters));
                OnPropertyChanged(nameof(LeaderSemis));
                OnPropertyChanged(nameof(FollowerSemis));
                OnPropertyChanged(nameof(FinalCompetition));

                ShowFinals = Competition?.FinalCompetition != null;
                ShowPrelims = Competition?.PairedPrelimCompetitions.Where(c => c.Round == Round.Prelims).FirstOrDefault() != null;
                ShowQuarters = Competition?.PairedPrelimCompetitions.Where(c => c.Round == Round.Quarterfinals).FirstOrDefault() != null;
                ShowSemis = Competition?.PairedPrelimCompetitions.Where(c => c.Round == Round.Semifinals).FirstOrDefault() != null;

                ShowCompetition = ShowFinals || ShowPrelims || ShowQuarters || ShowSemis;

                OnPropertyChanged(nameof(ShowPrelims));
                OnPropertyChanged(nameof(ShowQuarters));
                OnPropertyChanged(nameof(ShowSemis));
                OnPropertyChanged(nameof(ShowFinals));
                OnPropertyChanged(nameof(ShowCompetition));
            }
        }

        public IPrelimCompetition LeaderPrelims => Competition?.PairedPrelimCompetitions.Where(c => c.Round == Round.Prelims).FirstOrDefault()?.LeaderPrelimCompetition;
        public IPrelimCompetition FollowerPrelims => Competition?.PairedPrelimCompetitions.Where(c => c.Round == Round.Prelims).FirstOrDefault()?.FollowerPrelimCompetition;

        public IPrelimCompetition LeaderQuarters => Competition?.PairedPrelimCompetitions.Where(c => c.Round == Round.Quarterfinals).FirstOrDefault()?.LeaderPrelimCompetition;
        public IPrelimCompetition FollowerQuarters => Competition?.PairedPrelimCompetitions.Where(c => c.Round == Round.Quarterfinals).FirstOrDefault()?.FollowerPrelimCompetition;

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

        public bool ShowPrelims { get; set; }
        public bool ShowQuarters { get; set; }
        public bool ShowSemis { get; set; }
        public bool ShowFinals { get; set; }
        public bool ShowCompetition { get; set; }

        public ICommand AddCompetitorCommand { get; set; }
        public ICommand AddJudgeCommand { get; set; }
        public ICommand AddJudgeProfileCommand { get; set; }
        public ICommand AddDanceConventionCommand { get; set; }
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
            AddJudgeProfileCommand = new DelegateCommand(AddJudgeProfile);
            AddCompetitionCommand = new DelegateCommand(AddCompetition);
            AddDanceConventionCommand = new DelegateCommand(AddDanceConvention);
            SelectPrelimsPathCommand = new DelegateCommand(SelectPrelimsPath);
            SelectSemisPathCommand = new DelegateCommand(SelectSemisPath);
            SelectFinalsPathCommand = new DelegateCommand(SelectFinalsPath);
            ParseScoreSheetsCommand = new DelegateCommand(ParseScoreSheets);
            CancelCommand = new DelegateCommand(Clear);
            RefreshCacheCommand = new DelegateCommand(RefreshCache);

            _client = new HttpClient();
            _client.BaseAddress = new Uri("https://points.worldsdc.com/");

            Competition = new Competition()
            { 
                Division = Division.AllStar 
            };
            ScoresheetSelector = ScoresheetSelector.Auto;
            NewDanceConventionDate = DateTime.Now;

            _parseProgress = new Progress<double>(ReportProgress);

            RefreshCache();

            TestData();
        }

        private void TestData()
        {
            // EEPro
            //ScoresheetSelector = ScoresheetSelector.EEPro;
            //PrelimsPath = @"C:\Users\Alec\source\Impartial\ImpartialUI\Scoresheets\2023-03-04 madjam\prelims.html";
            //FinalsPath = @"C:\Users\Alec\source\Impartial\ImpartialUI\Scoresheets\2023-03-04 madjam\finals.html";

            // StepRightSolutions
            //ScoresheetSelector = ScoresheetSelector.StepRightSolutions;
            //PrelimsPath = @"C:\Users\Alec\source\Impartial\ImpartialUI\Scoresheets\2022-10-08 boogie by the bay\prelims.html";
            //SemisPath = @"C:\Users\Alec\source\Impartial\ImpartialUI\Scoresheets\2022-10-08 boogie by the bay\semis.html";
            //FinalsPath = @"C:\Users\Alec\source\Impartial\ImpartialUI\Scoresheets\2022-10-08 boogie by the bay\finals.html";

            // WorldDanceRegistry
            //ScoresheetSelector = ScoresheetSelector.WorldDanceRegistry;
            //PrelimsPath = @"C:\Users\Alec\source\Impartial\ImpartialUI\Scoresheets\2023-02-03 charlotte\prelims.html";
            //FinalsPath = @"C:\Users\Alec\source\Impartial\ImpartialUI\Scoresheets\2023-02-03 charlotte\finals.html";
        }

        private async void RefreshCache()
        {
            DanceConventions = (await App.DatabaseProvider.GetAllDanceConventionsAsync()).OrderBy(c => c.Date).ToList();
            //Competitors = (await App.DatabaseProvider.GetAllCompetitorsAsync()).OrderBy(c => c.FullName).ToList();
            //Judges = (await App.DatabaseProvider.GetAllJudgesAsync()).ToList().OrderBy(c => c.FullName).ToList();

            Competitors = App.CompetitorsDb;
            Judges = App.JudgesDb;
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
            if (int.TryParse(WsdcId, out int id))
            {
                var newCompetitor = new Competitor(FirstName, LastName, id);

                await App.DatabaseProvider.UpsertCompetitorAsync(newCompetitor);

                App.CompetitorsDb.Add(newCompetitor);
                App.CompetitorsDb = App.CompetitorsDb.OrderBy(c => c.FullName).ToList();
                Competitors = App.CompetitorsDb;

                FirstName = string.Empty;
                LastName = string.Empty;
                WsdcId = string.Empty;
            }
        }
        private void AddJudge()
        {
            AddJudgeToDb(new Judge(JudgeFirstName, JudgeLastName));
            JudgeFirstName = string.Empty;
            JudgeLastName = string.Empty;
        }
        private void AddJudgeProfile()
        {
            AddJudgeToDb(new Judge(SelectedCompetitor.UserId, Guid.NewGuid(), SelectedCompetitor.FirstName, SelectedCompetitor.LastName));
            SelectedCompetitor = null;
        }

        private async void AddJudgeToDb(IJudge newJudge)
        {
            await App.DatabaseProvider.UpsertJudgeAsync(newJudge);

            App.JudgesDb.Add(newJudge);
            App.JudgesDb = App.JudgesDb.OrderBy(j => j.FullName).ToList();
            Judges = App.JudgesDb;
        }

        private async void AddDanceConvention()
        {
            var newDanceConvention = new DanceConvention(NewDanceConventionName, NewDanceConventionDate);

            await App.DatabaseProvider.UpsertDanceConventionAsync(newDanceConvention);
            DanceConventions.Add(newDanceConvention);

            NewDanceConventionName = string.Empty;
            NewDanceConventionDate = DateTime.Now;
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

            if (ScoresheetSelector == ScoresheetSelector.Auto)
            {
                if (!(finalsPath == null || finalsPath == string.Empty))
                {
                    ScoresheetSelector = SelectScoresheetParser(finalsPath);
                }
            }

            try
            {
                switch (ScoresheetSelector)
                {
                    case ScoresheetSelector.EEPro:
                        _scoresheetParser = new EEProParser(prelimsPath: prelimsPath, finalsPath: finalsPath);
                        break;
                    case ScoresheetSelector.DanceConvention:
                        _scoresheetParser = new DanceConventionParser(prelimsPath: prelimsPath, finalsPath: finalsPath);
                        break;
                    case ScoresheetSelector.StepRightSolutions:
                        _scoresheetParser = new StepRightSolutionsParser(prelimsPath: prelimsPath, semisPath: semisPath, finalsPath: finalsPath);
                        break;
                    case ScoresheetSelector.WorldDanceRegistry:
                        _scoresheetParser = new WorldDanceRegistryParser(prelimsPath: prelimsPath, semisPath: semisPath, finalsPath: finalsPath);
                        break;
                    case ScoresheetSelector.Other:
                    default:
                        Exception = new Exception("Auto scoresheet selector could not determine which parser to use.");
                        return;
                }
            }
            catch (FileNotFoundException e)
            {
                Exception = e;
            }

            ICompetition comp;
            try
            {
                //TODO: make this async
                comp = _scoresheetParser.GetCompetition(Division.AllStar);
            } catch (DivisionNotFoundException e)
            {
                Exception = e;
                return;
            }

            await Match(comp);

            Competition = comp;
        }
        private async void AddCompetition()
        {
            // Set date times to match the selected dance convention for now
            Competition.Date = SelectedDanceConvention.Date;
            Competition.FinalCompetition.DateTime = SelectedDanceConvention.Date;
            foreach (var competition in Competition.PairedPrelimCompetitions)
            {
                competition.LeaderPrelimCompetition.DateTime = SelectedDanceConvention.Date;
                competition.FollowerPrelimCompetition.DateTime = SelectedDanceConvention.Date;
            }

            Trace.WriteLine(Competition.ToLongString());

            try
            {
                await App.DatabaseProvider.UpsertCompetitionAsync(Competition, SelectedDanceConvention.Id);
            } 
            catch (PostgresException e)
            {
                Clear();

                Exception = e;
                await App.DatabaseProvider.DeleteCompetitionAsync(Competition.Id);

                return;
            }

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

        private ScoresheetSelector SelectScoresheetParser(string path)
        {
            var sheet = File.ReadAllText(path).Replace("\n", "").Replace("\r", "");
            var parserString = Util.GetSubString(sheet, "<!-- saved from", " -->");

            if (parserString.Contains("worlddanceregistry"))
                return ScoresheetSelector.WorldDanceRegistry;
            else if (parserString.Contains("steprightsolutions"))
                return ScoresheetSelector.StepRightSolutions;
            else if (parserString.Contains("eepro"))
                return ScoresheetSelector.EEPro;
            else
                return ScoresheetSelector.Other;
        }

        /// <summary>
        /// Match competitors and judges to their DB counterpart
        /// </summary>
        /// <param name="competition"></param>
        private async Task Match(ICompetition competition)
        {
            foreach (var pairedPrelimCompetition in competition.PairedPrelimCompetitions)
            {
                foreach (var competitor in pairedPrelimCompetition.LeaderPrelimCompetition.Competitors)
                {
                    await MatchCompetitor(competitor);
                }
                foreach (var competitor in pairedPrelimCompetition.FollowerPrelimCompetition.Competitors)
                {
                    await MatchCompetitor(competitor);
                }

                foreach (var judge in pairedPrelimCompetition.LeaderPrelimCompetition.Judges)
                {
                    MatchJudge(judge);
                }
                foreach (var judge in pairedPrelimCompetition.FollowerPrelimCompetition.Judges)
                {
                    MatchJudge(judge);
                }
            }
            foreach (var couple in competition.FinalCompetition.Couples)
            {
                await MatchCompetitor(couple.Leader);
                await MatchCompetitor(couple.Follower);
            }
            foreach (var judge in competition.FinalCompetition.Judges)
            {
                MatchJudge(judge);
            }
        }
        private async Task MatchCompetitor(ICompetitor competitor)
        {
            var dbCompetitor = Util.FindCompetitorInCache(competitor.FirstName, competitor.LastName, App.CompetitorsDb);
            if (dbCompetitor != null)
            {
                competitor.UserId = dbCompetitor.UserId;
                competitor.CompetitorId = dbCompetitor.CompetitorId;
                competitor.WsdcId = dbCompetitor.WsdcId;
                competitor.FirstName = dbCompetitor.FirstName;
                competitor.LastName = dbCompetitor.LastName;
                competitor.LeadStats = dbCompetitor.LeadStats;
                competitor.FollowStats = dbCompetitor.FollowStats;
            }
            else
            {
                competitor.WsdcId = await GuessWsdcId(competitor.FirstName, competitor.LastName);
            }
        }
        private void MatchJudge(IJudge judge)
        {
            var dbJudge = Util.FindJudgeInCache(judge.FirstName, judge.LastName, App.JudgesDb);
            if (dbJudge != null)
            {
                judge.UserId = dbJudge.UserId;
                judge.JudgeId = dbJudge.JudgeId;
                judge.FirstName = dbJudge.FirstName;
                judge.LastName = dbJudge.LastName;
            }
        }
    }
}
