using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirWarProyecto3Datos1.LogicaCentral
{
    internal class Matriz
    {
        public Nodo[,] Matrix { get; private set; }

        public Matriz(int rows, int cols)
        {
            Matrix = new Nodo[rows, cols];
            // Inicializa la matriz con sus nodos.
            for (int i = 0; i < rows; i++) // Bucle que itera sobre cada fila
            {
                for (int j = 0; j < cols; j++) // En cada fila, itera sobre toda la columna.
                {
                    Matrix[i, j] = new Nodo(); // Crea un nodo en cada ubicación.
                }
            }

            // Conecta los nodos en las 8 direcciones.
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    // Conexiones básicas
                    if (i > 0) Matrix[i, j].Norte = Matrix[i - 1, j]; // Nodo norte
                    if (i < rows - 1) Matrix[i, j].Sur = Matrix[i + 1, j]; // Nodo sur
                    if (j > 0) Matrix[i, j].Oeste = Matrix[i, j - 1]; // Nodo oeste
                    if (j < cols - 1) Matrix[i, j].Este = Matrix[i, j + 1]; // Nodo este

                    // Conexiones diagonales
                    if (i > 0 && j > 0) Matrix[i, j].Noroeste = Matrix[i - 1, j - 1]; // Nodo noroeste
                    if (i > 0 && j < cols - 1) Matrix[i, j].Noreste = Matrix[i - 1, j + 1]; // Nodo noreste
                    if (i < rows - 1 && j > 0) Matrix[i, j].Suroeste = Matrix[i + 1, j - 1]; // Nodo suroeste
                    if (i < rows - 1 && j < cols - 1) Matrix[i, j].Sureste = Matrix[i + 1, j + 1]; // Nodo sureste
                }
            }
        }

        public int GetRow(Nodo nodo)
        {
            for (int i = 0; i < Matrix.GetLength(0); i++)
            {
                for (int j = 0; j < Matrix.GetLength(1); j++)
                {
                    if (Matrix[i, j] == nodo)
                        return i;
                }
            }
            return -1; // Retorna -1 si no se encuentra el nodo
        }

        public int GetColumn(Nodo nodo)
        {
            for (int i = 0; i < Matrix.GetLength(0); i++)
            {
                for (int j = 0; j < Matrix.GetLength(1); j++)
                {
                    if (Matrix[i, j] == nodo)
                        return j;
                }
            }
            return -1; // Retorna -1 si no se encuentra el nodo
        }
        //Metodo para obtener un nodo especifico de la matriz
        public Nodo GetNode(int row, int col)
        {
            return Matrix[row, col];
        }
    }
}
