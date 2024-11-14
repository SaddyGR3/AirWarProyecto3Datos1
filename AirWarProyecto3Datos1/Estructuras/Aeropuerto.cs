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

        public Aeropuerto(Nodo ubicacion)
        {
            if (ubicacion.Terreno == TipoTerreno.Tierra)
            {
                Ubicacion = ubicacion;
                Ubicacion.TieneAeropuerto = true; // Marca el nodo con el aeropuerto
            }
            else
            {
                throw new Exception("El aeropuerto debe ubicarse en terreno de tierra.");
            }
        }
    }
}
