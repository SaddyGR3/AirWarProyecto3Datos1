using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirWarProyecto3Datos1.LogicaCentral
{
    internal class NodoGrafo
    {
        public Nodo Nodo { get; set; }
        public List<Arista> Vecinos { get; set; } = new List<Arista>();
    }

    internal class Arista
    {
        public NodoGrafo Destino { get; set; }
        public double Peso { get; set; }
    }
}
