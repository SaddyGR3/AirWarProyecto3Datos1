using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirWarProyecto3Datos1.LogicaCentral
{
    public class PerlinNoise
    {
        private int[] permutation;

        // Constructor sin semilla para que sea aleatorio
        public PerlinNoise()
        {
            Random random = new Random(); // Generador de números aleatorios sin semilla fija
            permutation = new int[512];
            int[] p = new int[256];
            for (int i = 0; i < 256; i++)
                p[i] = i;

            // Mezclar usando un valor aleatorio
            for (int i = 255; i > 0; i--)
            {
                int swap = random.Next(i + 1);
                int temp = p[i];
                p[i] = p[swap];
                p[swap] = temp;
            }

            // Duplicar el arreglo de permutación
            for (int i = 0; i < 512; i++)
                permutation[i] = p[i % 256];
        }

        private static double Fade(double t) => t * t * t * (t * (t * 6 - 15) + 10);
        private static double Lerp(double a, double b, double t) => a + t * (b - a);
        private static double Grad(int hash, double x, double y)
        {
            int h = hash & 15;
            double u = h < 8 ? x : y;
            double v = h < 4 ? y : h == 12 || h == 14 ? x : 0;
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }

        public double Generate(double x, double y)
        {
            int xi = (int)Math.Floor(x) & 255;
            int yi = (int)Math.Floor(y) & 255;

            double xf = x - Math.Floor(x);
            double yf = y - Math.Floor(y);

            double u = Fade(xf);
            double v = Fade(yf);

            int aa = permutation[xi] + yi;
            int ab = permutation[xi] + yi + 1;
            int ba = permutation[xi + 1] + yi;
            int bb = permutation[xi + 1] + yi + 1;

            double x1, x2, y1;
            x1 = Lerp(Grad(permutation[aa], xf, yf), Grad(permutation[ba], xf - 1, yf), u);
            x2 = Lerp(Grad(permutation[ab], xf, yf - 1), Grad(permutation[bb], xf - 1, yf - 1), u);
            y1 = Lerp(x1, x2, v);

            return y1;
        }
    }
}
