using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BrickBreaker
{
    /// <summary>
    /// Interaction logic for LevelSelectorPage.xaml
    /// </summary>
    public partial class LevelSelectorPage : Page
    {
        public LevelSelectorPage()
        {
            InitializeComponent();
            LoadLevels();
        }

        private void LoadLevels()
        {
            int height = 50;
            int width = 100;
            int maxLevelCount = 6;
            int col = 0;
            int row = 0;

            for (int i = 0; i < LevelLoader.LevelCount; i++)
            {
                int levelIndex = i;
                Button levelButton = new()
                {
                    Height = height,
                    Width = width,
                    Content = "Level " + (i + 1),
                    Margin = new Thickness(5),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                Canvas.SetTop(levelButton, (height + 10) * row + 100);
                Canvas.SetLeft(levelButton, (width + 10) * col + 50);

                grid.Children.Add(levelButton);

                levelButton.Click += (sender, e) =>
                {
                    LevelLoader.SelectedLevel = levelIndex;
                    StartPage.Update();
                    NavigationService?.GoBack();
                };

                col++;
                if (col >= maxLevelCount)
                {
                    col = 0;
                    row++;
                }
            }
        }



        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}