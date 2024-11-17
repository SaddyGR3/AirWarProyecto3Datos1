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
        private List<Aeropuerto> aeropuertos;
        private List<Portaviones> portaviones;
        private List<Nodo> destinosPosibles;
        private List<Avion> avionesActivos = new List<Avion>();
        private Avion avion;

        private DispatcherTimer timer;
        private DispatcherTimer timerCreacionAvion;

        private List<Nodo> posicionesAeropuertos;
        private List<Nodo> posicionesPortaviones;
        // Lista para almacenar imágenes de cada avión en el mapa
        private Dictionary<Avion, Image> imagenesAviones = new Dictionary<Avion, Image>();


        public MainWindow()
        {
            //Aspectos de la Ventana
            InitializeComponent();
            matriz = new Matriz(30, 30); // Matriz de 30x30
            GenerarTerrenoPerlin();
            CargarImagenes(); // Cargar las imágenes una sola vez

            aeropuertos = new List<Aeropuerto>();
            portaviones = new List<Portaviones>();
            // Crear la instancia del imgAvionElement y asignarle el BitmapImage de la imagen del avión
            imgAvionElement = new Image
            {
                Width = CellSize,
                Height = CellSize,
                Source = imgAvion 
            };

            // Agrega imgAvionElement al Canvas al inicio
            if (!MapaCanvas.Children.Contains(imgAvionElement))
            {
                MapaCanvas.Children.Add(imgAvionElement);
            }

            posicionesAeropuertos = new List<Nodo>();
            posicionesPortaviones = new List<Nodo>();
            // Generar aeropuerto y portaviones antes de dibujar el mapa
            GenerarEstructuras();
            GenerarAvion();

            // Temporizador para creación de aviones
            timerCreacionAvion = new DispatcherTimer();
            timerCreacionAvion.Interval = TimeSpan.FromSeconds(10);
            timerCreacionAvion.Tick += (s, e) => GenerarAvion();
            timerCreacionAvion.Start();

            // Agrega la transformación de rotación
            imgAvionElement.RenderTransform = new RotateTransform(0);
            imgAvionElement.RenderTransformOrigin = new Point(0.5, 0.5);

            // Temporizador de animación de movimiento
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += (s, e) => MoverAviones();
            timer.Start();


            DibujarMapa();
        }

        /// </summary>
        /// Logica del Avion
        /// </summary>
        private void GenerarAvion()
        {
            System.Diagnostics.Debug.WriteLine("Intentando generar un avión en algún aeropuerto...");

            foreach (var aeropuerto in aeropuertos)
            {
                if (aeropuerto.HayEspacioEnHangar() && aeropuerto.PuedeConstruirAvion())
                {
                    Avion nuevoAvion = aeropuerto.CrearAvion(matriz, destinosPosibles); // Crear el avión
                    if (nuevoAvion != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Avión creado en aeropuerto: {aeropuerto.Nombre}");
                        avionesActivos.Add(nuevoAvion);
                        // Asignar el destino automáticamente y despegar
                        nuevoAvion.AsignarDestinoAleatorio();
                        aeropuerto.DespegarAvion(nuevoAvion);

                        // Dibujar el avión en el mapa
                        DibujarAvion(nuevoAvion);

                        return; // Salimos del método tras crear y despegar un avión
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine("No se creó el avión: ningún aeropuerto disponible.");
        }
        private void DibujarAvion(Avion avion)
        {
            // Crear una nueva imagen para este avión
            Image imgAvionInstance = new Image
            {
                Width = CellSize,
                Height = CellSize,
                Source = imgAvion,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform(0)
            };

            int fila = matriz.GetRow(avion.NodoActual);
            int columna = matriz.GetColumn(avion.NodoActual);

            Canvas.SetLeft(imgAvionInstance, columna * CellSize);
            Canvas.SetTop(imgAvionInstance, fila * CellSize);

            MapaCanvas.Children.Add(imgAvionInstance);

            // Asociar esta imagen con el avión en el diccionario
            imagenesAviones[avion] = imgAvionInstance;
        }



        private void MoverAviones()
        {
            if (avionesActivos.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("No hay aviones activos para mover.");
                return;
            }

            foreach (var avion in avionesActivos)
            {
                if (avion == null || !avion.Activo) continue; // Seguridad adicional

                Nodo nodoAnterior = avion.NodoActual;
                avion.MoverAvion();

                if (imagenesAviones.ContainsKey(avion))
                {
                    ActualizarAnimacionAvion(avion, nodoAnterior);
                }
            }
        }

        private void ActualizarAnimacionAvion(Avion avion, Nodo nodoAnterior)
        {
            int filaAnterior = matriz.GetRow(nodoAnterior);
            int columnaAnterior = matriz.GetColumn(nodoAnterior);
            int filaNueva = matriz.GetRow(avion.NodoActual);
            int columnaNueva = matriz.GetColumn(avion.NodoActual);

            var imgAvionElement = imagenesAviones[avion];
            double angle = CalcularAnguloRotacion(columnaAnterior, filaAnterior, columnaNueva, filaNueva);
            RotateTransform rotateTransform = imgAvionElement.RenderTransform as RotateTransform;

            // Animación de rotación
            DoubleAnimation animRotation = new DoubleAnimation
            {
                To = angle,
                Duration = TimeSpan.FromMilliseconds(250)
            };
            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, animRotation);

            // Animación de movimiento en X
            DoubleAnimation animX = new DoubleAnimation
            {
                From = columnaAnterior * CellSize,
                To = columnaNueva * CellSize,
                Duration = TimeSpan.FromMilliseconds(500)
            };
            imgAvionElement.BeginAnimation(Canvas.LeftProperty, animX);

            // Animación de movimiento en Y
            DoubleAnimation animY = new DoubleAnimation
            {
                From = filaAnterior * CellSize,
                To = filaNueva * CellSize,
                Duration = TimeSpan.FromMilliseconds(500)
            };
            imgAvionElement.BeginAnimation(Canvas.TopProperty, animY);
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
            System.Diagnostics.Debug.WriteLine("Imagen del avión cargada: " + (imgAvion != null));
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

        // Modificación en el método GenerarEstructuras para generar 2 aeropuertos y 2 portaviones:
        private void GenerarEstructuras()
        {
            Random random = new Random();
            int aeropuertosGenerados = 0;
            int portavionesGenerados = 0;

            destinosPosibles = new List<Nodo>();
            aeropuertos.Clear();
            portaviones.Clear();

            // Generar aeropuertos en nodos de tierra
            while (aeropuertosGenerados < 2)
            {
                int x = random.Next(1, 29);
                int y = random.Next(1, 29);
                Nodo nodo = matriz.Matrix[x, y];
                if (nodo.Terreno == TipoTerreno.Tierra && !nodo.TieneAeropuerto)
                {
                    var nuevoAeropuerto = new Aeropuerto(nodo);
                    nodo.TieneAeropuerto = true;
                    aeropuertos.Add(nuevoAeropuerto);
                    destinosPosibles.Add(nodo); // Agregar a la lista única
                    aeropuertosGenerados++;
                }
            }

            // Generar portaviones en nodos de agua
            while (portavionesGenerados < 2)
            {
                int x = random.Next(1, 29);
                int y = random.Next(1, 29);
                Nodo nodo = matriz.Matrix[x, y];
                if (nodo.Terreno == TipoTerreno.Mar && !nodo.TienePortaviones)
                {
                    var nuevoPortaviones = new Portaviones(nodo);
                    nodo.TienePortaviones = true;
                    portaviones.Add(nuevoPortaviones);
                    destinosPosibles.Add(nodo); // Agregar a la lista única
                    portavionesGenerados++;
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