using Impartial;
using System.Linq;

namespace ImpartialUI.Commands
{
    internal class AddCompetitorToDbCommand : CommandBase
    {
        public override async void Execute(object parameter)
        {
            var newCompetitor = (ICompetitor)parameter;

            await App.DatabaseProvider.UpsertCompetitorAsync(newCompetitor);

            App.CompetitorsDb.Add(newCompetitor);
            App.CompetitorsDb = App.CompetitorsDb.OrderBy(c => c.FullName).ToList();
        }
    }
}
