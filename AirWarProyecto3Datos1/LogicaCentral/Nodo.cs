using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirWarProyecto3Datos1.LogicaCentral
{
    internal class Nodo //Se necesitan 8 referencias para que los recorridos de los aviones sean mas naturales
    {
        public string Data { get; set; }
        public Nodo Norte { get; set; }
        public Nodo Sur { get; set; }
        public Nodo Oeste { get; set; }
        public Nodo Este { get; set; }
        public Nodo Noroeste { get; set; }
        public Nodo Noreste { get; set; }
        public Nodo Suroeste { get; set; }
        public Nodo Sureste { get; set; }
        public TipoTerreno Terreno { get; set; } //En este caso quiero probar enumeradores a ver si simplifican algo en el futuro.
        public bool TieneAvion { get; set; } //En este caso se opta por booleanos para saber si un nodo tiene un avion, aeropuerto, portaviones o bala, simplificara en extremo el calculo de colisiones y dibujado.
        public bool TieneAeropuerto { get; set; }
        public bool TienePortaviones { get; set; }
        public bool TieneJugador { get; set; }
        public bool TieneBala { get; set; }
        public int PesoRuta { get; set; }
        public object Elemento { get; set; }

        public Nodo()
        {
            Data = "";
            Norte = null;
            Sur = null;
            Oeste = null;
            Este = null;
            Noroeste = null;
            Noreste = null;
            Suroeste = null;
            Sureste = null;
            Terreno = TipoTerreno.Mar;
            TieneAvion = false;
            TieneAeropuerto = false;
            TienePortaviones = false;
            TieneJugador = false;
            TieneBala = false;
            PesoRuta = 0; //cada nodo tendra un peso de ruta,inicialmente puede ser 1 si es tierra, 2 si es agua.
            Elemento = null;

        }
    }
}
