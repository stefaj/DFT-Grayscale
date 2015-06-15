using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DFT_GRAYSCALE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string file_name = "";
        public MainWindow()
        {
            InitializeComponent();
        }

        void ProcessImage()
        {
            imgGray.Source = LoadAsGrayScale(file_name);
        }

        WriteableBitmap LoadAsGrayScale(string file_name)
        {
            if (file_name == "")
                return null;
            WriteableBitmap image;
            //Convert to grayscale
            var color_img = new WriteableBitmap(new BitmapImage(new Uri(file_name)));
            int width = (int)color_img.PixelWidth;
            int height = (int)color_img.PixelHeight;
            image = new WriteableBitmap(color_img.PixelWidth, color_img.PixelHeight, color_img.DpiX, color_img.DpiY, PixelFormats.Bgra32, BitmapPalettes.Gray256Transparent);
            image.Lock();
            
            unsafe
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {

                        Color c = new Color();
                        IntPtr pBackBuffer = color_img.BackBuffer;

                        byte* pBuff = (byte*)pBackBuffer.ToPointer();

                        c.B = pBuff[4 * x + (y * color_img.BackBufferStride)];
                        c.G = pBuff[4 * x + (y * color_img.BackBufferStride) + 1];
                        c.R = pBuff[4 * x + (y * color_img.BackBufferStride) + 2];
                        c.A = pBuff[4 * x + (y * color_img.BackBufferStride) + 3];

                        pBackBuffer = image.BackBuffer;
                        pBuff = (byte*)pBackBuffer.ToPointer();
                        Color gray_comp = Luminance(c);

                        pBuff[4 * x + (y * image.BackBufferStride)] = gray_comp.B;
                        pBuff[4 * x + (y * image.BackBufferStride) + 1] = gray_comp.G;
                        pBuff[4 * x + (y * image.BackBufferStride) + 2] = gray_comp.R;
                        pBuff[4 * x + (y * image.BackBufferStride) + 3] = gray_comp.A;
                    }

                }
            }

            image.AddDirtyRect(new Int32Rect(0, 0, width, height));
            image.Unlock();
            return image;
        }

        void DoFourier()
        {
            WriteableBitmap graybit = imgGray.Source as WriteableBitmap;
            int width = (int)graybit.PixelWidth;
            int height = (int)graybit.PixelHeight;

            Complex[,] imageData = new Complex[height, width];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    unsafe
                    {
                        IntPtr pBackBuffer = graybit.BackBuffer;
                        byte* pBuff = (byte*)pBackBuffer.ToPointer();

                        byte col = pBuff[4 * x + (y * graybit.BackBufferStride)];

                        imageData[y, x] = new Complex(col, 0);
                    }
                }
            }

            Complex[,] fourier = DFT.GetFourier(imageData);


            //Filters


            var shifted_fourier = fourier.Clone() as Complex[,];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    int x_mod = (x + width / 2) % width;
                    int y_mod = (y + height / 2) % height;
                    shifted_fourier[x, y] = fourier[x_mod, y_mod];
                }
            }

            var phase_fourier = shifted_fourier.Clone() as Complex[,];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    phase_fourier[x, y].r = Math.Atan2(phase_fourier[x, y].i, phase_fourier[x, y].r);
                    phase_fourier[x, y].i = 0;
                }
            }
            var normalized_phase = Complex.Normalize(phase_fourier, 255);
            imgPhase.Source = PlotComplex(normalized_phase);
            //var normalized = Complex.Normalize(fourier, 255);



            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    double thresh_high = slider_high.Value / 100 * width;
                    double thresh_low = slider_low.Value / 100 * width;
                    double circ = Math.Sqrt(Math.Pow(x - width / 2, 2) + Math.Pow(y - height / 2, 2));
                    if (circ > thresh_high || circ < thresh_low)
                    {
                        shifted_fourier[x, y].r = 0;
                        shifted_fourier[x, y].i = 0;
                    }
                }
            }

            var normalized_fourier = Complex.Normalize(shifted_fourier, 255);
            imgFourier.Source = PlotComplex(normalized_fourier);



            var inverse = DFT.GetInverseFourier(shifted_fourier);

            var filtered_normalized = Complex.Normalize(inverse, 255);

            imgInverse.Source = PlotComplex(filtered_normalized);
        }

        WriteableBitmap PlotComplex(Complex [,] data)
        {
            int width = data.GetLength(1);
            int height = data.GetLength(0);
            var image = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Gray256Transparent);
            image.Lock();

            unsafe
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        //var pixel = image.GetPixel(x, y);

                        IntPtr pBackBuffer = image.BackBuffer;
                        byte* pBuff = (byte*)pBackBuffer.ToPointer();

                        byte b = (byte) Complex.Magnitude(data[y, x]);
                        pBuff[4 * x + (y * image.BackBufferStride)] = b;
                        pBuff[4 * x + (y * image.BackBufferStride) + 1] = b;
                        pBuff[4 * x + (y * image.BackBufferStride) + 2] = b;
                        pBuff[4 * x + (y * image.BackBufferStride) + 3] = 255;
                    }

                }
            }

            image.AddDirtyRect(new Int32Rect(0, 0, width, height));
            image.Unlock();
            return image;
        }

        Color Luminance(Color rgb)
        {
            byte lum = (byte)(0.21 * rgb.R + 0.72 * rgb.G + 0.07 * rgb.B);
            
            return new Color() { R = lum, G = lum, B = lum, A = 255 };
        }

        public double[,] Multiply(double[,] mat1, double[,] mat2)
        {
            double[,] mat3 = new double[mat1.GetLength(0), mat2.GetLength(1)];
            if (mat1.GetLength(1) != mat2.GetLength(0))
                return null;
            for (int j = 0; j < mat2.GetLength(1); j++)//columns
            {
                for(int i = 0; i < mat1.GetLength(0); i++)//rows
                {
                    double sum = 0;
                    for(int k=0; k < mat2.GetLength(1); k++)
                    {
                        sum += mat1[i, k] * mat2[k, j];
                    }
                    mat3[i, j] = sum;
                }
            }
            return mat3;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ProcessImage();
            DoFourier();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog() == true)
            {
                file_name = ofd.FileName;
            }
        }


    }
}
