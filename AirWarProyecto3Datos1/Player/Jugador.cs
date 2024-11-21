using AirWarProyecto3Datos1.LogicaCentral;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AirWarProyecto3Datos1.Player
{
    internal class Jugador
    {
        public string Nombre { get; set; }
        public Nodo Ubicacion { get; set; }

        

        public Jugador(Nodo ubicacionInicial)
        {
            if (ubicacionInicial != null)
            {
                Ubicacion = ubicacionInicial;
                Ubicacion.TieneJugador = true; // Marca el nodo con el jugador
                Ubicacion.Elemento = this;
            }
            else
            {
                throw new Exception("La ubicación inicial del jugador no puede ser nula.");
            }
        }

        public void MoverIzquierda()
        {
            if (Ubicacion.Oeste != null)
            {
                // Actualiza la ubicación del jugador
                Ubicacion.TieneJugador = false;
                Ubicacion.Elemento = null;
                Ubicacion = Ubicacion.Oeste;
                Ubicacion.TieneJugador = true;
                Ubicacion.Elemento = this;
            }
        }

        public void MoverDerecha()
        {
            if (Ubicacion.Este != null)
            {
                // Actualiza la ubicación del jugador
                Ubicacion.TieneJugador = false;
                Ubicacion.Elemento = null;
                Ubicacion = Ubicacion.Este;
                Ubicacion.TieneJugador = true;
                Ubicacion.Elemento = this;
            }
        }
    }

}
