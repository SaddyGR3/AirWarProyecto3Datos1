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
using System.Collections.Generic;
using System.IO;


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

        private List<NodoGrafo> nodosGrafo = new List<NodoGrafo>();

        //balas 
        private DateTime tiempoInicioDisparo;
        private bool disparando = false;
        private List<Bala> balasActivas = new List<Bala>();
        private List<Bala> balasAEliminar = new List<Bala>();
        private DispatcherTimer timerBalas = new DispatcherTimer();
        private Dictionary<Bala, Image> imagenesBalas = new Dictionary<Bala, Image>();
        private int AvionesDerribados;
        private DispatcherTimer TimerJuego = new DispatcherTimer();
        private Dictionary<Aeropuerto, List<Tuple<Aeropuerto, int>>> rutasAeropuertos;


        private List<object> estructuras = new List<object>(); // Contiene tanto aeropuertos como portaviones
        private Dictionary<object, Dictionary<object, int>> rutas = new Dictionary<object, Dictionary<object, int>>();

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
            AvionesDerribados = 0;
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
            timerBalas = new DispatcherTimer();
            timerBalas.Interval = TimeSpan.FromMilliseconds(100);
            timerBalas.Tick += (s, e) => MoverBalas();
            timerBalas.Start();
            TimerJuego = new DispatcherTimer();
            TimerJuego.Interval = TimeSpan.FromMilliseconds(90000);
            TimerJuego.Tick += (s, e) => DetenerJuego();
            TimerJuego.Start();

            DibujarMapa();



        }
        private void DetenerJuego()
        {
            timerCreacionAvion.Stop();
            TimerJuego.Stop();
            timer.Stop();
            timerBalas.Stop();
            MessageBox.Show($"¡El juego terminó! Derribaste {AvionesDerribados} aviones.",
                    "Fin del Juego",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            System.Diagnostics.Debug.WriteLine($"el juego termino derribaste {AvionesDerribados} aviones");
        }
        /// </summary>
        /// Logica del Avion
        /// </summary>
        private void GenerarAvion()

        {
            //System.Diagnostics.Debug.WriteLine("Intentando generar un avión en algún aeropuerto...");

            foreach (var aeropuerto in aeropuertos)
            {
                if (aeropuerto.HayEspacioEnHangar() && aeropuerto.PuedeConstruirAvion())
                {
                    Avion nuevoAvion = aeropuerto.CrearAvion(matriz, destinosPosibles, rutas); // Crear el avión
                    if (nuevoAvion != null)
                    {
                        //System.Diagnostics.Debug.WriteLine($"Avión creado en aeropuerto: {aeropuerto.Nombre}");
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

            //System.Diagnostics.Debug.WriteLine("No se creó el avión: ningún aeropuerto disponible.");
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
            imgJugador = new BitmapImage(new Uri("Imagenes/Jugador.png", UriKind.Relative));
            System.Diagnostics.Debug.WriteLine("Imagen del jugador cargada: " + (imgJugador != null));
            imgBala = new BitmapImage(new Uri("Imagenes/Bala.png", UriKind.Relative));
            System.Diagnostics.Debug.WriteLine("Imagen de la bala cargada: " + (imgBala != null));

        }

        public void GenerarTerrenoPerlin()
        {
            PerlinNoise perlin = new PerlinNoise(); // Genera una nueva semilla aleatoria cada vez
            for (int i = 0; i < 30; i++)
            {
                for (int j = 0; j < 30; j++)
                {
                    Nodo nodo = matriz.Matrix[i, j];
                    double noiseValue = perlin.Generate(i * 0.1, j * 0.1); // Escala para ajustar el "zoom"

                    // Determina el tipo de terreno
                    nodo.Terreno = noiseValue > 0 ? TipoTerreno.Tierra : TipoTerreno.Mar;

                    // Asigna el PesoRuta según el tipo de terreno
                    if (nodo.Terreno == TipoTerreno.Tierra)
                    {
                        nodo.PesoRuta = 1; // Tierra
                    }
                    else if (nodo.Terreno == TipoTerreno.Mar)
                    {
                        nodo.PesoRuta = 8000; // Mar
                    }

                    // Ajusta el PesoRuta si el nodo tiene una estructura
                    if (nodo.TieneAeropuerto)
                    {
                        nodo.PesoRuta = 4; // Aeropuerto
                    }
                    else if (nodo.TienePortaviones)
                    {
                        nodo.PesoRuta = 8; // Portaviones
                    }
                }
            }
        }

        // Generar estructuras (Aeropuertos y Portaviones)
        private void GenerarEstructuras()
        {
            Random random = new Random();
            int aeropuertosGenerados = 0;
            int portavionesGenerados = 0;

            destinosPosibles = new List<Nodo>();
            aeropuertos.Clear();
            portaviones.Clear();
            estructuras.Clear();

            // Generar aeropuertos en nodos de tierra
            while (aeropuertosGenerados < 2)
            {
                int x = random.Next(1, 29);
                int y = random.Next(1, 29);
                Nodo nodo = matriz.Matrix[x, y];
                if (nodo.Terreno == TipoTerreno.Tierra && !nodo.TieneAeropuerto)
                {
                    var nuevoAeropuerto = new Aeropuerto(nodo);
                    aeropuertos.Add(nuevoAeropuerto);
                    estructuras.Add(nuevoAeropuerto);
                    destinosPosibles.Add(nodo); // Agregar como destino válido
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
                    portaviones.Add(nuevoPortaviones);
                    estructuras.Add(nuevoPortaviones);
                    destinosPosibles.Add(nodo); // Agregar como destino válido
                    portavionesGenerados++;
                }
            }

            GenerarRutasEntreEstructuras();
            foreach (var estructura in estructuras)
            {
                System.Diagnostics.Debug.WriteLine($"Estructura en nodo: {estructura}");
            }
        }

        // Calcular peso entre dos nodos (similar al cálculo de rutas rectas)
        private int CalcularPesoRuta(Nodo origen, Nodo destino)
        {
            int filaInicio = matriz.GetRow(origen);
            int columnaInicio = matriz.GetColumn(origen);
            int filaDestino = matriz.GetRow(destino);
            int columnaDestino = matriz.GetColumn(destino);

            var ruta = CalcularRutaRecta(filaInicio, columnaInicio, filaDestino, columnaDestino);
            return ruta.Sum(n => n.PesoRuta);
        }

        // Dibujar rutas entre estructuras
        private void DibujarRutas()
        {
            foreach (var origen in rutas.Keys)
            {
                var nodoOrigen = ObtenerNodo(origen); // Nodo inicial

                foreach (var destino in rutas[origen].Keys)
                {
                    var nodoDestino = ObtenerNodo(destino); // Nodo final

                    // Calcular la ruta como lista de nodos
                    var ruta = CalcularRutaRecta(
                        matriz.GetRow(nodoOrigen),
                        matriz.GetColumn(nodoOrigen),
                        matriz.GetRow(nodoDestino),
                        matriz.GetColumn(nodoDestino)
                    );

                    // Agregar nodo inicial si no está incluido en la ruta
                    if (ruta.Count == 0 ||
                        (matriz.GetRow(ruta[0]) != matriz.GetRow(nodoOrigen) ||
                         matriz.GetColumn(ruta[0]) != matriz.GetColumn(nodoOrigen)))
                    {
                        ruta.Insert(0, nodoOrigen);
                    }

                    // Obtener el peso de la ruta
                    int pesoRuta = rutas[origen][destino];

                    // Dibujar la ruta y el peso en el nodo medio
                    DibujarLineaRuta(nodoOrigen, nodoDestino, Brushes.Gray, ruta, pesoRuta);
                }
            }
        }

        // Dibujar línea entre dos nodos en el canvas
        private void DibujarLineaRuta(Nodo nodoInicio, Nodo nodoFin, Brush color, List<Nodo> ruta, int peso)
        {
            EscribirRutasEnArchivo();
            // Dibujar líneas entre nodos consecutivos
            for (int i = 0; i < ruta.Count - 1; i++)
            {
                var inicio = ruta[i];
                var fin = ruta[i + 1];

                // Obtener filas y columnas de los nodos
                int filaInicio = matriz.GetRow(inicio);
                int columnaInicio = matriz.GetColumn(inicio);
                int filaFin = matriz.GetRow(fin);
                int columnaFin = matriz.GetColumn(fin);

                // Convertir filas y columnas a coordenadas del canvas
                double x1 = columnaInicio * CellSize + CellSize / 2;
                double y1 = filaInicio * CellSize + CellSize / 2;
                double x2 = columnaFin * CellSize + CellSize / 2;
                double y2 = filaFin * CellSize + CellSize / 2;

                // Crear la línea
                Line linea = new Line
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2,
                    Stroke = color,
                    StrokeThickness = 2,
                    Opacity = 0.8
                };

                // Añadir la línea al canvas
                MapaCanvas.Children.Add(linea);
            }

            // Dibujar el peso solo en el nodo central
            if (ruta.Count > 0)
            {
                int indiceCentral = ruta.Count / 2;
                var nodoMedio = ruta[indiceCentral];

                // Coordenadas del nodo medio
                int filaMedio = matriz.GetRow(nodoMedio);
                int columnaMedio = matriz.GetColumn(nodoMedio);
                double textoX = columnaMedio * CellSize + CellSize / 2;
                double textoY = filaMedio * CellSize + CellSize / 2;

                // Crear el TextBlock para mostrar el peso
                TextBlock textoPeso = new TextBlock
                {
                    Text = peso.ToString(),
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold,
                };

                // Posicionar el texto en el Canvas
                Canvas.SetLeft(textoPeso, textoX);
                Canvas.SetTop(textoPeso, textoY);
                MapaCanvas.Children.Add(textoPeso);
            }
        }

        // Obtener el nodo desde una estructura (Aeropuerto o Portaviones)
        private Nodo ObtenerNodo(object estructura)
        {
            if (estructura is Aeropuerto aeropuerto) return aeropuerto.Ubicacion;
            if (estructura is Portaviones portaviones) return portaviones.Ubicacion;
            throw new InvalidOperationException("Estructura desconocida");
        }
        // Generar rutas entre todas las estructuras
        private void GenerarRutasEntreEstructuras()
        {
            rutas.Clear();
            foreach (var origen in estructuras)
            {
                var nodoOrigen = ObtenerNodo(origen);
                rutas[origen] = new Dictionary<object, int>();

                foreach (var destino in estructuras)
                {
                    if (origen == destino) continue;

                    var nodoDestino = ObtenerNodo(destino);
                    int peso = CalcularPesoRuta(nodoOrigen, nodoDestino);

                    rutas[origen][destino] = peso;
                }
            }



            foreach (var origen in rutas.Keys)
            {
                foreach (var destino in rutas[origen].Keys)
                {
                    System.Diagnostics.Debug.WriteLine($"Ruta: {origen} -> {destino}, Peso: {rutas[origen][destino]}");
                }
            }
        }

        private List<Nodo> CalcularRutaRecta(int filaInicio, int columnaInicio, int filaDestino, int columnaDestino)
        {
            List<Nodo> ruta = new List<Nodo>();
            int filaActual = filaInicio;
            int columnaActual = columnaInicio;

            while (filaActual != filaDestino || columnaActual != columnaDestino)
            {
                if (filaActual < filaDestino) filaActual++;
                else if (filaActual > filaDestino) filaActual--;

                if (columnaActual < columnaDestino) columnaActual++;
                else if (columnaActual > columnaDestino) columnaActual--;

                Nodo siguienteNodo = matriz.GetNode(filaActual, columnaActual);
                ruta.Add(siguienteNodo);

            }

            // Asegurar que el nodo final esté incluido
            Nodo nodoFinal = matriz.GetNode(filaDestino, columnaDestino);
            if (!ruta.Contains(nodoFinal))
            {
                ruta.Add(nodoFinal);
            }

            return ruta;
        }

        private void EscribirRutasEnArchivo()
        {

            string filePath = @"C:\Users\saddy\Desktop\rutas.txt";

            try
            {
                // Resolver ruta absoluta
                string absolutePath = System.IO.Path.GetFullPath(filePath);
                System.Diagnostics.Debug.WriteLine($"Ruta absoluta del archivo: {absolutePath}");

                // Crear la carpeta si no existe
                string directoryPath = System.IO.Path.GetDirectoryName(absolutePath);
                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    System.Diagnostics.Debug.WriteLine($"Directorio creado: {directoryPath}");
                }

                // Crear o sobrescribir el archivo
                using (StreamWriter writer = new StreamWriter(absolutePath, false))
                {
                    if (rutas.Count == 0)
                    {
                        writer.WriteLine("No hay rutas definidas.");
                        System.Diagnostics.Debug.WriteLine("El diccionario de rutas está vacío.");
                        return;
                    }

                    foreach (var origen in rutas.Keys)
                    {
                        string descripcionOrigen = ObtenerDescripcionEstructura(origen) ?? "Sin descripción";
                        writer.WriteLine($"Origen: {descripcionOrigen}");
                        System.Diagnostics.Debug.WriteLine($"Escribiendo origen: {descripcionOrigen}");

                        foreach (var destino in rutas[origen].Keys)
                        {
                            string descripcionDestino = ObtenerDescripcionEstructura(destino) ?? "Sin descripción";
                            int peso = rutas[origen][destino];
                            writer.WriteLine($"    Destino: {descripcionDestino}, Peso: {peso}");
                            System.Diagnostics.Debug.WriteLine($"    Destino: {descripcionDestino}, Peso: {peso}");
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Rutas guardadas correctamente en {absolutePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al escribir las rutas en el archivo: {ex.Message}");
            }
        }

        private string ObtenerDescripcionEstructura(object estructura)
        {
            if (estructura is Aeropuerto aeropuerto)
                return $"Aeropuerto en ({matriz.GetRow(aeropuerto.Ubicacion)}, {matriz.GetColumn(aeropuerto.Ubicacion)})";
            if (estructura is Portaviones portaviones)
                return $"Portaviones en ({matriz.GetRow(portaviones.Ubicacion)}, {matriz.GetColumn(portaviones.Ubicacion)})";
            return "Estructura desconocida";
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
            //System.Diagnostics.Debug.WriteLine($"Dibujando jugador en fila={fila}, columna={columna}");

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


        private void test()
        {
            for (int i = 0; i < 30; i++)
            {
                for (int j = 0; j < 30; j++)
                {
                    Nodo nodo = matriz.Matrix[i, j];
                    //System.Diagnostics.Debug.WriteLine($"Nodo ({i}, {j}): Terreno={nodo.Terreno}, PesoRuta={nodo.PesoRuta}");
                }
            }
        }


        private List<NodoGrafo> Dijkstra(NodoGrafo inicio, NodoGrafo destino)
        {
            var distancias = new Dictionary<NodoGrafo, double>();
            var previos = new Dictionary<NodoGrafo, NodoGrafo>();
            var nodosPendientes = new HashSet<NodoGrafo>(nodosGrafo);

            foreach (var nodo in nodosGrafo)
            {
                distancias[nodo] = double.MaxValue;
                previos[nodo] = null;
            }

            distancias[inicio] = 0;

            while (nodosPendientes.Count > 0)
            {
                var nodoActual = nodosPendientes.OrderBy(n => distancias[n]).First();
                nodosPendientes.Remove(nodoActual);

                if (nodoActual == destino)
                    break;

                foreach (var arista in nodoActual.Vecinos)
                {
                    if (!nodosPendientes.Contains(arista.Destino))
                        continue;

                    double nuevaDistancia = distancias[nodoActual] + arista.Peso;
                    if (nuevaDistancia < distancias[arista.Destino])
                    {
                        distancias[arista.Destino] = nuevaDistancia;
                        previos[arista.Destino] = nodoActual;
                    }
                }
            }

            // Reconstruir el camino
            var camino = new List<NodoGrafo>();
            var actual = destino;
            while (actual != null)
            {
                camino.Insert(0, actual);
                actual = previos[actual];
            }

            return camino;
        }
        private double CalcularDistancia(Nodo origen, Nodo destino)
        {
            int dx = matriz.GetRow(origen) - matriz.GetRow(destino);
            int dy = matriz.GetColumn(origen) - matriz.GetColumn(destino);
            return Math.Sqrt(dx * dx + dy * dy);
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
            DibujarRutas();
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



        //Controles
        //
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
                //System.Diagnostics.Debug.WriteLine("la velocidad de la bala es:", velocidadBala);
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

                DetectarColision();
                

                // Actualiza su posición en el Canvas
                DibujarBala(bala);
                MapaCanvas.InvalidateVisual();
            }
        }

        private void EliminarBalaDeVista(Bala bala)
        {
            if (imagenesBalas.ContainsKey(bala))
            {
                Image imgBalaElement = imagenesBalas[bala];
                MapaCanvas.Children.Remove(imgBalaElement); // Elimina la imagen del Canvas
                imagenesBalas.Remove(bala);
                System.Diagnostics.Debug.WriteLine($"Imagen de bala eliminada: {bala}");
            }

            // Eliminar la bala de la lista de balas activas
            if (balasActivas.Contains(bala))
            {
                balasActivas.Remove(bala);
                System.Diagnostics.Debug.WriteLine($"Bala eliminada de balasActivas: {bala}");
            }

            // Refresca el Canvas manualmente
            MapaCanvas.InvalidateVisual();
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
        private void DetectarColision()
        {
            
            var avionesAEliminar = new List<Avion>();

            foreach (var bala in balasActivas)
            {
                if (bala.ImpactarAvion())
                {
                    foreach (var avion in avionesActivos)
                    {
                        if (avion.NodoActual == bala.NodoActual) // Verificar si están en la misma casilla
                        {
                            AvionesDerribados++;
                            MostrarExplosion(avion.NodoActual);
                            avion.Destruir();
                            avionesAEliminar.Add(avion); // Marcar avión para eliminación
                            System.Diagnostics.Debug.WriteLine("Un avión debió destruirse");
                        }
                    }

                    balasAEliminar.Add(bala); // Marcar bala para eliminación
                }
            }

            // Eliminar las balas marcadas
            foreach (var bala in balasAEliminar)
            {
                System.Diagnostics.Debug.WriteLine("una bala debio borrarse");
                
                bala.DestruirBala();
                EliminarBalaDeVista(bala);
                //ActualizarBalas();
                MapaCanvas.InvalidateVisual();
            }

            // Eliminar los aviones marcados
            foreach (var avion in avionesAEliminar)
            {
                EliminarAvionDeVista(avion);
            }
            MapaCanvas.InvalidateVisual();
        }

        private void EliminarAvionDeVista(Avion avion)
        {
            if (imagenesAviones.ContainsKey(avion))
            {
                Image imgAvionElement = imagenesAviones[avion];
                MapaCanvas.Children.Remove(imgAvionElement); // Elimina la imagen del Canvas

                // Eliminar la avion de la lista de imágenes
                imagenesAviones.Remove(avion);
                System.Diagnostics.Debug.WriteLine("un avion debio borrarse");
            }
            avionesActivos.Remove(avion);
        }

        private void ActualizarBalas()
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
               
                MapaCanvas.InvalidateVisual();
            }
        }
    }
}