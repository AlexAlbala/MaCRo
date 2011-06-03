using System;
using Microsoft.SPOT;

namespace MaCRo.Drivers.IMU
{
    class Matrix
    {
        /**************************************************/
        //Multiply two 3x3 matrixs. This function developed by Jordi can be easily adapted to multiple n*n matrix's. (Pero me da flojera!). 
        public static void Matrix_Multiply(float[][] a, float[][] b, float[][] mat)
        {
            float[] op = new float[3];
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    for (int w = 0; w < 3; w++)
                    {
                        op[w] = a[x][w] * b[w][y];
                    }
                    mat[x][y] = 0;
                    mat[x][y] = op[0] + op[1] + op[2];

                    float test = mat[x][y];
                }
            }
        }


    }
}
