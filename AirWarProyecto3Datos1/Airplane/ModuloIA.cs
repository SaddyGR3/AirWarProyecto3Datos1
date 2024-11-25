using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirWarProyecto3Datos1.Airplane
{
    public class ModuloIA
    
    {
        public string ID { get; private set; }
        public string Rol { get; protected set; }
        public int HorasDeVuelo { get; set; }

        public ModuloIA(string rol)
        {
            ID = GenerarID();
            Rol = rol;
            HorasDeVuelo = 0;
        }

        private string GenerarID()
        {
            var random = new Random();
            char letra1 = (char)random.Next('A', 'Z' + 1);
            char letra2 = (char)random.Next('A', 'Z' + 1);
            char letra3 = (char)random.Next('A', 'Z' + 1);
            return $"{letra1}{letra2}{letra3}";
        }

        public void IncrementarHorasDeVuelo()
        {
            HorasDeVuelo++;
        }
    }

        public class Pilot : ModuloIA
    {
            public Pilot() : base("Pilot") { }
        }

        public class Copilot : ModuloIA
    {
            public Copilot() : base("Copilot") { }
        }

        public class Maintenance : ModuloIA
    {
            public Maintenance() : base("Maintenance") { }
        }

        public class SpaceAwareness : ModuloIA
    {
            public SpaceAwareness() : base("Space Awareness") { }
        }
    }

