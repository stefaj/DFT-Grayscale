using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFT_GRAYSCALE
{
    public struct Complex
    {
        public double r;
        public double i;

        public Complex(double r=0, double i=0)
        {
            this.r = r;
            this.i = i;
        }

        public static Complex Conjugate(Complex c1)
        {
            return new Complex(c1.r, -c1.i);
        }

        public static Complex operator +(Complex c1, Complex c2)
        {
            return new Complex(c1.r + c2.r, c1.i + c2.i);
        }

        public static Complex operator -(Complex c1, Complex c2)
        {
            return new Complex(c1.r - c2.r, c1.i - c2.i);
        }

        public static Complex operator *(Complex c1, Complex c2)
        {
            return new Complex(c1.r*c2.r - (c1.i*c2.i), c1.r * c2.i + c1.i * c2.r);
        }

        public static Complex operator *(Complex c1, double d)
        {
            return new Complex(c1.r * d, c1.i * d);
        }

        public static Complex operator /(Complex c1, double d)
        {
            return new Complex(c1.r / d, c1.i / d);
        }

        public static double Magnitude(Complex c1)
        {
            return Math.Sqrt(c1.r * c1.r + c1.i * c1.i);
        }

        public static Complex Inverse(Complex c1)
        {
            return new Complex(c1.r, -c1.i) / (c1.r * c1.r + c1.i * c1.i);
        }

        public static Complex operator ^(Complex c1, int n)
        {
            if (n >= 0)
                return Pow(c1, (uint)n);
            else
                return Complex.Inverse(Pow(c1, (uint)(-n)));
        }

        /*private static Complex Pow(Complex c1, uint n)
        {
            if (n == 0)
                return new Complex(1, 0);
            else return c1 * Pow(c1, (n - 1));
        }*/

        private static Complex Pow(Complex c1, uint n)
        {
            if (n == 0)
                return new Complex(1, 0);

            n--;
            Complex nc = c1;

            while(n>0)
            {
                nc = nc * c1;
                n--;
            }

            return nc;
        }



        public override string ToString()
        {
            return "(" + r + "," + i + ")";
        }

        public static Complex[,] Multiply(Complex[,] mat1, Complex[,] mat2)
        {
            Complex[,] mat3 = new Complex[mat1.GetLength(0), mat2.GetLength(1)];
            if (mat1.GetLength(1) != mat2.GetLength(0))
                return null;
            
            for (int i = 0; i < mat1.GetLength(0); i++)//rows
            {
                for (int j = 0; j < mat2.GetLength(1); j++)//columns
                {
                    Complex sum = new Complex();
                    for (int k = 0; k < mat2.GetLength(1); k++)
                    {
                        sum += mat1[i, k] * mat2[k, j];
                    }
                    mat3[i, j] = sum;
                }
            }
            return mat3;
        }

        public static Complex[] Multiply(Complex[,] mat1, Complex[] mat2)
        {
            Complex[] mat3 = new Complex[mat1.GetLength(0)];
            if (mat1.GetLength(1) != mat2.GetLength(0))
                return null;
            for (int i = 0; i < mat1.GetLength(0); i++)//rows
            {
                Complex sum = new Complex();
                for (int k = 0; k < mat2.GetLength(0); k++)
                {
                    sum += mat1[i, k] * mat2[k];
                }
                mat3[i] = sum;

            }
            return mat3;
        }

        public static Complex[,] Normalize(Complex[,] data, double max)
        {
            double local_max = 0;
            for (int x = 0; x < data.GetLength(1); x++)
                for (int y = 0; y < data.GetLength(0); y++)
                {
                    double m =Math.Abs(Complex.Magnitude(data[x,y]));
                    if (m > local_max)
                        local_max = m;
                }
            for (int x = 0; x < data.GetLength(1); x++)
                for (int y = 0; y < data.GetLength(0); y++)
                {
                    data[x, y] = data[x,y] / local_max * max;
                }
            return data;

        }

        public static Complex[,] Transpose(Complex[,] data)
        {
            Complex[,] t = data;
            for(int x = 0; x < data.GetLength(1); x++)
            {
                for(int y = 0; y < data.GetLength(0); y++)
                {
                    t[x, y] = data[y, x];
                }
            }
            return t;
        }

    }
}
