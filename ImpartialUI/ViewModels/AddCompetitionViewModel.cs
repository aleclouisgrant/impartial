using Impartial;
using ImpartialUI.Commands;
using ImpartialUI.Enums;
using ImpartialUI.Models;
using ImpartialUI.Services.ScoresheetParser;
using Microsoft.Win32;
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
            AddCompetitionCommand = new DelegateCommand(AddCompetition);
            AddDanceConventionCommand = new DelegateCommand(AddDanceConvention);
            SelectPrelimsPathCommand = new DelegateCommand(SelectPrelimsPath);
            SelectSemisPathCommand = new DelegateCommand(SelectSemisPath);
            SelectFinalsPathCommand = new DelegateCommand(SelectFinalsPath);
            ParseScoreSheetsCommand = new DelegateCommand(ParseScoreSheets);
            CancelCommand = new DelegateCommand(Clear);
            RefreshCacheCommand = new DelegateCommand(RefreshCache);

            //_client = new HttpClient();
            //_client.BaseAddress = new Uri("https://points.worldsdc.com/");

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
            //var newDanceConvention = new DanceConvention("MADJam", DateTime.Parse("2023-03-04"));
            //DanceConventions.Add(newDanceConvention);
            //SelectedDanceConvention = newDanceConvention;
            //ScoresheetSelector = ScoresheetSelector.EEPro;
            //PrelimsPath = @"C:\Users\Alec\source\Impartial\ImpartialUI\Scoresheets\2023-03-04 madjam\prelims.html";
            //FinalsPath = @"C:\Users\Alec\source\Impartial\ImpartialUI\Scoresheets\2023-03-04 madjam\finals.html";

            // StepRightSolutions
            //var newDanceConvention = new DanceConvention("Boogie By The Bay", DateTime.Parse("2022-10-08"));
            //DanceConventions.Add(newDanceConvention);
            //SelectedDanceConvention = newDanceConvention;
            //ScoresheetSelector = ScoresheetSelector.StepRightSolutions;
            //PrelimsPath = @"C:\Users\Alec\source\Impartial\ImpartialUI\Scoresheets\2022-10-08 boogie by the bay\prelims.html";
            //SemisPath = @"C:\Users\Alec\source\Impartial\ImpartialUI\Scoresheets\2022-10-08 boogie by the bay\semis.html";
            //FinalsPath = @"C:\Users\Alec\source\Impartial\ImpartialUI\Scoresheets\2022-10-08 boogie by the bay\finals.html";

            // WorldDanceRegistry
            var newDanceConvention = new DanceConvention("Charlotte Westie Fest", DateTime.Parse("2023-02-03"));
            DanceConventions.Add(newDanceConvention);
            SelectedDanceConvention = newDanceConvention;
            ScoresheetSelector = ScoresheetSelector.WorldDanceRegistry;
            PrelimsPath = @"C:\Users\Alec\source\Impartial\ImpartialUI\Scoresheets\2023-02-03 charlotte\prelims.html";
            FinalsPath = @"C:\Users\Alec\source\Impartial\ImpartialUI\Scoresheets\2023-02-03 charlotte\finals.html";
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
            if (int.TryParse(WsdcId, out int id)) {
                var newCompetitor = new Competitor(FirstName, LastName, int.Parse(WsdcId));

                await App.DatabaseProvider.UpsertCompetitorAsync(newCompetitor);
                Competitors.Add(newCompetitor);
                Competitors = Competitors.OrderBy(c => c.FullName).ToList();

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
            Judges = Judges.OrderBy(j => j.FullName).ToList();

            JudgeFirstName = string.Empty;
            JudgeLastName = string.Empty;
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
                ScoresheetSelector = SelectScoresheetParser(finalsPath);
            }

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

            Competition = comp;
        }
        private void AddCompetition()
        {
            Trace.WriteLine(Competition.ToLongString());
            App.DatabaseProvider.UpsertCompetitionAsync(Competition, SelectedDanceConvention.Id);
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
    }
}
