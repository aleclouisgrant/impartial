using Impartial;
using ImpartialUI.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace ImpartialUI.ViewModels
{
    public class SelectJudgeViewModel : BaseViewModel
    {
        //public bool? Selected => SelectedJudge != null;
        public bool? Selected => true;

        public Judge Judge { get; set; }

        private Judge selectedJudge;
        public Judge SelectedJudge
        {
            get { return selectedJudge; }
            set
            {
                selectedJudge = value;
                OnPropertyChanged();
            }
        }

        public SelectJudgeViewModel(Judge judge)
        {
            Judge = judge;
        }
    }
}
