using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BrickBreaker
{
    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : Page
    {
        private static TextBlock textBlock { get; set; } = new();
        public StartPage()
        {
            InitializeComponent();
            textBlock = levelLabel;
            Update();
        }
        public static void Update()
        {
            textBlock.Text = "Level " + (LevelLoader.SelectedLevel + 1);
        }
        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new GamePage());
        }

        private void Statistics_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new StatisticsPage());
        }

        private void LevelSelector_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new LevelSelectorPage());
        }
    }
}
