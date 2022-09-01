using ImpartialUI.Commands;
using Microsoft.Win32;
using System.Windows.Input;
using Impartial;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System;

namespace ImpartialUI.ViewModels
{
    public class ParseScoreSheetsViewModel : BaseViewModel
    {
        static string DATABASE_STRING = "Impartial";

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

        public ICommand ShowCalculationCommand { get; set; }
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
            ShowCalculationCommand = new DelegateCommand(ShowCalculation);
            GuessJudgesCommand = new DelegateCommand(GuessJudges);
            ToggleSortJudgesCommand = new DelegateCommand(ToggleSortJudges);
            ClearJudgesDatabaseCommand = new DelegateCommand(ClearJudgesDatabase);
            SendToDatabaseCommand = new DelegateCommand(SendToDatabase);

            _databaseProvider = new MongoDatabaseProvider(DATABASE_STRING);
            RefreshJudgesDatabase();
        }

        private void RefreshJudgesDatabase()
        {
            JudgesDb = _databaseProvider.GetAllJudges();
            JudgesDb = JudgesDb.OrderBy(j => j.Accuracy).ToList();
            OnPropertyChanged(nameof(JudgesDb));
        }

        private void ShowCalculation(object obj)
        {
            Judge judge = (Judge)obj;

            string s = judge.FullName + " (" + judge.Accuracy + "):";
            int e = 0;

            foreach (var score in judge.Scores)
            {
                int e1 = Math.Abs(score.ActualPlacement - score.Placement);
                s += " (" + score.ActualPlacement + "-" + score.Placement + " = " + e1 + ") +";
                e = e + e1;
            }
            s = s.Trim('+');
            s += "= " + e + "/" + judge.Count + " = " + Math.Round((double)e / judge.Count, 2);

            MessageLog = s;
        }
        private void GuessJudges()
        {
            if (SelectJudges == null)
                return;

            foreach (var judge in SelectJudges)
            {
                judge.SelectedJudge = GetClosestJudgeByFirstName(judge.Judge.FirstName, JudgesDb);
            }
        }

        private void AddJudge()
        {
            _databaseProvider.InsertJudge(new Judge(FirstName, LastName));
            RefreshJudgesDatabase();

            // clear the name fields
            FirstName = ""; LastName = "";
        }
        private void DeleteJudge(object obj)
        {
            _databaseProvider.DeleteJudge((Judge)obj);
            RefreshJudgesDatabase();
        }
        private void ClearJudgesDatabase()
        {
            _databaseProvider.DeleteAllJudges();
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
            var division = Division.Advanced;

            _scoresheetParser = new EEProParser(PrelimsPath, FinalsPath);
            Judges = _scoresheetParser.GetJudgesByDivision(division);
            RefreshJudgesDatabase();

            var selectJudges = new List<SelectJudgeViewModel>();
            foreach (var judge in Judges)
            {
                selectJudges.Add(new SelectJudgeViewModel(judge) { SelectedJudge = GetClosestJudgeByFirstName(judge.FirstName, JudgesDb) });
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
            var divisions = _scoresheetParser.GetDivisions();

            foreach (var division in divisions)
            {
                Competitions.Add(_scoresheetParser.GetCompetition(division));
            }

            // sort from lowest to highest division
            Competitions = Competitions.OrderBy(c => (int)c.Division).ToList();

            OnPropertyChanged(nameof(Competitions));
        }

        private void SendToDatabase()
        {
            foreach (var competition in Competitions)
            {
                foreach (var score in competition.Scores)
                {
                    if (score.Judge.Scores == null)
                        score.Judge.Scores = new List<Score>();

                    score.Judge.Accuracy = Math.Round(score.Judge.Scores.Sum(s => s.Accuracy) / score.Judge.Scores.Count, 2);
                    score.Judge.Top5Accuracy = Math.Round(score.Judge.Scores.FindAll(s => s.ActualPlacement <= 5).Sum(s => s.Accuracy) / score.Judge.Scores.Count, 2);
                    _databaseProvider.UpdateJudge(score.Judge.Id, score.Judge);
                }
            }

            MessageLog = "completed";
            RefreshJudgesDatabase();

            Judges = null; SelectJudges = null;
            OnPropertyChanged(nameof(Judges));
            OnPropertyChanged(nameof(SelectJudges));
        }

        private Judge GetClosestJudgeByFirstName(string input, List<Judge> list)
        {
            int leastDistance = 10000;
            Judge match = null;

            foreach (Judge j in list)
            {
                int d = GetEditDistance(input, j.FirstName);
                if (d == 0)
                    return j;

                if (d < leastDistance)
                {
                    leastDistance = d;
                    match = j;
                }
            }

            return match;
        }
        private string GetClosestString(string input, List<string> list)
        {
            int leastDistance = 10000;
            string match = "";

            foreach (string s in list)
            {
                int d = GetEditDistance(input, s);
                if (d == 0)
                    return s;

                if (d < leastDistance)
                {
                    leastDistance = d;
                    match = s;
                }
            }

            return match;
        }
        public int GetEditDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0)
                return m;

            if (m == 0)
                return n;

            for (int i = 0; i <= n; d[i, 0] = i++) { }
            for (int j = 0; j <= m; d[0, j] = j++) { }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }
    }
}
