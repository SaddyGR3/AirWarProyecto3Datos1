using AirWarProyecto3Datos1.LogicaCentral;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirWarProyecto3Datos1.Estructuras
{
    internal class Portaviones
    {
        public string Nombre { get; set; }
        public Nodo Ubicacion { get; set; }
        public int capacidadHangar = 30000;
        public int avionesEnHangar = 0;


        public Portaviones(Nodo ubicacion)
        {
            if (ubicacion.Terreno == TipoTerreno.Mar)
            {
                Ubicacion = ubicacion;
                Ubicacion.TienePortaviones = true; // Marca el nodo con el aeropuerto
                Ubicacion.Elemento = this;
            }
            else
            {
                throw new Exception("El portaviones debe ubicarse en terreno de agua.");
            }
        }
        public void AvionAterriza()
        {
            if (HayEspacioEnHangar())
            {
                avionesEnHangar++;
                System.Diagnostics.Debug.WriteLine($"Avión aterrizó en {Nombre}. Aviones en hangar: {avionesEnHangar}.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Hangar del aeropuerto {Nombre} lleno. Avión no puede aterrizar.");
            }
        }
        public bool HayEspacioEnHangar()
        {
            return avionesEnHangar < capacidadHangar;
        }


        public void AvionDespega()
        {
            if (avionesEnHangar > 0)
            {
                avionesEnHangar--;
            }
            else
            {

            }
        }
        private bool EsNodoValido(Nodo nodo)
        {
            // Define aquí las reglas para un destino válido, si existen
            return nodo != null; // Ejemplo básico
        }
    }
}
