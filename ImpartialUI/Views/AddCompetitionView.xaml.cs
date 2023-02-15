using System.Windows.Controls;

namespace ImpartialUI.Views
{
    public partial class AddCompetitionView : UserControl
    {
        public AddCompetitionView()
        {
            InitializeComponent();
        }

        private void AddJudgeButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            LeaderPrelimsAdder.UpdateJudges();
            FollowersPrelimsAdder.UpdateJudges();
            FinalsCompetitionAdder.UpdateJudges();
        }

        private void AddCompetitorButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            LeaderPrelimsAdder.UpdateCompetitors();
            FollowersPrelimsAdder.UpdateCompetitors();
            FinalsCompetitionAdder.UpdateCompetitors();
        }
    }
}
