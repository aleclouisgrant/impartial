using Impartial;
using ImpartialUI.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace ImpartialUI.ViewModels
{
    public class RatingsViewModel : BaseViewModel
    {
        public ICommand CrunchRatingsCommand { get; set; }
        private IDatabaseProvider _databaseProvider;

        public RatingsViewModel()
        {
            CrunchRatingsCommand = new DelegateCommand(CrunchRatings);
            _databaseProvider = App.DatabaseProvider;
        }
        private async void CrunchRatings()
        {

            IEnumerable<Competition> competitions = await _databaseProvider.GetAllCompetitionsAsync();
            competitions = competitions.OrderBy(c => c.Date).ToList();
            
            foreach (Competition competition in competitions)
            {
                var couples = EloRatingService.CalculateRatings(competition.Couples);

                foreach (Couple couple in couples)
                {
                    await App.DatabaseProvider.UpdateCompetitorAsync(couple.Leader.Id, couple.Leader);
                    await App.DatabaseProvider.UpdateCompetitorAsync(couple.Follower.Id, couple.Follower);
                }
            }
        }
    }
}
