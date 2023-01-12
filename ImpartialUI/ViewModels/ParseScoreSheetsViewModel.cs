using ImpartialUI.Commands;
using Microsoft.Win32;
using System.Windows.Input;
using Impartial;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System;
using Impartial.Services.ScoresheetParser;
using System.Threading.Tasks;

namespace ImpartialUI.ViewModels
{
    public class ParseScoreSheetsViewModel : BaseViewModel
    {
        private IDatabaseProvider _databaseProvider;
        private IScoresheetParser _scoresheetParser;

        private bool sort = true;

        public List<SelectJudgeViewModel> SelectJudges { get; set; }
        public List<Judge> Judges { get; set; }
        public List<Judge> JudgesDb { get; set; }

        public List<Competition> Competitions { get; set; }

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

        private string firstName;
        public string FirstName
        { 
            get { return firstName; }
            set
            {
                firstName = value;
                OnPropertyChanged();
            }
        }
        private string lastName;
        public string LastName
        {
            get { return lastName; }
            set
            {
                lastName = value;
                OnPropertyChanged();
            }
        }
        public Visibility SelectJudgesVisibility => SelectJudges?.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        public ICommand SelectPrelimsPathCommand { get; set; }
        public ICommand SelectFinalsPathCommand { get; set; }
        public ICommand AssignJudgesCommand { get; set; }
        public ICommand ParseScoreSheetsCommand { get; set; }

        public ICommand AddJudgeCommand { get; set; }
        public ICommand DeleteJudgeCommand { get; set; }
        public ICommand RefreshJudgesDatabaseCommand { get; set; }
        public ICommand ToggleSortJudgesCommand { get; set; }
        public ICommand ClearJudgesDatabaseCommand { get; set; }
        public ICommand SendToDatabaseCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public ICommand GuessJudgesCommand { get; set; }

        private string messageLog;
        public string MessageLog
        {
            get { return messageLog; }
            set
            {
                messageLog = value;
                OnPropertyChanged();
            }
        }

        public ParseScoreSheetsViewModel()
        {
            SelectPrelimsPathCommand = new DelegateCommand(SelectPrelimsPath);
            SelectFinalsPathCommand = new DelegateCommand(SelectFinalsPath);
            AssignJudgesCommand = new DelegateCommand(GetJudges);
            ParseScoreSheetsCommand = new DelegateCommand(ParseScoreSheets);
            RefreshJudgesDatabaseCommand = new DelegateCommand(RefreshJudgesDatabase);
            AddJudgeCommand = new DelegateCommand(AddJudge);
            DeleteJudgeCommand = new DelegateCommand(DeleteJudge);
            GuessJudgesCommand = new DelegateCommand(GuessJudges);
            ToggleSortJudgesCommand = new DelegateCommand(ToggleSortJudges);
            ClearJudgesDatabaseCommand = new DelegateCommand(ClearJudgesDatabase);
            SendToDatabaseCommand = new DelegateCommand(SendToDatabase);
            CancelCommand = new DelegateCommand(Cancel);

            //this should be passed in the constructor via DI
            _databaseProvider = App.DatabaseProvider;

            RefreshJudgesDatabase();
        }

        private async void RefreshJudgesDatabase()
        {
            //JudgesDb = (await _databaseProvider.GetAllJudgesAsync()).ToList();
            //JudgesDb = JudgesDb.OrderBy(j => j.Accuracy).ToList();
            OnPropertyChanged(nameof(JudgesDb));
        }

        private void GuessJudges()
        {
            if (SelectJudges == null)
                return;

            foreach (var judge in SelectJudges)
            {
                judge.SelectedJudge = (Judge)Util.GetClosestPersonByFirstName(judge.Judge.FirstName, JudgesDb);
            }
        }

        private async void AddJudge()
        {
            await _databaseProvider.UpsertJudgeAsync(new Judge(FirstName, LastName));
            RefreshJudgesDatabase();

            // clear the name fields
            FirstName = ""; LastName = "";
        }
        private void DeleteJudge(object obj)
        {
            _databaseProvider.DeleteJudgeAsync((Judge)obj);
            RefreshJudgesDatabase();
        }
        private void ClearJudgesDatabase()
        {
            //_databaseProvider.DeleteAllJudgesAsync();
            RefreshJudgesDatabase();
        }

        private void ToggleSortJudges()
        {
            if (sort)
                SortJudgesListByTop5();
            else
                SortJudgesListByTotal();

            sort = !sort;
        }

        private void SortJudgesListByTop5()
        {
            JudgesDb = JudgesDb.OrderBy(j => j.Top5Accuracy).ToList();
            OnPropertyChanged(nameof(JudgesDb));
        }
        private void SortJudgesListByTotal()
        {
            JudgesDb = JudgesDb.OrderBy(j => j.Accuracy).ToList();
            OnPropertyChanged(nameof(JudgesDb));
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

        private void GetJudges()
        {
            //temp this is gonna be in a loop
            var division = Division.AllStar;

            _scoresheetParser = new EEProParser(PrelimsPath, FinalsPath);
            RefreshJudgesDatabase();

            var selectJudges = new List<SelectJudgeViewModel>();
            foreach (var judge in Judges)
            {
                selectJudges.Add(new SelectJudgeViewModel(judge) { SelectedJudge = (Judge)Util.GetClosestPersonByFirstName(judge.FirstName, JudgesDb) });
            }

            SelectJudges = selectJudges;

            if (Judges.Count == 0)
                MessageLog = "No judges found for this division";

            OnPropertyChanged(nameof(SelectJudgesVisibility));
            OnPropertyChanged(nameof(Judges));
            OnPropertyChanged(nameof(SelectJudges));
        }

        private void ParseScoreSheets()
        {
            _scoresheetParser = new EEProParser(prelimsPath, finalsPath);

            Competitions = new List<Competition>();
            Judges = new List<Judge>();

            var divisions = _scoresheetParser.GetDivisions();

            foreach (var division in divisions)
            {
                var comp = _scoresheetParser.GetCompetition(division);
                Competitions.Add(comp);

                foreach (var judge in comp.Judges)
                {
                    if (!Judges.Any(j => j.FullName == judge.FullName)) //these should actually be compared with IDs
                    {
                        Judges.Add(new Judge(judge.FirstName, judge.LastName)
                        {
                            Scores = judge.Scores
                        });
                    }
                    else
                    {
                        Judges.FirstOrDefault(j => j.FullName == judge.FullName).Scores.AddRange(judge.Scores);
                    }
                }
            }

            // sort from lowest to highest division
            Competitions = Competitions.OrderBy(c => (int)c.Division).ToList();

            OnPropertyChanged(nameof(Competitions));
            OnPropertyChanged(nameof(Judges));
        }

        private void SendToDatabase()
        {
            foreach (var competition in Competitions)
            {
                foreach (var score in competition.Scores)
                {
                    if (score.Judge.Scores == null)
                        score.Judge.Scores = new List<Score>();

                    //_databaseProvider.UpdateJudgeAsync(score.Judge.Id, score.Judge);
                }
            }

            MessageLog = "completed";
            RefreshJudgesDatabase();

            Judges = null; SelectJudges = null;
            OnPropertyChanged(nameof(Judges));
            OnPropertyChanged(nameof(SelectJudges));
        }

        private void Cancel()
        {
            //close window
        }
    }
}
