using System.Windows;
using System.Windows.Controls;

namespace BrickBreaker
{
    public partial class StatisticsPage : Page
    {
        public StatisticsPage()
        {
            InitializeComponent();
            LoadStatistics();
        }

        private void LoadStatistics()
        {
            TimePlayedTextBlock.Text = ((int)Statistics.TimePlayed).ToString() + " s";
            BrickDestroyedTextBlock.Text = Statistics.BrickDestroyed.ToString();
            GamesPlayedTextBlock.Text = Statistics.GamesPlayed.ToString();
            GamesWonTextBlock.Text = Statistics.GamesWon.ToString();
            GamesLostTextBlock.Text = Statistics.GamesLost.ToString();
            UpgradesCollectedTextBlock.Text = Statistics.UpgradesCollected.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}
