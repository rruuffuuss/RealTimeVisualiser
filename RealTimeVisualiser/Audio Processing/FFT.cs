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


    class FastFourierTransform
    {
        private Vector2 _screenDimensions;
        private Vector2 _graphOrigin;
        private PointF[] _realTransform;
        private Texture2D _colour;

        private float Xscale;
        private float YscaleConst;
        private float Yscale;

        private float _pitchBias;

        public FastFourierTransform(Vector2 ScreenDimensions, Texture2D pixelcolour)
        {
            //_graphOrigin = new Vector2( 470f / 1920f * 1920 , 821f / 1080f * ScreenDimensions.Y);

            //scale origin and size to screensize
            _graphOrigin = new Vector2(470f / 1920f * ScreenDimensions.X, 820f / 1080f * ScreenDimensions.Y);
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

                    //draw lines between current point and last point
                    var pos1 = new Vector2((int)(p.X + _graphOrigin.X), (int)(_graphOrigin.Y - Math.Abs(p.Y)));
                    DrawLine(_spriteBatch, pos0, pos1, Color.White);
                    pos0 = pos1;
                }
            }



        }

        public void FFTForwardtoPoints(Single[] samples, float amplitude, float bias)
        {
            //scale to values from sliders
            Yscale = YscaleConst * amplitude * 10;
            _pitchBias = bias * 5;

            var complexSamples = toComplexArray(samples);
            var transform = FFT(complexSamples);
            _realTransform = complex_transform_to_freqs(transform);
            //Debug.WriteLine(string.Join(',', _realTransform));
            //Debug.WriteLine(string.Join(',', transform));

        }

        /// <summary>
        /// calculates the DFT of the iput samples
        /// </summary>
        /// <param name="samples"></param>
        /// <returns>array of points of DFT</returns>
        private Complex[] FFT(Complex[] samples)
        {

            Complex CONST_i = new Complex(0, 1);

            int n = samples.Length;
            if (n == 1) return samples;
            //else if (!IsPowerOfTwo(n)) { throw new ArgumentOutOfRangeException(); }

            //o = first root of unity for array of length n
            Complex o = Complex.Pow(Math.E, (2 * Math.PI * CONST_i) / n);

            //splits samples into even and odd
            Complex[] samples_e = samples.Where((x, i) => i % 2 == 0).ToArray();
            Complex[] samples_o = samples.Where((x, i) => i % 2 != 0).ToArray();

            //FFTs both arrays
            Complex[] transform_e = FFT(samples_e);
            Complex[] transform_o = FFT(samples_o);

            Complex[] transform = new Complex[n];

            //combines even and odd FFTs back into single array
            for (int i = 0; i < n / 2; i++)
            {
                transform[i] = transform_e[i] + Complex.Pow(o, i) * transform_o[i];
                transform[i + n / 2] = transform_e[i] - Complex.Pow(o, i) * transform_o[i];
            }

            return transform;

        }

        /// <summary>
        /// converts and array of real numbers to an array of real numbers
        /// </summary>
        /// <param name="singleArray"></param>
        /// <returns>an array of complex numbers</returns>
        private Complex[] toComplexArray(Single[] singleArray)
        {
            Complex[] complexArray = new Complex[singleArray.Length];

            for (int i = 0; i < singleArray.Length; i++)
            {
                complexArray[i] = new Complex(singleArray[i], 0);
            }

            return complexArray;
        }

        /// <summary>
        /// checks if number is power of 2
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        static bool IsPowerOfTwo(int d)
        {
            ulong x = Convert.ToUInt64(d);
            return (x != 0) && ((x & (x - 1)) == 0);
        }

        //used for testing only
        /// <summary>
        /// parses a string to an array of doubles
        /// </summary>
        /// <param name="stringSamples"></param>
        /// <returns>an array of doubles</returns>
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

        //used for testing only
        /// <summary>
        /// prints double array
        /// </summary>
        /// <param name="array"></param>
        private void printDoubleArray(double[] array)
        {
            foreach (double i in array)
            {
                Console.WriteLine(Convert.ToString(i));
            }
        }

        /// <summary>
        /// //scales and crops the complex transform to a scale and range that is appropriate for music and human listening
        /// </summary>
        /// <param name="complexArray"></param>
        /// <returns>an array of points</returns>
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
                var x = Convert.ToSingle(Xscale * Math.Sqrt(i));
                var y = Yscale * Math.Sqrt(i * _pitchBias);
                freqs.Add(new PointF(x, Convert.ToSingle(y * complexArray[i].Imaginary)));

                //freqs.Add(new PointF(Math.Abs(Convert.ToSingle(complexArray[i].Real) * Yscale), Math.Abs(Convert.ToSingle(complexArray[i].Imaginary) * Yscale)));
                //if (complexArray[i].Imaginary > maxIm) maxIm = complexArray[i].Imaginary;

                //if(complexArray[i+1].Imaginary < 1) break;
                //}
            }

            //Debug.WriteLine(Yscale);
            //or (int i = 0; i < freqs.Count; i++) freqs[i] = new PointF(freqs[i].X, Convert.ToSingle(_screenDimensions.Y - freqs[i].Y * Yscale));
            return freqs.ToArray();
        }

        /// <summary>
        /// plans the X and Y scales to be used for this execution of the game 
        /// </summary>
        /// <param name="samplesNo"></param>
        public void SetInputLength(int samplesNo)
        {
            //Xscale = ((samplesNo * 1.9135f) / _screenDimensions.X) /(samplesNo / 1024f) * (1f/ (samplesNo / 1024f));
            Xscale = (float)(2006450f / (_screenDimensions.X * Math.Sqrt(samplesNo)))* 4.47f;
            Yscale =  _screenDimensions.Y / samplesNo;
            YscaleConst = Yscale;
        }

        /// <summary>
        /// draws a line between 2 points
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="color"></param>
        /// <param name="width"></param>
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
