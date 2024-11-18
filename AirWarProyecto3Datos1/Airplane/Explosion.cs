using AirWarProyecto3Datos1.Estructuras;
using AirWarProyecto3Datos1.LogicaCentral;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows;
namespace AirWarProyecto3Datos1.Airplane
{
    internal class Explosion
    {
        public Point Posicion { get; private set; } // Coordenadas de la explosión en el Canvas
        public Image ImageElement { get; private set; } // Elemento visual

        public Explosion(Point posicion, BitmapImage imagen)
        {
            Posicion = posicion;

            // Crear y configurar el elemento visual
            ImageElement = new Image
            {
                Source = imagen,
                Width = 50, // Ajustar tamaño según diseño
                Height = 50
            };

            // Colocar en la posición inicial
            Canvas.SetLeft(ImageElement, Posicion.X);
            Canvas.SetTop(ImageElement, Posicion.Y);
        }
    }
}
