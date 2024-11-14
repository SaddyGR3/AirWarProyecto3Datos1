using AirWarProyecto3Datos1.Estructuras;
using AirWarProyecto3Datos1.LogicaCentral;
using AirWarProyecto3Datos1.Airplane;
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
using System.Windows.Threading;
using System.Windows.Media.Animation;


namespace AirWarProyecto3Datos1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Variables de la interfaz
        private const int CellSize = 30;
        private Matriz matriz;

        // Variables para las imágenes
        private BitmapImage imgPortaviones;
        private BitmapImage imgAeropuerto;
        private BitmapImage imgAvion;
        private Image imgAvionElement;

        //Instancias
        private Aeropuerto aeropuerto;
        private Portaviones portaviones;
        private Avion avion;

        private DispatcherTimer timer;


        public MainWindow()
        {
            //Aspectos de la Ventana
            InitializeComponent();
            matriz = new Matriz(30, 30); // Matriz de 30x30
            GenerarTerrenoPerlin();
            CargarImagenes(); // Cargar las imágenes una sola vez
  
            // Generar aeropuerto y portaviones antes de dibujar el mapa
            GenerarEstructuras();
            GenerarAvion();
            // Crear la instancia del imgAvionElement y asignarle el BitmapImage de la imagen del avión
            imgAvionElement = new Image
            {
                Width = CellSize,
                Height = CellSize,
                Source = imgAvion 
            };

            // Agrega la transformación de rotación
            imgAvionElement.RenderTransform = new RotateTransform(0);
            imgAvionElement.RenderTransformOrigin = new Point(0.5, 0.5);
            // Configurar y empezar el temporizador para mover el avión
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500); // Mueve el avión cada 500 ms
            timer.Tick += (s, e) => MoverAvion();
            timer.Start();

            DibujarMapa();
        }

        /// </summary>
        /// Logica del Avion
        /// </summary>

        private void GenerarAvion()
        {
            if (aeropuerto != null && portaviones != null)
            {
                avion = new Avion(aeropuerto.Ubicacion, portaviones.Ubicacion, matriz);
            }
        }

        private void MoverAvion()
        {
            Nodo nodoAnterior = avion.NodoActual;
            avion.MoverAvion();

            if (nodoAnterior != null && avion.NodoActual != null)
            {
                int filaAnterior = matriz.GetRow(nodoAnterior);
                int columnaAnterior = matriz.GetColumn(nodoAnterior);
                int filaNueva = matriz.GetRow(avion.NodoActual);
                int columnaNueva = matriz.GetColumn(avion.NodoActual);

                if (!MapaCanvas.Children.Contains(imgAvionElement))
                {
                    MapaCanvas.Children.Add(imgAvionElement);
                }

                // Rotación basada en la dirección del movimiento
                double angle = CalcularAnguloRotacion(columnaAnterior, filaAnterior, columnaNueva, filaNueva);
                RotateTransform rotateTransform = imgAvionElement.RenderTransform as RotateTransform;
                DoubleAnimation animRotation = new DoubleAnimation
                {
                    To = angle,
                    Duration = TimeSpan.FromMilliseconds(250) // Para rotar sin perder el movimiento fluido
                };
                rotateTransform.BeginAnimation(RotateTransform.AngleProperty, animRotation);

                // Movimiento X y Y con animaciones para mantener la fluidez
                DoubleAnimation animX = new DoubleAnimation
                {
                    From = columnaAnterior * CellSize,
                    To = columnaNueva * CellSize,
                    Duration = TimeSpan.FromMilliseconds(500)
                };
                imgAvionElement.BeginAnimation(Canvas.LeftProperty, animX);

                DoubleAnimation animY = new DoubleAnimation
                {
                    From = filaAnterior * CellSize,
                    To = filaNueva * CellSize,
                    Duration = TimeSpan.FromMilliseconds(500)
                };
                imgAvionElement.BeginAnimation(Canvas.TopProperty, animY);
            }
        }


        // Método para calcular el ángulo de rotación
        private double CalcularAngulo(int filaAnterior, int columnaAnterior, int filaNueva, int columnaNueva)
        {
            int deltaX = columnaNueva - columnaAnterior;
            int deltaY = filaNueva - filaAnterior;

            // Calcula el ángulo en grados
            double angulo = Math.Atan2(deltaY, deltaX) * (180 / Math.PI);
            return angulo;
        }

        private double CalcularAnguloRotacion(int x1, int y1, int x2, int y2)
        {
            int dx = x2 - x1;
            int dy = y2 - y1;

            if (dx == 1 && dy == 0) return 90;   // Este
            if (dx == -1 && dy == 0) return 270; // Oeste
            if (dx == 0 && dy == 1) return 180;  // Sur
            if (dx == 0 && dy == -1) return 0;   // Norte
            if (dx == 1 && dy == -1) return 45;  // Noreste
            if (dx == 1 && dy == 1) return 135;  // Sureste
            if (dx == -1 && dy == -1) return 315; // Noroeste
            if (dx == -1 && dy == 1) return 225;  // Suroeste
            return 0; // Default a norte
        }



        /// <summary>
        /// Dibujado de mapa y Cargado de imagenes
        /// </summary>
        private void CargarImagenes()
        {
            // Cargar las imágenes desde la carpeta de Imagenes
            imgPortaviones = new BitmapImage(new Uri("Imagenes/portaviones.png", UriKind.Relative));
            imgAeropuerto = new BitmapImage(new Uri("Imagenes/Aeropuerto.png", UriKind.Relative));
            imgAvion = new BitmapImage(new Uri("Imagenes/Avion.png", UriKind.Relative));
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

        private void GenerarEstructuras()
        {
            Random random = new Random();

            // Generar un aeropuerto en un nodo de tierra que no esté en los bordes
            bool aeropuertoGenerado = false;
            while (!aeropuertoGenerado)
            {
                int x = random.Next(1, 29); // Rango de 1 a 28 para evitar los bordes
                int y = random.Next(1, 29);

                Nodo nodo = matriz.Matrix[x, y];
                if (nodo.Terreno == TipoTerreno.Tierra && !nodo.TieneAeropuerto)
                {
                    aeropuerto = new Aeropuerto(nodo);
                    nodo.TieneAeropuerto = true;
                    aeropuertoGenerado = true;
                }
            }

            // Generar un portaviones en un nodo de agua que no esté en los bordes
            bool portavionesGenerado = false;
            while (!portavionesGenerado)
            {
                int x = random.Next(1, 29); // Rango de 1 a 28 para evitar los bordes
                int y = random.Next(1, 29);

                Nodo nodo = matriz.Matrix[x, y];
                if (nodo.Terreno == TipoTerreno.Mar && !nodo.TienePortaviones)
                {
                    portaviones = new Portaviones(nodo);
                    nodo.TienePortaviones = true;
                    portavionesGenerado = true;
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

                    // Terreno
                    Rectangle rect = new Rectangle
                    {
                        Width = CellSize,
                        Height = CellSize,
                        Fill = nodo.Terreno == TipoTerreno.Tierra ? Brushes.Green : Brushes.Blue
                    };

                    Canvas.SetLeft(rect, j * CellSize);
                    Canvas.SetTop(rect, i * CellSize);
                    MapaCanvas.Children.Add(rect);

                    // Agrega las imágenes de las estructuras si es necesario
                    if (nodo.TieneAeropuerto)
                    {
                        Image img = new Image
                        {
                            Width = CellSize + 20,
                            Height = CellSize + 20,
                            Source = imgAeropuerto
                        };
                        Canvas.SetLeft(img, j * CellSize - 5);
                        Canvas.SetTop(img, i * CellSize - 5);
                        MapaCanvas.Children.Add(img);
                    }
                    else if (nodo.TienePortaviones)
                    {
                        Image img = new Image
                        {
                            Width = CellSize + 10,
                            Height = CellSize + 10,
                            Source = imgPortaviones
                        };
                        Canvas.SetLeft(img, j * CellSize - 5);
                        Canvas.SetTop(img, i * CellSize - 5);
                        MapaCanvas.Children.Add(img);
                    }
                }
            }

            // Añadir imgAvionElement al Canvas en su posición inicial
            if (avion != null && avion.NodoActual != null)
            {
                int filaAvion = matriz.GetRow(avion.NodoActual);
                int columnaAvion = matriz.GetColumn(avion.NodoActual);

                Canvas.SetLeft(imgAvionElement, columnaAvion * CellSize);
                Canvas.SetTop(imgAvionElement, filaAvion * CellSize);
                if (!MapaCanvas.Children.Contains(imgAvionElement))
                {
                    MapaCanvas.Children.Add(imgAvionElement);
                }
            }
        }
    }
}