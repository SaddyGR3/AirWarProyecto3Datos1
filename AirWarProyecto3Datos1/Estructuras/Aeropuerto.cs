using AirWarProyecto3Datos1.Airplane;
using AirWarProyecto3Datos1.LogicaCentral;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirWarProyecto3Datos1.Estructuras
{
    internal class Aeropuerto
    {
        public string Nombre { get; set; }
        public Nodo Ubicacion { get; set; }
        public int capacidadHangar = 30000;
        public int avionesEnHangar = 0;
        private DateTime ultimoTiempoConstruccion;
        private const int cooldownConstruccionSegundos = 10;

        public Aeropuerto(Nodo ubicacion)
        {
            if (ubicacion.Terreno == TipoTerreno.Tierra)
            {
                Ubicacion = ubicacion;
                Ubicacion.TieneAeropuerto = true; // Marca el nodo con el aeropuerto
                ultimoTiempoConstruccion = DateTime.MinValue; // Inicializar en un valor mínimo
            }
            else
            {
                throw new Exception("El aeropuerto debe ubicarse en terreno de tierra.");
            }
        }
        public bool HayEspacioEnHangar()
        {
            return avionesEnHangar < capacidadHangar;
        }
        public bool PuedeConstruirAvion()
        {
            return (DateTime.Now - ultimoTiempoConstruccion).TotalSeconds >= cooldownConstruccionSegundos;
        }
        public void AvionAterriza()
        {
            if (HayEspacioEnHangar())
            {
                avionesEnHangar++;
            }
            else
            {
               
            }
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
        public void DespegarAvion(Avion avion, Nodo destino)
        {
            avion.AsignarDestino(destino);
            AvionDespega(); // Resta uno del hangar al despegar
        }
        public Avion CrearAvion(Matriz matriz)
        {
            if (!HayEspacioEnHangar() || !PuedeConstruirAvion())
                throw new InvalidOperationException("No se puede construir un avión en este momento.");

            Avion nuevoAvion = new Avion(Ubicacion, matriz);
            AvionAterriza(); // Añade uno al hangar al crearse
            ultimoTiempoConstruccion = DateTime.Now; // Actualiza el tiempo de construcción
            return nuevoAvion;
        }

        private bool EsNodoValido(Nodo nodo)
        {
            // Define aquí las reglas para un destino válido, si existen
            return nodo != null; // Ejemplo básico
        }
    }
}
