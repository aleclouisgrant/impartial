using Impartial;
using ImpartialUI.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace ImpartialUI.ViewModels
{
    public class ViewCompetitionViewModel : BaseViewModel
    {
        private List<ICompetition> _competitions;
        public List<ICompetition> Competitions
        {
            get { return _competitions; }
            set
            {
                _competitions = value.ToList();
                OnPropertyChanged();
            }
        }

        private ICompetition _selectedCompetition;
        public ICompetition SelectedCompetition 
        { 
            get { return _selectedCompetition; }
            set 
            {
                _selectedCompetition = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(LeaderPrelims));
                OnPropertyChanged(nameof(FollowerPrelims));
                OnPropertyChanged(nameof(LeaderSemis));
                OnPropertyChanged(nameof(FollowerSemis));
                OnPropertyChanged(nameof(FinalCompetition));

                ShowFinals = SelectedCompetition?.FinalCompetition != null;
                ShowPrelims = SelectedCompetition?.PairedPrelimCompetitions.Where(c => c.Round == Round.Prelims).FirstOrDefault() != null;
                ShowSemis = SelectedCompetition?.PairedPrelimCompetitions.Where(c => c.Round == Round.Semifinals).FirstOrDefault() != null;

                OnPropertyChanged(nameof(ShowPrelims));
                OnPropertyChanged(nameof(ShowSemis));
                OnPropertyChanged(nameof(ShowFinals));
            }
        }

        public IPrelimCompetition LeaderPrelims => SelectedCompetition?.PairedPrelimCompetitions.Where(c => c.Round == Round.Prelims).FirstOrDefault()?.LeaderPrelimCompetition;
        public IPrelimCompetition FollowerPrelims => SelectedCompetition?.PairedPrelimCompetitions.Where(c => c.Round == Round.Prelims).FirstOrDefault()?.FollowerPrelimCompetition;

        public IPrelimCompetition LeaderSemis => SelectedCompetition?.PairedPrelimCompetitions.Where(c => c.Round == Round.Semifinals).FirstOrDefault()?.LeaderPrelimCompetition;
        public IPrelimCompetition FollowerSemis => SelectedCompetition?.PairedPrelimCompetitions.Where(c => c.Round == Round.Semifinals).FirstOrDefault()?.FollowerPrelimCompetition;

        public IFinalCompetition FinalCompetition => SelectedCompetition?.FinalCompetition;

        public bool ShowPrelims { get; set; }
        public bool ShowSemis { get; set; }
        public bool ShowFinals { get; set; }

        public ICommand RefreshCompetitionsCommand { get; set; }
        public ICommand SaveCompetitionCommand { get; set; }
        public ICommand DeleteCompetitionCommand { get; set; }

        public ViewCompetitionViewModel()
        {
            RefreshCompetitionsCommand = new DelegateCommand(RefreshCompetitions);
            SaveCompetitionCommand = new DelegateCommand(SaveCompetition);
            DeleteCompetitionCommand = new DelegateCommand(DeleteCompetition);
        }

        private async void RefreshCompetitions()
        {
            Competitions = (await App.DatabaseProvider.GetAllCompetitionsAsync()).OrderBy(c => c.Date).ToList();
        }
        private async void DeleteCompetition()
        {
            await App.DatabaseProvider.DeleteCompetitionAsync(SelectedCompetition.Id);

            // should be able to do just this instead of refreshing but it's not working for some reason
            //Competitions.Remove(Competition);
            //OnPropertyChanged(nameof(Competitions));

            RefreshCompetitions();
            SelectedCompetition = null;
        }
        private async void SaveCompetition()
        {
            Guid id = SelectedCompetition.Id;
            //TODO:
            //await App.DatabaseProvider.UpsertCompetitionAsync(Competition);

            RefreshCompetitions();
            SelectedCompetition = Competitions.Where(c => c.Id == id).FirstOrDefault();
        }
    }
}
