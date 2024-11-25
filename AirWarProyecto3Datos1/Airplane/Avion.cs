using AirWarProyecto3Datos1.Estructuras;
using AirWarProyecto3Datos1.LogicaCentral;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AirWarProyecto3Datos1.Airplane
{
    internal class Avion
    {
        public Nodo NodoActual { get; private set; }
        public List<Nodo> Ruta { get; private set; }
        public Nodo Destino { get; private set; }
        private Matriz matriz;
        public bool HaLlegadoADestino { get; private set; }
        public Aeropuerto AeropuertoActual { get; private set; }
        private List<Nodo> DestinosPosibles { get; set; }
        public Portaviones PortavionesActual { get; private set; }
        public bool Activo { get; private set; }
        public int Combustible { get; private set; } 
        private Dictionary<object, Dictionary<object, int>> RutasPredefinidas;
        public Pilot Pilot { get; private set; }
        public Copilot Copilot { get; private set; }
        public Maintenance Maintenance { get; private set; }
        public SpaceAwareness SpaceAwareness { get; private set; }
        public Guid ID { get; private set; }

        public Avion(Nodo nodoInicial, Matriz matriz, List<Nodo> destinosPosibles, Dictionary<object, Dictionary<object, int>> rutasPredefinidas)
        {
            NodoActual = nodoInicial;
            NodoActual.TieneAvion = true;
            DestinosPosibles = destinosPosibles;
            this.matriz = matriz;
            Activo = true;
            Ruta = new List<Nodo>();
            HaLlegadoADestino = false;
            Combustible = 1000; 
            RutasPredefinidas = rutasPredefinidas; // Almacena las rutas calculadas
            GenerarID();

            // Inicializa módulos de AI al crear el avión
            Pilot = new Pilot();
            Copilot = new Copilot();
            Maintenance = new Maintenance();
            SpaceAwareness = new SpaceAwareness();
        }
      
      
        public void GenerarID()
        {
            ID = Guid.NewGuid();
            System.Diagnostics.Debug.WriteLine($"el ID es {ID}");

        }

        // Asigna un destino aleatorio desde los disponibles
        public void AsignarDestinoAleatorio()
        {
            // Seleccionar un destino aleatorio
            var destinosDisponibles = DestinosPosibles
                .Where(destino => destino != NodoActual)
                .ToList();

            if (!destinosDisponibles.Any())
                throw new InvalidOperationException("No hay destinos disponibles.");

            Random random = new Random();
            int indiceDestino = random.Next(destinosDisponibles.Count);
            Destino = destinosDisponibles[indiceDestino];

            // Evalua rutas alternativas
            Ruta = CalcularRutaMasEconomica(NodoActual, Destino);

            System.Diagnostics.Debug.WriteLine($"Destino asignado: {Destino.Data}. Ruta calculada: {Ruta.Count} nodos.");
        }

        // Método para procesar aterrizaje y despegar automáticamente después de un tiempo
        public async Task ProcesarAterrizajeAsync()
        {
            if (!Activo) return; // Valida si el avión aún está activo

            if (Destino.TieneAeropuerto && Destino.Elemento is Aeropuerto aeropuertoDestino)
            {
                NodoActual.TieneAvion = false;
                aeropuertoDestino.AvionAterriza(this); // Modifica para pasar el avión actual
                System.Diagnostics.Debug.WriteLine($"El avión aterrizó en el aeropuerto {aeropuertoDestino.Nombre}.");
            }
            else if (Destino.TienePortaviones && Destino.Elemento is Portaviones portavionesDestino)
            {
                NodoActual.TieneAvion = false;
                portavionesDestino.AvionAterriza(this); // Modificar para pasar el avión actual
                System.Diagnostics.Debug.WriteLine($"El avión aterrizó en el portaviones {portavionesDestino.Nombre}.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("El destino no es un aeropuerto o portaviones válido.");
                return;
            }

            // Espera antes de despegar
            await Task.Delay(TimeSpan.FromSeconds(5));

            // Verifica nuevamente antes de asignar destino
            if (Activo)
            {
                AvisoDespegue();
            }
        }

        private List<Nodo> CalcularRutaMasEconomica(Nodo origen, Nodo destino)
        {
            List<Nodo> rutaMasEconomica = CalcularRutaRecta(matriz.GetRow(origen), matriz.GetColumn(origen), matriz.GetRow(destino), matriz.GetColumn(destino));
            int pesoRutaDirecta = ObtenerPesoRuta(origen, destino);

            foreach (var intermedio in DestinosPosibles)
            {
                if (intermedio == origen || intermedio == destino) continue;

                // Evalua peso usando el nodo intermedio
                int pesoViaIntermedio = ObtenerPesoRuta(origen, intermedio) + ObtenerPesoRuta(intermedio, destino);

                if (pesoViaIntermedio < pesoRutaDirecta)
                {
                    // Si es más económico, concatena rutas
                    List<Nodo> rutaViaIntermedio = new List<Nodo>();
                    rutaViaIntermedio.AddRange(CalcularRutaRecta(matriz.GetRow(origen), matriz.GetColumn(origen), matriz.GetRow(intermedio), matriz.GetColumn(intermedio)));
                    rutaViaIntermedio.AddRange(CalcularRutaRecta(matriz.GetRow(intermedio), matriz.GetColumn(intermedio), matriz.GetRow(destino), matriz.GetColumn(destino)));

                    rutaMasEconomica = rutaViaIntermedio;
                    pesoRutaDirecta = pesoViaIntermedio; // Actualizar el peso para comparar con otras rutas
                }
            }

            return rutaMasEconomica;
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

            // Asegura que el nodo final esté incluido
            Nodo nodoFinal = matriz.GetNode(filaDestino, columnaDestino);
            if (!ruta.Contains(nodoFinal))
            {
                ruta.Add(nodoFinal);
            }

            return ruta;
        }
        private int ObtenerPesoRuta(Nodo origen, Nodo destino)
        {
            if (RutasPredefinidas.TryGetValue(origen, out var destinos) && destinos.TryGetValue(destino, out var peso))
            {
                return peso;
            }

            return int.MaxValue; // Retorna un valor muy alto si no hay ruta definida
        }
        private List<Nodo> EvaluarRutaMasBarata(object origen, object destino)
        {
            var mejorRuta = new List<Nodo>();
            int menorCosto = int.MaxValue;

            // Cola para explorar caminos (nodo actual, ruta hasta ahora, costo acumulado)
            var cola = new Queue<(object nodoActual, List<object> ruta, int costo)>();
            cola.Enqueue((origen, new List<object> { origen }, 0));

            while (cola.Count > 0)
            {
                var (nodoActual, rutaHastaAhora, costoActual) = cola.Dequeue();

                // Si llegamos al destino, evalua si es la mejor ruta
                if (nodoActual == destino)
                {
                    if (costoActual < menorCosto)
                    {
                        menorCosto = costoActual;
                        mejorRuta = rutaHastaAhora.Select(n => ObtenerNodo(n)).ToList();
                    }
                    continue;
                }

                // Continua explorando nodos vecinos
                if (RutasPredefinidas.ContainsKey(nodoActual))
                {
                    foreach (var (nodoVecino, peso) in RutasPredefinidas[nodoActual])
                    {
                        if (!rutaHastaAhora.Contains(nodoVecino)) // Evitar ciclos
                        {
                            var nuevaRuta = new List<object>(rutaHastaAhora) { nodoVecino };
                            cola.Enqueue((nodoVecino, nuevaRuta, costoActual + peso));
                        }
                    }
                }
            }

            return mejorRuta;
        }
        // Método auxiliar para obtener Nodo a partir de una estructura
        private Nodo ObtenerNodo(object estructura)
        {
            return estructura as Nodo; 
        }
        private void AvisoDespegue()
        {
            if (Destino.TieneAeropuerto && Destino.Elemento is Aeropuerto aeropuertoDestino)
            {
                aeropuertoDestino.AvionDespega();
                AsignarDestinoAleatorio();
                System.Diagnostics.Debug.WriteLine($"El avión despego de aeropuerto {aeropuertoDestino.Nombre}.");
            }
            else if (Destino.TienePortaviones && Destino.Elemento is Portaviones portavionesDestino)
            {
                portavionesDestino.AvionDespega();
                AsignarDestinoAleatorio();
                System.Diagnostics.Debug.WriteLine($"El avión despego de portaviones {portavionesDestino.Nombre}.");
            }

        }


        public void MoverAvion()
        {
            if (!Activo)
            {
                System.Diagnostics.Debug.WriteLine("El avión está inactivo y no puede moverse.");
                return;
            }

            if (Ruta.Count > 0)
            {
                NodoActual.TieneAvion = false;
                NodoActual = Ruta[0];
                NodoActual.TieneAvion = true;
                Ruta.RemoveAt(0);
                ConsumirCombustible(); // Consumir combustible al moverse
                //System.Diagnostics.Debug.WriteLine("Se mueve algun avion");

                if (Ruta.Count == 0)
                {
                    HaLlegadoADestino = true;
                    Task.Run(() => ProcesarAterrizajeAsync());
                }
            }
            else
            {
               
            }
        }



        // Reduce combustible al moverse
        private void ConsumirCombustible()
        {
            Combustible -= 10;
            //System.Diagnostics.Debug.WriteLine($"Combustible restante: {Combustible}");

            // Incrementar las horas de vuelo en los módulos IA
            foreach (var modulo in ObtenerModulos())
            {
                modulo.IncrementarHorasDeVuelo();
            }

            if (Combustible <= 0)
            {
                Destruir();
            }
        }
        // Reabastecer combustible al aterrizar
        public void RecargarCombustible(int cantidad)
        {
            Combustible += cantidad;
            if (Combustible > 1000) // Limitar al máximo de combustible
                Combustible = 1000;
            System.Diagnostics.Debug.WriteLine($"Combustible recargado. Nivel actual: {Combustible}.");
        }

        // Destruir el avión si el combustible llega a 0
        public void Destruir()
        {
            Activo = false;
            NodoActual.TieneAvion = false;
            System.Diagnostics.Debug.WriteLine($"El avión con ID {ID} se destruyó por falta de combustible.");

            // Imprime detalles de los módulos de IA
            System.Diagnostics.Debug.WriteLine("Detalles de los módulos del avión:");
            var modulos = ObtenerModulos();
            foreach (var modulo in modulos)
            {
                System.Diagnostics.Debug.WriteLine($"ID: {modulo.ID}, Rol: {modulo.Rol}, Horas de Vuelo: {modulo.HorasDeVuelo}");
            }
        }


        // Obtiene todos los módulos
        public List<ModuloIA> ObtenerModulos()
        {
            return new List<ModuloIA> { Pilot, Copilot, Maintenance, SpaceAwareness };
        }

        // Métodos para ordenar los módulos
        public void OrdenarModulosPorID()
        {
            var modulos = ObtenerModulos();
            SelectionSort(modulos, CompararPorID);
        }

        public void OrdenarModulosPorRol()
        {
            var modulos = ObtenerModulos();
            SelectionSort(modulos, CompararPorRol);
        }

        public void OrdenarModulosPorHoras()
        {
            var modulos = ObtenerModulos();
            SelectionSort(modulos, CompararPorHoras);
        }

        // Algoritmo Selection Sort para ordenar los módulos
        private void SelectionSort(List<ModuloIA> modulos, Comparison<ModuloIA> comparador)
        {
            for (int i = 0; i < modulos.Count - 1; i++)
            {
                int indiceMin = i;
                for (int j = i + 1; j < modulos.Count; j++)
                {
                    if (comparador(modulos[j], modulos[indiceMin]) < 0)
                    {
                        indiceMin = j;
                    }
                }

                // Intercambia los elementos
                var temp = modulos[i];
                modulos[i] = modulos[indiceMin];
                modulos[indiceMin] = temp;
            }
        }

        private int CompararPorID(ModuloIA a, ModuloIA b)
        {
            return string.Compare(a.ID, b.ID);
        }

        private int CompararPorRol(ModuloIA a, ModuloIA b)
        {
            return string.Compare(a.Rol, b.Rol);
        }

        private int CompararPorHoras(ModuloIA a, ModuloIA b)
        {
            return a.HorasDeVuelo.CompareTo(b.HorasDeVuelo);
        }
        // Método para obtener el siguiente nodo en la dirección hacia el destino

        private Nodo ObtenerSiguienteNodoEnDireccion(Nodo actual, Nodo destino)
        {
            // Compara las posiciones en relación con el nodo actual y el destino
            bool moverNorte = actual.Norte != null && destino == actual.Norte || actual.Norte == destino;
            bool moverSur = actual.Sur != null && destino == actual.Sur || actual.Sur == destino;
            bool moverEste = actual.Este != null && destino == actual.Este || actual.Este == destino;
            bool moverOeste = actual.Oeste != null && destino == actual.Oeste || actual.Oeste == destino;

            if (moverNorte) return actual.Norte;
            if (moverSur) return actual.Sur;
            if (moverEste) return actual.Este;
            if (moverOeste) return actual.Oeste;

            // Movimiento diagonal si aplica
            if (actual.Noroeste != null && destino == actual.Noroeste) return actual.Noroeste;
            if (actual.Noreste != null && destino == actual.Noreste) return actual.Noreste;
            if (actual.Suroeste != null && destino == actual.Suroeste) return actual.Suroeste;
            if (actual.Sureste != null && destino == actual.Sureste) return actual.Sureste;

            return null; // Si no se encuentra ninguna dirección válida
        }

    }
}
