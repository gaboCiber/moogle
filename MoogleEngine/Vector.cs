using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoogleEngine
{
    public class Vector
    {
        #region Fields
        private float[] components;
        public int Length { get => components.Length; }
        #endregion

        #region Constructor & Indexer
        public Vector(float[] vector)
        {
            components = vector[0..vector.Length];
        }

        public float this[int index]
        {
            get => (index >= 0 && index < Length) ? components[index] : -1;
        }
        #endregion

        #region Operations
        public static float[] Sum(float[] vector1, float[] vector2)
        {
            // Check the size of the vectors
            if (vector1.Length != vector2.Length)
                return new float[0];
            
            float[] vectorSum = new float[vector1.Length];

            for (int i = 0; i < vector1.Length; i++)
            {
                vectorSum[i] = vector1[i] + vector2[i];
            }

            return vectorSum;
        }

        public static float[] ScalarMultiplication(float[] vector, float scalar)
        {
            // Multiply the scalor with each component of the vector
            float[] vectorMult = new float[vector.Length];

            for (int i = 0; i < vector.Length; i++)
            {
                vectorMult[i] = scalar * vector[i];
            }

            return vectorMult;
        }

        public static float Norma(float[] vector)
        {
            float norma = 0;

            for (int i = 0; i < vector.Length; i++)
            {
                norma += vector[i] * vector[i];
            }

            return MathF.Sqrt(norma);
        }
        #endregion
    }
}
