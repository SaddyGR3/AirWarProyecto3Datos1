using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirWarProyecto3Datos1.LogicaCentral
{
    internal class ListaAdyacencia
    {
        public Nodo NodoDestino { get; set; }
        public int Peso { get; set; }

        public ListaAdyacencia(Nodo destino, int peso)
        {
            NodoDestino = destino;
            Peso = peso;
        }
    }
}
