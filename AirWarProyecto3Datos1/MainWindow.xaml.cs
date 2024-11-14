using AirWarProyecto3Datos1.LogicaCentral;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AirWarProyecto3Datos1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int CellSize = 20;
        private Matriz matriz;
        public MainWindow()
        {
            InitializeComponent();
            matriz = new Matriz(30, 30); // Matriz de 30x30
            GenerarTerrenoPerlin();
            DibujarMapa();
        }

        public void GenerarTerrenoPerlin()
        {
            PerlinNoise perlin = new PerlinNoise(); // Genera una nueva semilla aleatoria cada vez
            for (int i = 0; i < 30; i++)
            {
                for (int j = 0; j < 30; j++)
                {
                    double noiseValue = perlin.Generate(i * 0.1, j * 0.1); // Escala para ajustar el "zoom"
                    matriz.Matrix[i, j].Terreno = noiseValue > 0 ? TipoTerreno.Tierra : TipoTerreno.Mar;
                }
            }
        }
        private void DibujarMapa()
        {
            MapaCanvas.Children.Clear();
            for (int i = 0; i < 30; i++)
            {
                for (int j = 0; j < 30; j++)
                {
                    Nodo nodo = matriz.Matrix[i, j];
                    Rectangle rect = new Rectangle
                    {
                        Width = CellSize,
                        Height = CellSize,
                        Fill = nodo.Terreno == TipoTerreno.Tierra ? Brushes.Green : Brushes.Blue
                    };

                    Canvas.SetLeft(rect, j * CellSize);
                    Canvas.SetTop(rect, i * CellSize);
                    MapaCanvas.Children.Add(rect);
                }
            }
        }
    }

}