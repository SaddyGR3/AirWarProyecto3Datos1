using AirWarProyecto3Datos1.Estructuras;
using AirWarProyecto3Datos1.LogicaCentral;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public int Combustible { get; private set; } // Nueva propiedad


        public Avion(Nodo nodoInicial, Matriz matriz, List<Nodo> destinosPosibles)
        {
            NodoActual = nodoInicial;
            NodoActual.TieneAvion = true;
            DestinosPosibles = destinosPosibles;
            this.matriz = matriz;
            Activo = true;
            Ruta = new List<Nodo>();
            HaLlegadoADestino = false;
            Combustible = 1000; // Inicializar con el combustible estándar
        }

        // Asignar un destino aleatorio desde los disponibles
        public void AsignarDestinoAleatorio()
        {
            var destinosDisponibles = DestinosPosibles
                .Where(destino => destino != NodoActual)
                .ToList();

            if (!destinosDisponibles.Any())
                throw new InvalidOperationException("No hay destinos disponibles.");

            Random random = new Random();
            int indiceDestino = random.Next(destinosDisponibles.Count);
            Destino = destinosDisponibles[indiceDestino];
            CalcularRutaHaciaDestino();

            System.Diagnostics.Debug.WriteLine($"Destino asignado: {Destino.Data}. Ruta calculada: {Ruta.Count} nodos.");
        }

        // Método para procesar aterrizaje y despegar automáticamente después de un tiempo
        public async Task ProcesarAterrizajeAsync()
        {
            if (!Activo) return; // Validar si el avión aún está activo

            if (Destino.TieneAeropuerto && Destino.Elemento is Aeropuerto aeropuertoDestino)
            {
                NodoActual.TieneAvion = false;
                aeropuertoDestino.AvionAterriza(this); // Modificar para pasar el avión actual
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

            // Esperar antes de despegar
            await Task.Delay(TimeSpan.FromSeconds(5));

            // Verificar nuevamente antes de asignar destino
            if (Activo)
            {
                AvisoDespegue();
            }
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
        // Calcular la ruta hacia el destino asignado
        private void CalcularRutaHaciaDestino()
        {
            if (Destino == null)
                throw new InvalidOperationException("El destino no está asignado.");

            int filaInicio = matriz.GetRow(NodoActual);
            int columnaInicio = matriz.GetColumn(NodoActual);
            int filaDestino = matriz.GetRow(Destino);
            int columnaDestino = matriz.GetColumn(Destino);

            Ruta = CalcularRutaRecta(filaInicio, columnaInicio, filaDestino, columnaDestino);
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
                System.Diagnostics.Debug.WriteLine("Se mueve algun avion");

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

        // Método para calcular la ruta recta hacia el destino
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
            return ruta;
        }

        // Reducir combustible al moverse
        private void ConsumirCombustible()
        {
            Combustible -= 10;
            System.Diagnostics.Debug.WriteLine($"Combustible restante: {Combustible}");

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
            System.Diagnostics.Debug.WriteLine("El avión se destruyó por falta de combustible.");
            // Lógica adicional para removerlo del juego si es necesario
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
