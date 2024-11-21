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
        public bool Activo { get; private set; }
        private int velocidadBala;
        private DispatcherTimer timer;


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
                System.Diagnostics.Debug.WriteLine("bala llego al limite");
                timer.Stop(); // Detener el temporizador
                
                return;
            }

            NodoActual.TieneBala = false; // Limpiar la casilla anterior
            NodoActual = siguienteNodo; // Actualizar la posición de la bala
            NodoActual.TieneBala = true; // Colocar la bala en la nueva casilla
            System.Diagnostics.Debug.WriteLine("¡Bala se mueve!");
        }
        
        // Método para destruir la bala cuando se queda sin combustible
        private void DestruirBala()
        {
            Activo = false;
            NodoActual.TieneBala = false;
            // Aquí podrías agregar la lógica para eliminar la bala de la vista
        }

        // Método para gestionar el impacto de la bala sobre un avión
        private void ImpactarAvion()
        {
            // Aquí se asume que si hay un avión en el nodo, se destruye
            // Podrías agregar un método en la clase Avion para manejar la destrucción
            System.Diagnostics.Debug.WriteLine("¡Bala impactó un avión!");
            // Lógica para destruir el avión o restarle vida
            // avion.Destruir();
        }
    }
   
}
