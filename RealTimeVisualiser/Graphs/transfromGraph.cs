using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using MathNet.Numerics.IntegralTransforms;

namespace RealTimeVisualiser
{
    class transfromGraph
    {
        private int _sampleRate;
        private Vector2 origin;
        private Vector2 axisLength;

        public transfromGraph(int samplerate, Vector2 screenSize, Texture2D pixelcolour)
        {
            origin.X = Convert.ToInt32(1920 / screenSize.X * 470);
            origin.Y = Convert.ToInt32(1080 / screenSize.Y * 423.5);
        }

        public Single[] FFT(Single[] data)
        {
            //return Fourier.ForwardReal(data, data.Length + 2, ToString());
            return data;
        }
    }
}
