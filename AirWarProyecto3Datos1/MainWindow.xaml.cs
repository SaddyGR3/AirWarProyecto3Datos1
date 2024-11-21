using AirWarProyecto3Datos1.Estructuras;
using AirWarProyecto3Datos1.LogicaCentral;
using AirWarProyecto3Datos1.Airplane;
using AirWarProyecto3Datos1.Player;
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
        private BitmapImage imgExplosion;
        private BitmapImage imgJugador;
        private Image imgAvionElement;
        private Image imgJugadorElement;
        private BitmapImage imgBala;
        private Image imgBalaElement;
        
        //Instancias
        private List<Aeropuerto> aeropuertos;
        private List<Portaviones> portaviones;
        private List<Nodo> destinosPosibles;
        private List<Avion> avionesActivos = new List<Avion>();
        private Jugador jugador;
        private Avion avion;
        private Bala bala;
        

        private DispatcherTimer timer;
        private DispatcherTimer timerCreacionAvion;

        private List<Nodo> posicionesAeropuertos;
        private List<Nodo> posicionesPortaviones;
        // Lista para almacenar imágenes de cada avión en el mapa
        private Dictionary<Avion, Image> imagenesAviones = new Dictionary<Avion, Image>();
        //balas 
        private DateTime tiempoInicioDisparo;
        private bool disparando = false;
        private List<Bala> balasActivas = new List<Bala>();
        private DispatcherTimer timerBalas = new DispatcherTimer();
        private Dictionary<Bala, Image> imagenesBalas = new Dictionary<Bala, Image>();

        public MainWindow()
        {
            //Aspectos de la Ventana
            InitializeComponent();
            matriz = new Matriz(30, 30); // Matriz de 30x30
            GenerarTerrenoPerlin();
            CargarImagenes(); // Cargar las imágenes una sola vez
            Nodo ubicacionInicial = matriz.Matrix[matriz.Matrix.GetLength(0) - 1, matriz.Matrix.GetLength(1) / 2];
            jugador = new Jugador(ubicacionInicial);

            DibujarJugador();
            
            this.KeyDown += Window_KeyDown;
            imgJugadorElement = new Image
            {
                Width = CellSize,
                Height = CellSize,
                Source = imgJugador // BitmapImage cargado previamente
            };

            int fila = matriz.GetRow(jugador.Ubicacion);
            int columna = matriz.GetColumn(jugador.Ubicacion);

            Canvas.SetLeft(imgJugadorElement, columna * CellSize);
            Canvas.SetTop(imgJugadorElement, fila * CellSize);

            MapaCanvas.Children.Add(imgJugadorElement);
            Panel.SetZIndex(imgJugadorElement, int.MaxValue);


            
            

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
            //GenerarAvion();

            // Temporizador para creación de aviones
            timerCreacionAvion = new DispatcherTimer();
            timerCreacionAvion.Interval = TimeSpan.FromSeconds(3);
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
            timerBalas= new DispatcherTimer();
            timerBalas.Interval = TimeSpan.FromMilliseconds(100);
            timerBalas.Tick += (s, e) => MoverBalas();
            timerBalas.Start();

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
            if (avionesActivos.Count == 0) return;

            List<Avion> avionesParaEliminar = new List<Avion>();

            foreach (var avion in avionesActivos)
            {
                if (avion == null || !avion.Activo) continue; // Seguridad adicional

                Nodo nodoAnterior = avion.NodoActual;
                avion.MoverAvion();

                if (avion.Combustible <= 0)
                {
                    // Mostrar explosión
                    MostrarExplosion(avion.NodoActual);

                    // Marcar el avión como inactivo
                    avionesParaEliminar.Add(avion);

                    // Eliminar la imagen del avión
                    if (imagenesAviones.ContainsKey(avion))
                    {
                        MapaCanvas.Children.Remove(imagenesAviones[avion]);
                        imagenesAviones.Remove(avion);
                    }
                }
                else if (imagenesAviones.ContainsKey(avion))
                {
                    ActualizarAnimacionAvion(avion, nodoAnterior);
                }
            }

            // Eliminar aviones destruidos de la lista activa
            foreach (var avion in avionesParaEliminar)
            {
                avionesActivos.Remove(avion);
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

        private void MostrarExplosion(Nodo nodo)
        {
            System.Diagnostics.Debug.WriteLine("Mostrando explosión en el nodo...");

            if (imgExplosion == null)
            {
                System.Diagnostics.Debug.WriteLine("La imagen de explosión no está cargada.");
                return;
            }

            int fila = matriz.GetRow(nodo);
            int columna = matriz.GetColumn(nodo);

            // Crear una nueva imagen para la explosión
            Image imgExplosionInstance = new Image
            {
                Width = CellSize + 10,
                Height = CellSize + 10,
                Source = imgExplosion
            };
            Panel.SetZIndex(imgExplosionInstance, int.MaxValue);

            // Posicionar la imagen en el Canvas
            Canvas.SetLeft(imgExplosionInstance, columna * CellSize - 5);
            Canvas.SetTop(imgExplosionInstance, fila * CellSize - 5);

            // Agregar al Canvas
            MapaCanvas.Children.Add(imgExplosionInstance);
            MapaCanvas.InvalidateVisual(); // Forzar el refresco de la interfaz

            // Temporizador para eliminar la explosión después de 2 segundos
            DispatcherTimer timerExplosion = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };

            timerExplosion.Tick += (s, e) =>
            {
                MapaCanvas.Children.Remove(imgExplosionInstance);
                timerExplosion.Stop();
            };

            timerExplosion.Start();
        }

        private Point ObtenerCoordenadasCanvas(Nodo nodo)
        {
            int fila = matriz.GetRow(nodo);
            int columna = matriz.GetColumn(nodo);
            // Asegúrate de que TamañoCelda esté definido correctamente
            const int TamañoCelda = 30; // Ajustar según el tamaño de las celdas en tu juego
            return new Point(columna * TamañoCelda, fila * TamañoCelda);
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
            imgExplosion = new BitmapImage(new Uri("Imagenes/explosion.png", UriKind.Relative));
            System.Diagnostics.Debug.WriteLine("Imagen de la explosión cargada: " + (imgExplosion != null));
            imgJugador = imgExplosion = new BitmapImage(new Uri("Imagenes/Jugador.png", UriKind.Relative));
            System.Diagnostics.Debug.WriteLine("Imagen del jugador cargada: " + (imgJugador != null));
            imgBala= imgExplosion = new BitmapImage(new Uri("Imagenes/Bala.png", UriKind.Relative));
            System.Diagnostics.Debug.WriteLine("Imagen de la bala cargada: " + (imgBala != null));

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
        private void DibujarJugador()
        {
            if (imgJugadorElement == null)
            {
                imgJugadorElement = new Image
                {
                    Width = CellSize,
                    Height = CellSize,
                    Source = imgJugador
                };
            }

            // Calcula la posición
            int fila = matriz.GetRow(jugador.Ubicacion);
            int columna = matriz.GetColumn(jugador.Ubicacion);
            System.Diagnostics.Debug.WriteLine($"Dibujando jugador en fila={fila}, columna={columna}");

            // Configura la posición en el Canvas
            Canvas.SetLeft(imgJugadorElement, columna * CellSize);
            Canvas.SetTop(imgJugadorElement, fila * CellSize);

            // Asegúrate de agregar al Canvas
            if (!MapaCanvas.Children.Contains(imgJugadorElement))
            {
                MapaCanvas.Children.Add(imgJugadorElement);
            }

            // Asegúrate de que esté visible y al frente
            Panel.SetZIndex(imgJugadorElement, int.MaxValue);
            MapaCanvas.InvalidateVisual();
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
            if (jugador != null && jugador.Ubicacion != null)
            {
                int filaJugador = matriz.GetRow(jugador.Ubicacion);
                int columnaJugador = matriz.GetColumn(jugador.Ubicacion);

                Canvas.SetLeft(imgJugadorElement, columnaJugador * CellSize);
                Canvas.SetTop(imgJugadorElement, filaJugador * CellSize);
                if (!MapaCanvas.Children.Contains(imgJugadorElement))
                {
                    MapaCanvas.Children.Add(imgJugadorElement);
                }
            }
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            Nodo nodoAnterior = jugador.Ubicacion;

            switch (e.Key)
            {
                case Key.Left:
                    jugador.MoverIzquierda();
                    
                    DibujarJugador(); // Actualiza la representación gráfica del jugador

                    
                    break;
                case Key.Right:
                    jugador.MoverDerecha();
                    
                    DibujarJugador();
                    
                    break;
                case Key.Space:
                    if (!disparando)
                    {
                        disparando = true;
                        tiempoInicioDisparo = DateTime.Now; // Guarda el tiempo cuando se presionó el espacio
                        
                    }
                    break;
            }
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (e.Key == Key.Space && disparando)
            {
                disparando = false;
                TimeSpan duracionPresion = DateTime.Now - tiempoInicioDisparo;

                // Calcular la velocidad en función del tiempo de presión
                // El tiempo de presión se utiliza para ajustar la velocidad de la bala.
                int velocidadBala = Math.Max(50, Math.Min(200, 200 - (int)duracionPresion.TotalMilliseconds));


                // Crear la bala con la velocidad calculada
                Bala nuevaBala = new Bala(jugador.Ubicacion, matriz, velocidadBala);
                balasActivas.Add(nuevaBala);  // Añadir la bala a las activas
                DibujarBala(nuevaBala);  // Dibuja la bala en la pantalla
            }
        }




        // Método para dibujar la bala
        // Método para mover las balas
        private void MoverBalas()
        {
            if (balasActivas.Count == 0) return;

            foreach (var bala in balasActivas.ToList()) // Usamos ToList() para evitar modificar la lista mientras la iteramos
            {
                if (bala == null || !bala.Activo)
                {
                    EliminarBalaDeVista(bala); // Eliminar la bala de la vista si ya no está activa
                    continue;
                }

                // Mueve la bala según su velocidad
                bala.MoverBala();

                // Actualiza su posición en el Canvas
                DibujarBala(bala);
            }
        }

        private void EliminarBalaDeVista(Bala bala)
        {
            if (imagenesBalas.ContainsKey(bala))
            {
                // Eliminar la imagen de la pantalla
                Image imgBalaElement = imagenesBalas[bala];
                MapaCanvas.Children.Remove(imgBalaElement); // Elimina la imagen del Canvas

                // Eliminar la bala de la lista de imágenes
                imagenesBalas.Remove(bala);
            }

            // Eliminar la bala de la lista de balas activas
            balasActivas.Remove(bala);
        }




        // Método para dibujar la bala (actualiza su posición en el Canvas)
        private void DibujarBala(Bala bala)
        {
            // Si la bala ya tiene una imagen, actualizar su posición
            if (!imagenesBalas.ContainsKey(bala))
            {
                // Crear una nueva imagen para la bala si no existe
                imgBalaElement = new Image
                {
                    Width = CellSize,
                    Height = CellSize,
                    Source = imgBala // Imagen de la bala previamente cargada
                };

                int fila = matriz.GetRow(bala.NodoActual);
                int columna = matriz.GetColumn(bala.NodoActual);

                Canvas.SetLeft(imgBalaElement, columna * CellSize);
                Canvas.SetTop(imgBalaElement, fila * CellSize);

                MapaCanvas.Children.Add(imgBalaElement); // Añadir la imagen al canvas

                imagenesBalas[bala] = imgBalaElement; // Guardar la imagen para su eliminación futura
            }
            else
            {
                // Si la bala ya tiene imagen, actualizar su posición
                imgBalaElement = imagenesBalas[bala];

                int fila = matriz.GetRow(bala.NodoActual);
                int columna = matriz.GetColumn(bala.NodoActual);

                Canvas.SetLeft(imgBalaElement, columna * CellSize);
                Canvas.SetTop(imgBalaElement, fila * CellSize);
            }
        }







    }
}