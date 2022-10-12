using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace RealTimeVisualiser
{
    class inputGraph
    {

        private int _sampleRate;
        private Vector2 origin;
        private Vector2 axisLength;
        private Texture2D _colour;
        private Single _timeScale;
        private double distanceMultiplier;
        private Single YAxisMultiplier;


        private List<Single> displayData;
        private List<Single> FFTData;

        public inputGraph( int samplerate,Vector2 screenSize, Texture2D pixelcolour)
        {

            _sampleRate = samplerate;
            origin = new Vector2(Convert.ToInt32(screenSize.X / 54.86), (float)(screenSize.Y -  ((screenSize.Y /4.219) * 0.5))); //origin at bottom right 
            axisLength = new Vector2(screenSize.X - (screenSize.X-origin.X) * 2, screenSize.Y / 4);
            _colour = pixelcolour;
            displayData = new List<Single> ();

            //screensize as a fraction of 1080 x 4.2 x the max amplitude of a sound sample / 512
            YAxisMultiplier = (float)(screenSize.Y * 140/1080);  


            
        }

        public  void Draw(GameTime _gameTime, SpriteBatch _spriteBatch)
        {
            Debug.WriteLine("test3");
            Debug.WriteLine(displayData.Count);
            for(int i = 0; i < displayData.Count; i++)
            {
  
                //Debug.WriteLine(displayData[i]);
                var position = new Rectangle(Convert.ToInt32(origin.X - i * (distanceMultiplier)), Convert.ToInt32(origin.Y - (displayData[i]) * YAxisMultiplier), 1, 1);
                _spriteBatch.Draw(_colour, position, Color.White);
                //Debug.WriteLine(position); 
               
            }
            
        }

        public List<Single> Update(GameTime gameTime, List<Single> newData, Single timeScale, Single FFTSamplesLength)
        {

            var totalSamples = _sampleRate * _timeScale;

            _timeScale = timeScale;
            displayData.AddRange(newData);
            var x = (int)(displayData.Count - totalSamples);
            if(x > 0) displayData.RemoveRange(0, x);
            //Debug.WriteLine(x);
            distanceMultiplier = axisLength.X / totalSamples;

            var i = Convert.ToInt32(FFTSamplesLength / timeScale * displayData.Count);

            FFTData = displayData.GetRange(displayData.Count - i, i);

            return FFTData;
        }

        
        public void _stop()
        {

            displayData = new List<Single> { 0 };
        }
    }
}
