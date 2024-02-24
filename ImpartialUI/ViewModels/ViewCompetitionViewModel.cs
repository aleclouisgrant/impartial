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
        private ICompetition _competition;
        public ICompetition Competition 
        { 
            get { return _competition; }
            set 
            { 
                _competition = value;
                OnPropertyChanged();
            }
        }

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

        public ICommand RefreshCompetitionsCommand { get; set; }
        public ICommand SaveCompetitionCommand { get; set; }
        public ICommand DeleteCompetitionCommand { get; set; }

        public ViewCompetitionViewModel()
        {
            RefreshCompetitionsCommand = new DelegateCommand(RefreshCompetitions);
            SaveCompetitionCommand = new DelegateCommand(SaveCompetition);
            DeleteCompetitionCommand = new DelegateCommand(DeleteCompetition);
        }

        public ViewCompetitionViewModel(List<ICompetition> competitions)
        {
            Competitions = competitions;

            RefreshCompetitionsCommand = new DelegateCommand(RefreshCompetitions);
            SaveCompetitionCommand = new DelegateCommand(SaveCompetition);
            DeleteCompetitionCommand = new DelegateCommand(DeleteCompetition);

            RefreshCompetitions();
        }

        private async void RefreshCompetitions()
        {
            Competitions = (await App.DatabaseProvider.GetAllCompetitionsAsync()).OrderBy(c => c.Date).ToList();
        }
        private async void DeleteCompetition()
        {
            await App.DatabaseProvider.DeleteCompetitionAsync(Competition);

            // should be able to do just this instead of refreshing but it's not working for some reason
            //Competitions.Remove(Competition);
            //OnPropertyChanged(nameof(Competitions));

            RefreshCompetitions();
            Competition = null;
        }
        private async void SaveCompetition()
        {
            Guid id = Competition.Id;
            await App.DatabaseProvider.UpsertCompetitionAsync(Competition);

            RefreshCompetitions();
            Competition = Competitions.Where(c => c.Id == id).FirstOrDefault();
        }
    }
}
