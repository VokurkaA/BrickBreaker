using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
    /// Interaction logic for GamePage.xaml
    /// </summary>
    public partial class GamePage : Page
    {
        public static double CanvasWidth { get; private set; }
        public static double CanvasHeight { get; private set; }
        public GamePage()
        {
            InitializeComponent();
            InitializeGame();
        }
        void InitializeGame()
        {
            CanvasWidth = canvas.Width;
            CanvasHeight = canvas.Height;
            //Brick[,] brickLayout = LevelLoader.Levels[LevelLoader.SelectedLevel];

            Game game = new(canvas, LevelLoader.GetLevel());
            game.Start();
            NavigationService?.GoBack();
        }
    }
}
