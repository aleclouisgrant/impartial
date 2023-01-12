using HtmlAgilityPack;
using Impartial;
using Impartial.Services.ScoresheetParser;
using ImpartialUI.Commands;
using ImpartialUI.Enums;
using Microsoft.Win32;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ImpartialUI.ViewModels
{
    public class AddCompetitionViewModel : BaseViewModel
    {
        private IDatabaseProvider _databaseProvider;
        private IScoresheetParser _scoresheetParser;
        private HttpClient _client;

        private List<Competitor> _competitors = new List<Competitor>();
        public List<Competitor> Competitors
        {
            get { return _competitors; }
            set { 
                _competitors = value; 
                OnPropertyChanged(); 
            }
        }

        private string prelimsPath;
        public string PrelimsPath
        {
            get { return prelimsPath; }
            set
            {
                if (prelimsPath == value)
                    return;
                prelimsPath = value;
                OnPropertyChanged();
            }
        }

        private string finalsPath;
        public string FinalsPath
        {
            get { return finalsPath; }
            set
            {
                if (finalsPath == value)
                    return;
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
        public ICommand AddJudgeCommand { get; set; }
        public ICommand AddCompetitionCommand { get; set; }
        public ICommand ParseScoreSheetsCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand SelectPrelimsPathCommand { get; set; }
        public ICommand SelectFinalsPathCommand { get; set; }

        public AddCompetitionViewModel()
        {
            AddCompetitorCommand = new DelegateCommand(AddCompetitor);
            AddJudgeCommand = new DelegateCommand(AddJudge);
            AddCompetitionCommand = new DelegateCommand(AddCompetition);
            SelectPrelimsPathCommand = new DelegateCommand(SelectPrelimsPath);
            SelectFinalsPathCommand = new DelegateCommand(SelectFinalsPath);
            ParseScoreSheetsCommand = new DelegateCommand(ParseScoreSheets);
            CancelCommand = new DelegateCommand(Clear);

            _databaseProvider = App.DatabaseProvider;
            _client = new HttpClient();
            _client.BaseAddress = new Uri("https://points.worldsdc.com/");

            Competition = new Competition(Division.AllStar);
            ScoresheetSelector = ScoresheetSelector.Auto;

            ScoresheetSelector = ScoresheetSelector.StepRightSolutions;
            PrelimsPath = @"C:\Users\Alec\source\Impartial\ImpartialUI\Scoresheets\Unlogged\2022-07-23 swingtime\prelims.html";
            FinalsPath = @"C:\Users\Alec\source\Impartial\ImpartialUI\Scoresheets\Unlogged\2022-07-23 swingtime\finals.html";
            ParseScoreSheets();
        }

        private async void TestData()
        {
            var competition = new Competition()
            {
                Date = DateTime.Parse("1-1-2023"),
                Name = "Countdown Swing Boston",
                Division = Division.AllStar,
                Scores = new List<Score>()
            };

            Judge anne = await _databaseProvider.GetJudgeByNameAsync("Anne", "Fleming");
            Judge arjay = await _databaseProvider.GetJudgeByNameAsync("Arjay", "Centeno");
            Judge bryn = await _databaseProvider.GetJudgeByNameAsync("Bryn", "Anderson");
            Judge john = await _databaseProvider.GetJudgeByNameAsync("John", "Lindo");
            Judge lemery = await _databaseProvider.GetJudgeByNameAsync("Lemery", "Rollinscott");

            Competitor brandon = await _databaseProvider.GetCompetitorByNameAsync("Brandon", "Rasmussen");
            Competitor melodie = await _databaseProvider.GetCompetitorByNameAsync("Melodie", "Paletta");
            Competitor neil = await _databaseProvider.GetCompetitorByNameAsync("Neil", "Joshi");
            Competitor kristen = await _databaseProvider.GetCompetitorByNameAsync("Kristen", "Wallace");
            Competitor lucky = await _databaseProvider.GetCompetitorByNameAsync("Lucky", "Sipin");
            Competitor dimitri = await _databaseProvider.GetCompetitorByNameAsync("Dimitri", "Hector");
            Competitor maxwell = await _databaseProvider.GetCompetitorByNameAsync("Maxwell", "Thew");
            Competitor shanna = await _databaseProvider.GetCompetitorByNameAsync("Shanna", "Porcari");
            Competitor oscar = await _databaseProvider.GetCompetitorByNameAsync("Oscar", "Hampton");
            Competitor saya = await _databaseProvider.GetCompetitorByNameAsync("Sayaka", "Suzaki");
            Competitor joshu = await _databaseProvider.GetCompetitorByNameAsync("Joshu", "Creel");
            Competitor jen = await _databaseProvider.GetCompetitorByNameAsync("Jen", "Ferreira");
            Competitor edem = await _databaseProvider.GetCompetitorByNameAsync("Edem", "Attikese");
            Competitor jia = await _databaseProvider.GetCompetitorByNameAsync("Jia", "Lu");
            Competitor kyle = await _databaseProvider.GetCompetitorByNameAsync("Kyle", "FitzGerald");
            Competitor rachel = await _databaseProvider.GetCompetitorByNameAsync("Rachel", "Shook");
            Competitor sam = await _databaseProvider.GetCompetitorByNameAsync("Sam", "Vaden");
            Competitor elli = await _databaseProvider.GetCompetitorByNameAsync("Elli", "Warner");
            Competitor kaiano = await _databaseProvider.GetCompetitorByNameAsync("Kaiano", "Levine");
            Competitor liz = await _databaseProvider.GetCompetitorByNameAsync("Liz", "Ravdin");
            Competitor alec = await _databaseProvider.GetCompetitorByNameAsync("Alec", "Grant");
            Competitor olivia = await _databaseProvider.GetCompetitorByNameAsync("Olivia", "Burnsed");
            Competitor david = await _databaseProvider.GetCompetitorByNameAsync("David", "Carrington");
            Competitor jesann = await _databaseProvider.GetCompetitorByNameAsync("Jes Ann", "Nail");

            competition.Scores.Add(new Score(competition, anne, brandon, melodie, 1, 1));
            competition.Scores.Add(new Score(competition, anne, neil, kristen, 4, 2));
            competition.Scores.Add(new Score(competition, anne, lucky, dimitri, 3, 3));
            competition.Scores.Add(new Score(competition, anne, maxwell, shanna, 6, 4));
            competition.Scores.Add(new Score(competition, anne, oscar, saya, 9, 5));
            competition.Scores.Add(new Score(competition, anne, joshu, jen, 8, 6));
            competition.Scores.Add(new Score(competition, anne, edem, jia, 5, 7));
            competition.Scores.Add(new Score(competition, anne, kyle, rachel, 2, 8));
            competition.Scores.Add(new Score(competition, anne, sam, elli, 12, 9));
            competition.Scores.Add(new Score(competition, anne, kaiano, liz, 7, 10));
            competition.Scores.Add(new Score(competition, anne, alec, olivia, 10, 11));
            competition.Scores.Add(new Score(competition, anne, david, jesann, 11, 12));

            competition.Scores.Add(new Score(competition, arjay, brandon, melodie, 2, 1));
            competition.Scores.Add(new Score(competition, arjay, neil, kristen, 4, 2));
            competition.Scores.Add(new Score(competition, arjay, lucky, dimitri, 3, 3));
            competition.Scores.Add(new Score(competition, arjay, maxwell, shanna, 1, 4));
            competition.Scores.Add(new Score(competition, arjay, oscar, saya, 5, 5));
            competition.Scores.Add(new Score(competition, arjay, joshu, jen, 8, 6));
            competition.Scores.Add(new Score(competition, arjay, edem, jia, 12, 7));
            competition.Scores.Add(new Score(competition, arjay, kyle, rachel, 6, 8));
            competition.Scores.Add(new Score(competition, arjay, sam, elli, 7, 9));
            competition.Scores.Add(new Score(competition, arjay, kaiano, liz, 9, 10));
            competition.Scores.Add(new Score(competition, arjay, alec, olivia, 10, 11));
            competition.Scores.Add(new Score(competition, arjay, david, jesann, 11, 12));

            competition.Scores.Add(new Score(competition, bryn, brandon, melodie, 11, 1));
            competition.Scores.Add(new Score(competition, bryn, neil, kristen, 1, 2));
            competition.Scores.Add(new Score(competition, bryn, lucky, dimitri, 3, 3));
            competition.Scores.Add(new Score(competition, bryn, maxwell, shanna, 4, 4));
            competition.Scores.Add(new Score(competition, bryn, oscar, saya, 2, 5));
            competition.Scores.Add(new Score(competition, bryn, joshu, jen, 6, 6));
            competition.Scores.Add(new Score(competition, bryn, edem, jia, 7, 7));
            competition.Scores.Add(new Score(competition, bryn, kyle, rachel, 10, 8));
            competition.Scores.Add(new Score(competition, bryn, sam, elli, 5, 9));
            competition.Scores.Add(new Score(competition, bryn, kaiano, liz, 9, 10));
            competition.Scores.Add(new Score(competition, bryn, alec, olivia, 8, 11));
            competition.Scores.Add(new Score(competition, bryn, david, jesann, 12, 12));

            competition.Scores.Add(new Score(competition, john, brandon, melodie, 1, 1));
            competition.Scores.Add(new Score(competition, john, neil, kristen, 2, 2));
            competition.Scores.Add(new Score(competition, john, lucky, dimitri, 5, 3));
            competition.Scores.Add(new Score(competition, john, maxwell, shanna, 3, 4));
            competition.Scores.Add(new Score(competition, john, oscar, saya, 9, 5));
            competition.Scores.Add(new Score(competition, john, joshu, jen, 4, 6));
            competition.Scores.Add(new Score(competition, john, edem, jia, 6, 7));
            competition.Scores.Add(new Score(competition, john, kyle, rachel, 7, 8));
            competition.Scores.Add(new Score(competition, john, sam, elli, 8, 9));
            competition.Scores.Add(new Score(competition, john, kaiano, liz, 10, 10));
            competition.Scores.Add(new Score(competition, john, alec, olivia, 11, 11));
            competition.Scores.Add(new Score(competition, john, david, jesann, 12, 12));

            competition.Scores.Add(new Score(competition, lemery, brandon, melodie, 4, 1));
            competition.Scores.Add(new Score(competition, lemery, neil, kristen, 2, 2));
            competition.Scores.Add(new Score(competition, lemery, lucky, dimitri, 1, 3));
            competition.Scores.Add(new Score(competition, lemery, maxwell, shanna, 10, 4));
            competition.Scores.Add(new Score(competition, lemery, oscar, saya, 3, 5));
            competition.Scores.Add(new Score(competition, lemery, joshu, jen, 5, 6));
            competition.Scores.Add(new Score(competition, lemery, edem, jia, 7, 7));
            competition.Scores.Add(new Score(competition, lemery, kyle, rachel, 12, 8));
            competition.Scores.Add(new Score(competition, lemery, sam, elli, 9, 9));
            competition.Scores.Add(new Score(competition, lemery, kaiano, liz, 6, 10));
            competition.Scores.Add(new Score(competition, lemery, alec, olivia, 8, 11));
            competition.Scores.Add(new Score(competition, lemery, david, jesann, 11, 12));

            Competition = competition;
        }

        private void Clear()
        {
            Competition = new Competition(Division.AllStar);
            PrelimsPath = string.Empty;
            FinalsPath = string.Empty;
            ScoresheetSelector = ScoresheetSelector.Auto;
        }

        private async void AddCompetitor()
        {
            if (int.TryParse(WsdcId, out int id)) {
                var newCompetitor = new Competitor(FirstName, LastName, int.Parse(WsdcId));

                await _databaseProvider.UpsertCompetitorAsync(newCompetitor);

                FirstName = string.Empty;
                LastName = string.Empty;
                WsdcId = string.Empty;
            }
        }

        private async void AddJudge()
        {
            var newJudge = new Judge(JudgeFirstName, JudgeLastName);

            await _databaseProvider.UpsertJudgeAsync(newJudge);

            JudgeFirstName = string.Empty;
            JudgeLastName = string.Empty;
        }

        private void AddCompetition()
        {
            Trace.WriteLine(Competition.ToLongString());

            _databaseProvider.UpsertCompetitionAsync(Competition);

            Clear();
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
                case ScoresheetSelector.StepRightSolutions:
                    _scoresheetParser = new StepRightSolutionsParser(prelimsPath, finalsPath);
                    break;
                case ScoresheetSelector.WorldDanceRegistry:
                    _scoresheetParser = new WorldDanceRegistryParser(prelimsPath, finalsPath);
                    break;
                case ScoresheetSelector.Other:
                default:
                    return;
            }

            if (!_scoresheetParser.GetDivisions().Contains(Division.AllStar))
                return;

            var comp = _scoresheetParser.GetCompetition(Division.AllStar);

            foreach (var competitor in comp.PrelimLeaders)
            {
                var serverCompetitor = await _databaseProvider.GetCompetitorByNameAsync(competitor.FirstName, competitor.LastName);

                if (serverCompetitor != null)
                {
                    serverCompetitor = new Competitor(
                        serverCompetitor.Id, serverCompetitor.WsdcId,
                        serverCompetitor.FirstName, serverCompetitor.LastName,
                        serverCompetitor.LeadStats.Rating, serverCompetitor.LeadStats.Variance,
                        serverCompetitor.FollowStats.Rating, serverCompetitor.FollowStats.Variance);

                    var scores = comp.LeaderPrelimScores.Where(s => s.Competitor.FullName == serverCompetitor.FullName).ToList();
                    foreach (var score in scores)
                    {
                        score.Competitor = serverCompetitor;
                    }
                }
                else
                {
                    int wsdcId = await GuessWsdcId(competitor.FirstName, competitor.LastName);

                    var scores = comp.LeaderPrelimScores.Where(s => s.Competitor.FullName == competitor.FullName).ToList();
                    foreach (var score in scores)
                    {
                        score.Competitor.WsdcId = wsdcId;
                    }
                }
            }

            foreach (var competitor in comp.PrelimFollowers)
            {
                var serverCompetitor = await _databaseProvider.GetCompetitorByNameAsync(competitor.FirstName, competitor.LastName);

                if (serverCompetitor != null)
                {
                    serverCompetitor = new Competitor(
                        serverCompetitor.Id, serverCompetitor.WsdcId,
                        serverCompetitor.FirstName, serverCompetitor.LastName,
                        serverCompetitor.LeadStats.Rating, serverCompetitor.LeadStats.Variance,
                        serverCompetitor.FollowStats.Rating, serverCompetitor.FollowStats.Variance);

                    var scores = comp.FollowerPrelimScores.Where(s => s.Competitor.FullName == serverCompetitor.FullName).ToList();
                    foreach (var score in scores)
                    {
                        score.Competitor = serverCompetitor;
                    }
                }
                else
                {
                    int wsdcId = await GuessWsdcId(competitor.FirstName, competitor.LastName);

                    var scores = comp.FollowerPrelimScores.Where(s => s.Competitor.FullName == competitor.FullName).ToList();
                    foreach (var score in scores)
                    {
                        score.Competitor.WsdcId = wsdcId;
                    }
                }
            }

            foreach (var judge in comp.Judges)
            {
                var serverJudge = await _databaseProvider.GetJudgeByNameAsync(judge.FirstName, judge.LastName);

                if (serverJudge != null)
                {
                    serverJudge = new Judge(serverJudge.Id, serverJudge.FirstName, serverJudge.LastName, serverJudge.Accuracy, serverJudge.Top5Accuracy)
                    {
                        Scores = judge.Scores
                    };

                    var scores = comp.Scores.Where(s => s.Judge.FullName == serverJudge.FullName).ToList();
                    foreach (var score in scores)
                    {
                        score.Judge = serverJudge;
                    }
                }
            }

            foreach (var couple in comp.Couples)
            {
                var serverLeader = await _databaseProvider.GetCompetitorByNameAsync(couple.Leader.FirstName, couple.Leader.LastName);

                if (serverLeader != null)
                {
                    serverLeader = new Competitor(
                        serverLeader.Id, serverLeader.WsdcId,
                        serverLeader.FirstName, serverLeader.LastName,
                        serverLeader.LeadStats.Rating, serverLeader.LeadStats.Variance,
                        serverLeader.FollowStats.Rating, serverLeader.FollowStats.Variance);

                    var scores = comp.Scores.Where(s => s.Leader.FullName == serverLeader.FullName).ToList();
                    foreach (var score in scores)
                    {
                        score.Leader = serverLeader;
                    }
                }
                else
                {
                    int wsdcId = await GuessWsdcId(couple.Leader.FirstName, couple.Leader.LastName);

                    var scores = comp.Scores.Where(s => s.Leader.FullName == couple.Leader.FullName).ToList();
                    foreach (var score in scores)
                    {
                        score.Leader.WsdcId = wsdcId;
                    }
                }

                var serverFollower = await _databaseProvider.GetCompetitorByNameAsync(couple.Follower.FirstName, couple.Follower.LastName);

                if (serverFollower != null)
                {
                    serverFollower = new Competitor(
                        serverFollower.Id, serverFollower.WsdcId,
                        serverFollower.FirstName, serverFollower.LastName,
                        serverFollower.LeadStats.Rating, serverFollower.LeadStats.Variance,
                        serverFollower.FollowStats.Rating, serverFollower.FollowStats.Variance);

                    var scores = comp.Scores.Where(s => s.Follower.FullName == serverFollower.FullName).ToList();
                    foreach (var score in scores)
                    {
                        score.Follower = serverFollower;
                    }
                }
                else
                {
                    int wsdcId = await GuessWsdcId(couple.Follower.FirstName, couple.Follower.LastName);

                    var scores = comp.Scores.Where(s => s.Follower.FullName == couple.Follower.FullName).ToList();
                    foreach (var score in scores)
                    {
                        score.Follower.WsdcId = wsdcId;
                    }
                }
            }

            Competition = comp;
        }

        private async Task<int> GuessWsdcId(string firstName, string LastName)
        {
            try
            {
                var response = await _client.PostAsync("/lookup/find?q=" + FirstName + "%20" + LastName, null);
                string sheet = await response.Content.ReadAsStringAsync();
                string idString = Regex.Match(sheet.Substring(sheet.IndexOf("wscid"), 20), @"\d+").Value;
                return Int32.TryParse(idString, out int id) ? id : -1;
            } 
            catch
            {
                return -1;
            }

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
