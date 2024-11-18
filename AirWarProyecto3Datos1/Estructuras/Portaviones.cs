using AirWarProyecto3Datos1.Airplane;
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
        // Reserva de combustible
        private const int capacidadMaximaCombustible = 10000;
        private int reservaCombustible = capacidadMaximaCombustible;


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
        public void AvionAterriza(Avion avion)
        {
            if (HayEspacioEnHangar())
            {
                avionesEnHangar++;
                // Distribuir combustible al avión (2 de cada 3 aviones)
                if (new Random().Next(3) < 2 && reservaCombustible > 0)
                {
                    int combustibleDistribuido = new Random().Next(200, 501);
                    if (combustibleDistribuido > reservaCombustible)
                        combustibleDistribuido = reservaCombustible;

                    avion.RecargarCombustible(combustibleDistribuido);
                    reservaCombustible -= combustibleDistribuido;

                    System.Diagnostics.Debug.WriteLine($"Se suministraron {combustibleDistribuido} de combustible al avión.");
                }
                System.Diagnostics.Debug.WriteLine($"Avión aterrizó en {Nombre}. Aviones en hangar: {avionesEnHangar}.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Hangar del portaviones {Nombre} lleno. Avión no puede aterrizar.");
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
