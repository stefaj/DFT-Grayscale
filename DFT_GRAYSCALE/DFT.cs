using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DFT_GRAYSCALE
{
    public class DFT
    {
        public static Complex GetWN(int N)
        {
            return new Complex(Math.Cos(2*Math.PI / N), Math.Sin(2*Math.PI/N));
        }

        public static Complex[,] GetDN(int N, bool inverse = false)
        {
            Complex[,] DN = new Complex[N, N];
            Complex WN = GetWN(N);
            int inv = inverse ? 1 : -1;
            for(int i = 0; i < N; i++)
            {
                for(int j = 0; j < N; j++)
                {
                    DN[i, j] = WN ^ (inv * i * j);
                    DN[i, j].i = (Math.Abs(DN[i, j].i) < 1E-15) ? 0 : DN[i, j].i;
                    DN[i, j].r = (Math.Abs(DN[i, j].r) < 1E-15) ? 0 : DN[i, j].r;
                }
            }
            return DN;
        }

        public static Complex[] GetFourier(Complex[] x)
        {
            Complex[,] DN = GetDN(x.Length);
            return Complex.Multiply(DN, x);
        }

        public static Complex[,] GetFourier(Complex[,] x)
        {
            Complex[,] DN = GetDN(x.GetLength(0));
            Complex[,] DM = GetDN(x.GetLength(1));
            return Complex.Multiply(Complex.Multiply(DM, x), DN);
        } 

        public static Complex[,] GetInverseFourier(Complex[,] x)
        {
            Complex[,] DN = GetDN(x.GetLength(0),true);
            Complex[,] DM = GetDN(x.GetLength(1),true);
            return Complex.Multiply(Complex.Multiply(DM, x), DN);
        }

        public static Complex[] GetInverseFourier(Complex[] x)
        {
            Complex[,] DN = GetDN(x.Length, true);
            Complex[] res = Complex.Multiply(DN, x);
            for (int i = 0; i < res.Length; i++)
                res[i] /= x.Length;
            return res;
        }
    }
}
