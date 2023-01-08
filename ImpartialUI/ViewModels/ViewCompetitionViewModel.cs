using Impartial;

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

        public ViewCompetitionViewModel()
        {

        }
    }
}
