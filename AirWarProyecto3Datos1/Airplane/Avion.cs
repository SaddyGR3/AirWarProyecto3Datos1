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
        private Matriz matriz;


        public Avion(Nodo nodoInicial, Nodo destino, Matriz matriz)
        {
            NodoActual = nodoInicial;
            NodoActual.TieneAvion = true;
            this.matriz = matriz;

            // Genera la ruta recta desde el nodoInicial hasta el destino
            int filaInicio = matriz.GetRow(nodoInicial);
            int columnaInicio = matriz.GetColumn(nodoInicial);
            int filaDestino = matriz.GetRow(destino);
            int columnaDestino = matriz.GetColumn(destino);

            Ruta = CalcularRutaRecta(filaInicio, columnaInicio, filaDestino, columnaDestino);

        }

        // Mover el avión al siguiente nodo de la ruta
        public void MoverAvion()
        {
            if (Ruta.Count > 0)
            {
                NodoActual.TieneAvion = false; // Marcar el nodo actual como sin avión
                NodoActual = Ruta[0];
                NodoActual.TieneAvion = true; // Marcar el nuevo nodo con el avión
                Ruta.RemoveAt(0); // Avanza en la ruta
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
