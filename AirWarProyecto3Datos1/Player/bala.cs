using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using AirWarProyecto3Datos1.LogicaCentral;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace AirWarProyecto3Datos1.Player
{
    internal class Bala
    {
        public Nodo NodoActual { get; private set; }
        public Matriz Matriz { get; private set; }
        public bool Activo { get;  set; }
        private int velocidadBala;
        public DispatcherTimer timer;


        public Bala(Nodo nodoInicial, Matriz matriz, int velocidad)
        {
            NodoActual = nodoInicial;
            Matriz = matriz;
            velocidadBala = velocidad;
            Activo = true;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(velocidadBala); // Ajustamos la velocidad según el valor dado
            timer.Tick += (sender, e) => MoverBala(); // Llamamos al método para mover la bala
            timer.Start();

        }

        // Método para mover la bala hacia arriba
        public void MoverBala()
        {
            if (!Activo) return; // Si la bala no está activa, no se mueve

            Nodo siguienteNodo = NodoActual.Norte; // Mover hacia el norte (arriba)

            // Verificar si la bala llegó al borde del mapa
            if (siguienteNodo == null)
            {
                // La bala salió del mapa, desactivarla
                Activo = false;
                NodoActual.TieneBala = false; // Limpiar la casilla
                //System.Diagnostics.Debug.WriteLine("bala llego al limite");
                timer.Stop(); // Detener el temporizador
                
                return;
            }

            NodoActual.TieneBala = false; // Limpiar la casilla anterior
            NodoActual = siguienteNodo; // Actualizar la posición de la bala
            NodoActual.TieneBala = true; // Colocar la bala en la nueva casilla
            //System.Diagnostics.Debug.WriteLine("¡Bala se mueve!");
        }

        // Método para destruir la bala cuando se queda sin combustible
        public void DestruirBala()
        {
            Activo = false;
            NodoActual.TieneBala = false;
            if (timer != null)
            {
                timer.Stop(); // Detener el temporizador
                timer = null; // Liberar referencia para evitar fugas
                System.Diagnostics.Debug.WriteLine("Temporizador de bala detenido y eliminado.");
            }
        }


        // Método para gestionar el impacto de la bala sobre un avión
        public bool ImpactarAvion()
        {
            if (NodoActual.TieneAvion == true)
            {
                Activo = false;
                NodoActual.TieneBala = false;
                timer.Stop();
                System.Diagnostics.Debug.WriteLine("¡Bala impactó un avión!");
                return true;
            }
            return false;
            //System.Diagnostics.Debug.WriteLine("¡Bala impactó un avión!");
            // Lógica para destruir el avión o restarle vida
            // avion.Destruir();
        }
    }
   
}
