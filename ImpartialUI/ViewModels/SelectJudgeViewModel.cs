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

        public IJudge Judge { get; set; }

        private IJudge selectedJudge;
        public IJudge SelectedJudge
        {
            get { return selectedJudge; }
            set
            {
                selectedJudge = value;
                OnPropertyChanged();
            }
        }

        public SelectJudgeViewModel(IJudge judge)
        {
            Judge = judge;
        }
    }
}
