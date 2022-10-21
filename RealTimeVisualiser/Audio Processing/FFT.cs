using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Numerics;
using System.Drawing;
namespace RealTimeVisualiser
{

    //NEED TO IMPLEMENT FUNCTION THAT TAKES HZ OR SAMPLE TIME AND USES IT TO WORK OUT FREQUENCY
    //currently all frequencies are arbitrary values that are correctly proportional to each other 

    class FastFourierTransform
    {
        private Vector2 _screenDimensions;
        private Vector2 _graphOrigin;

        public FastFourierTransform(Vector2 ScreenDimensions)
        {
            Vector2 _screenDimensions = ScreenDimensions;

            
            //foreach (var p in realTransform) Console.WriteLine("frequency: " + Convert.ToString(Math.Round(p.X / 1.499268, 5)));
            //Console.WriteLine("biggest:" + biggest);
            /*
            foreach (PointF p in realTransform)
            {
                var tempPoint = new PointF(p.X, ScreenDimensions.Y);
                graphics.DrawLine(pen, p, tempPoint);
            }
            */
            //graphics.DrawLines(pen, realTransform);
            //graphics.DrawLines(pen, input);
        }
        public PointF[] FFTForwardtoPoints(Single[] samples)
        {
            Complex[] complexSamples = toComplexArray(samples);
            Complex[] transform = FFT(complexSamples);
            PointF[] realTransform = complex_transform_to_freqs(transform, _screenDimensions);
            return realTransform;
        }
        private static Complex[] FFT(Complex[] samples)
        {

            Complex CONST_i = new Complex(0, 1);

            int n = samples.Length;
            if (n == 1) return samples;
            else if (!IsPowerOfTwo(n)) { throw new ArgumentOutOfRangeException(); }

            Complex o = Complex.Pow(Math.E, (2 * Math.PI * CONST_i) / n);

            Complex[] samples_e = samples.Where((x, i) => i % 2 == 0).ToArray();
            Complex[] samples_o = samples.Where((x, i) => i % 2 != 0).ToArray();

            Complex[] transform_e = FFT(samples_e);
            Complex[] transform_o = FFT(samples_o);

            Complex[] transform = new Complex[n];

            for (int i = 0; i < n / 2; i++)
            {
                transform[i] = transform_e[i] + Complex.Pow(o, i) * transform_o[i];
                transform[i + n / 2] = transform_e[i] - Complex.Pow(o, i) * transform_o[i];
            }

            return transform;

        }
        private static Complex[] toComplexArray(Single[] singleArray)
        {
            Complex[] complexArray = new Complex[singleArray.Length];

            for (int i = 0; i < singleArray.Length; i++)
            {
                complexArray[i] = new Complex(singleArray[i], 0);
            }

            return complexArray;
        }

        static bool IsPowerOfTwo(int d)
        {
            ulong x = Convert.ToUInt64(d);
            return (x != 0) && ((x & (x - 1)) == 0);
        }

        private static double[] str_to_double_array(string stringSamples)
        {
            string[] str_array = (stringSamples.Split(new string[] { "," }, StringSplitOptions.None));
            double[] doubleArray = new double[str_array.Length];
            for (int i = 0; i < str_array.Length; i++)
            {
                //Debug.WriteLine(str_array[i].Split(',')[0]);
                doubleArray[i] = Convert.ToDouble(str_array[i]);
            }
            return doubleArray;
        }

        private static void printDoubleArray(double[] array)
        {
            foreach (double i in array)
            {
                Console.WriteLine(Convert.ToString(i));
            }
        }

        private double[] getFreqs(PointF[] points)
        {
            var x = new double[0];
            return x;
        }

        private static PointF[] complex_transform_to_freqs(Complex[] complexArray, Vector2 dimensions)
        {

            List<PointF> freqs = new List<PointF> { };
            var Xscale = complexArray.Length / dimensions.X;
            double maxIm = 0;

            //Single x = 1000F / complexArray.Length;
            for (int i = 0; i < complexArray.Length; i++)
            {
                if (complexArray[i].Imaginary > 1)
                {
                    freqs.Add(new PointF(i * Xscale, Convert.ToSingle(complexArray[i].Imaginary)));
                    if (complexArray[i].Imaginary > maxIm) maxIm = complexArray[i].Imaginary;
                }
            }
            var Yscale = maxIm / complexArray.Length;
            for (int i = 0; i < freqs.Count; i++) freqs[i] = new PointF(freqs[i].X, Convert.ToSingle(dimensions.Y - freqs[i].Y * Yscale));
            return freqs.ToArray();
        }

    }
}
