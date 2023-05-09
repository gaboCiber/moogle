using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoogleEngine
{
    public class Matrix
    {
        #region Fields

        private float[,] components;
        public int Columns { get => components.GetLength(1); }
        public int Rows { get => components.GetLength(0); }

        #endregion

        #region Constructor & Indexer
        public Matrix(float[,] matrix)
        {
            components = new float[matrix.GetLength(0), matrix.GetLength(1)];
            components = Matrix.Sum(components, matrix);
        }

        public float this[int i, int j]
        {
            get => (i >= 0 && i < Rows && j >= 0 && j < Columns) ? components[i, j] : -1;
        }
        #endregion

        #region Operations
        public static float[,] Sum(float[,] matrix1, float[,] matrix2)
        {
            // Check the size of the matrix
            if (matrix1.GetLength(0) != matrix2.GetLength(0) || matrix1.GetLength(1) != matrix2.GetLength(1))
                return new float[0, 0];

            // Sum the matrices
            float[,] matrixSum = new float[matrix1.GetLength(0), matrix1.GetLength(1)];

            for (int i = 0; i < matrix1.GetLength(0); i++)
            {
                for (int j = 0; j < matrix1.GetLength(1); j++)
                {
                    matrixSum[i, j] = matrix1[i, j] + matrix2[i, j];
                }
            }

            return matrixSum;
        }

        public static float[,] ScalarMultiplication(float[,] matrix, float scalar)
        {
            // Multiply the scalor with each component of the matrix
            float[,] matrixMult = new float[matrix.GetLength(0), matrix.GetLength(1)];

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    matrixMult[i, j] = scalar * matrix[i, j];
                }
            }

            return matrixMult;
        }

        public static float[,] Multiplication(float[,] matrix1, float[,] matrix2)
        {
            // Check the size of the matrices: the number of columns of the first one must be same as the numbers of rows of the second one
            if (matrix1.GetLength(1) != matrix2.GetLength(0))
                return new float[0, 0];

            // Multiply the matrices: the matrix resulted from the multiplication has the rows and columns of the first and second one respectively
            float[,] matrixMult = new float[matrix1.GetLength(0), matrix2.GetLength(1)];

            for (int i = 0; i < matrix1.GetLength(0); i++)
            {
                for (int j = 0; j < matrix2.GetLength(1); j++)
                {
                    for (int k = 0; k < matrix1.GetLength(1); k++)
                    {
                        matrixMult[i, j] += matrix1[i, k] * matrix2[k, j];
                    }
                }
            }

            return matrixMult;
        }

        public static float[] Multiplication(float[,] matrix, float[] columnVector)
        {
            // Check the size of the matrix and the vector: the number of columns of the matrix must be same as the numbers of rows of the vector
            if (matrix.GetLength(1) != columnVector.GetLength(0))
                return new float[0];

            // Multiply the matrix and the vector
            float[] resultVector = new float[matrix.GetLength(0)];

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    resultVector[i] += matrix[i, j] * columnVector[j];
                }
            }

            return resultVector;
        }

        public static float[,] Transpose(float[,] matrix)
        {
            // The transpose matrix has the rows and colums switched from the original matix
            float[,] matrixTranspose = new float[matrix.GetLength(1), matrix.GetLength(0)];

            for (int i = 0; i < matrixTranspose.GetLength(0); i++)
            {
                for (int j = 0; j < matrixTranspose.GetLength(1); j++)
                {
                    matrixTranspose[i, j] = matrix[j, i];
                }
            }

            return matrixTranspose;
        }

        public static float[] Norma(float[,] matrix)
        {
            float[] norma = new float[matrix.GetLength(0)];

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    norma[i] += matrix[i, j] * matrix[i, j];
                }

                norma[i] = MathF.Sqrt(norma[i]);
            }

            return norma;
        } 
        #endregion
    }
}
