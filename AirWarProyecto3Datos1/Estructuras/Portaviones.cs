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

        public Portaviones(Nodo ubicacion)
        {
            if (ubicacion.Terreno == TipoTerreno.Mar)
            {
                Ubicacion = ubicacion;
                Ubicacion.TienePortaviones = true; // Marca el nodo con el portaviones
            }
            else
            {
                throw new Exception("El portaviones debe ubicarse en terreno de agua.");
            }
        }
    }
}
