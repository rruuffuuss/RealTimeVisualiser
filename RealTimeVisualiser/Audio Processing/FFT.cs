using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Numerics;
using Color = Microsoft.Xna.Framework.Color;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using System.Drawing;
namespace RealTimeVisualiser
{

    //NEED TO IMPLEMENT FUNCTION THAT TAKES HZ OR SAMPLE TIME AND USES IT TO WORK OUT FREQUENCY
    //currently all frequencies are arbitrary values that are correctly proportional to each other 

    class FastFourierTransform
    {
        private Vector2 _screenDimensions;
        private Vector2 _graphOrigin;
        private PointF[] _realTransform;
        private Texture2D _colour;

        private float Xscale;
        private float YscaleConst;
        private float Yscale;

        public FastFourierTransform(Vector2 ScreenDimensions, Texture2D pixelcolour)
        {
            //_graphOrigin = new Vector2( 470f / 1920f * 1920 , 821f / 1080f * 1080);
            _graphOrigin = new Vector2(470f / 1920f * 1920, 820f / 1080f * 1080);
            _screenDimensions = new Vector2( 1416f / 1920f * ScreenDimensions.X, 795f / 1080f * ScreenDimensions.Y);

            _colour = pixelcolour;

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

        public void Draw(SpriteBatch _spriteBatch)
        {
            if( _realTransform != null )
            {
                var pos0 = _graphOrigin;
                foreach (PointF p in _realTransform)
                {
                    /*
                    //var pos = new Rectangle((int)Math.Abs(p.X + _graphOrigin.X), (int)Math.Abs(_graphOrigin.Y - p.Y), 1, (int)Math.Abs(p.Y));
                    var pos1 = new Rectangle((int)(p.X + _graphOrigin.X), (int)(_graphOrigin.Y - Math.Abs(p.Y)), 3, 3);
                    _spriteBatch.Draw(_colour, pos, Color.White);
                    */
                    var pos1 = new Vector2((int)(p.X + _graphOrigin.X), (int)(_graphOrigin.Y - Math.Abs(p.Y)));
                    DrawLine(_spriteBatch, pos0, pos1, Color.White);
                    pos0 = pos1;
                }
            }
            
        }

        public void FFTForwardtoPoints(Single[] samples, float amplitude)
        {
            Yscale = YscaleConst * amplitude * 10;
            var complexSamples = toComplexArray(samples);
            var transform = FFT(complexSamples);
            _realTransform = complex_transform_to_freqs(transform);
            //Debug.WriteLine(string.Join(',', _realTransform));
            //Debug.WriteLine(string.Join(',', transform));

        }
        private Complex[] FFT(Complex[] samples)
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
        private Complex[] toComplexArray(Single[] singleArray)
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

        private double[] str_to_double_array(string stringSamples)
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

        private void printDoubleArray(double[] array)
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

        private PointF[] complex_transform_to_freqs(Complex[] complexArray)
        {
            //Debug.WriteLine(complexArray.Length);
            List<PointF> freqs = new List<PointF> { };
            //var Xscale = (complexArray.Length * 1.9135f) / _screenDimensions.X ;
            //double maxIm = 0;
            //var Yscale = _screenDimensions.Y * 100 / complexArray.Length;
            //Single x = 1000F / complexArray.Length;
            for (int i = 0; i < complexArray.Length/20; i++)
            {
                //if (complexArray[i].Imaginary > 1)
                //{
                freqs.Add(new PointF(Convert.ToSingle(Xscale * Math.Sqrt(i)), Convert.ToSingle((complexArray[i].Magnitude) * Yscale  * Math.Sqrt(i * 0.5))));
                //freqs.Add(new PointF(Math.Abs(Convert.ToSingle(complexArray[i].Real) * Yscale), Math.Abs(Convert.ToSingle(complexArray[i].Imaginary) * Yscale)));
                //if (complexArray[i].Imaginary > maxIm) maxIm = complexArray[i].Imaginary;

                //if(complexArray[i+1].Imaginary < 1) break;
                //}
            }

            //Debug.WriteLine(Yscale);
            //or (int i = 0; i < freqs.Count; i++) freqs[i] = new PointF(freqs[i].X, Convert.ToSingle(_screenDimensions.Y - freqs[i].Y * Yscale));
            return freqs.ToArray();
        }

        public void SetInputLength(int samplesNo)
        {
            //Xscale = ((samplesNo * 1.9135f) / _screenDimensions.X) /(samplesNo / 1024f) * (1f/ (samplesNo / 1024f));
            Xscale = (float)(2006450f / (_screenDimensions.X * Math.Sqrt(samplesNo)))* 4.47f;
            Yscale =  _screenDimensions.Y / samplesNo;
            YscaleConst = Yscale;
        }

        private void DrawLine(SpriteBatch spriteBatch, Vector2 begin, Vector2 end, Color color, int width = 1)
        {
            Rectangle r = new Rectangle((int)begin.X, (int)begin.Y, (int)(end - begin).Length() + width, width);
            Vector2 v = Vector2.Normalize(begin - end);
            float angle = (float)Math.Acos(Vector2.Dot(v, -Vector2.UnitX));
            if (begin.Y > end.Y) angle = MathHelper.TwoPi - angle;
            spriteBatch.Draw(_colour, r, null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
        }

    }
}
