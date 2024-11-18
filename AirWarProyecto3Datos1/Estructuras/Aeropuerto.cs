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

        // Reserva de combustible
        private const int capacidadMaximaCombustible = 10000;
        private int reservaCombustible = capacidadMaximaCombustible;

        public Aeropuerto(Nodo ubicacion)
        {
            if (ubicacion.Terreno == TipoTerreno.Tierra)
            {
                Ubicacion = ubicacion;
                Ubicacion.TieneAeropuerto = true; // Marca el nodo con el aeropuerto
                ultimoTiempoConstruccion = DateTime.MinValue; // Inicializar en un valor mínimo
                Ubicacion.Elemento = this;
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
                System.Diagnostics.Debug.WriteLine($"Hangar del aeropuerto {Nombre} lleno. Avión no puede aterrizar.");
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
        public void DespegarAvion(Avion avion)
        {
            if (avionesEnHangar > 0 && avion != null)
            {
                AvionDespega(); // Reduce el contador de aviones en el hangar
                System.Diagnostics.Debug.WriteLine($"El avión despegó del aeropuerto {Nombre} hacia su destino: {avion.Destino.Data}.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"No se pudo despegar el avión desde {Nombre}. Verifica el hangar o la instancia del avión.");
            }
        }

        public Avion CrearAvion(Matriz matriz, List<Nodo> DestinosPosibles)
        {
            if (!HayEspacioEnHangar() || !PuedeConstruirAvion())
                throw new InvalidOperationException("No se puede construir un avión en este momento.");

            Avion nuevoAvion = new Avion(Ubicacion, matriz, DestinosPosibles);
            avionesEnHangar++;
            ultimoTiempoConstruccion = DateTime.Now; // Actualiza el tiempo de construcción
            System.Diagnostics.Debug.WriteLine($"se creo un avion en el aeropuerto:");
            return nuevoAvion;
            
        }

        private bool EsNodoValido(Nodo nodo)
        {
            // Define aquí las reglas para un destino válido, si existen
            return nodo != null; // Ejemplo básico
        }
    }
}
