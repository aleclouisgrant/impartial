﻿using Impartial;
using ImpartialUI.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace ImpartialUI.ViewModels
{
    public class ViewCompetitionViewModel : BaseViewModel
    {
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

        private List<Competition> _competitions;
        public List<Competition> Competitions
        {
            get { return _competitions; }
            set
            {
                _competitions = value.ToList();
                OnPropertyChanged();
            }
        }

        public ICommand RefreshCompetitionsCommand { get; set; }
        public ICommand DeleteCompetitionCommand { get; set; }

        public ViewCompetitionViewModel()
        {
            RefreshCompetitionsCommand = new DelegateCommand(RefreshCompetitions);
            DeleteCompetitionCommand = new DelegateCommand(DeleteCompetition);
        }

        public ViewCompetitionViewModel(List<Competition> competitions)
        {
            Competitions = competitions;

            RefreshCompetitionsCommand = new DelegateCommand(RefreshCompetitions);
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
            Competitions.Remove(Competition);

            Competition = null;
        }
    }
}
