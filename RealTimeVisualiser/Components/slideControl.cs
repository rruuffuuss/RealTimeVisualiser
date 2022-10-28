using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace RealTimeVisualiser
{
    public class Slide : Component
    {

        #region Fields
        private MouseState _currentMouse;
        private bool _isHovering;
        private MouseState _previousMouse;

        private Texture2D _slitTexture;
        private Texture2D _handleTexture;
        #endregion

        #region Properties

    
        public bool clicked { get; private set; }

        public Color penColour { get; set; }

        public Vector2 position { get; set; }
        public Vector2 dimensions { get; set; }
        public float slitWidth { get; set; }
        public float handleWidth { get; set; }

        public float distance { get; set; }

        public Rectangle slitRectangle
        {
            get
            {
                return new Rectangle((int)position.X, (int)(position.Y + dimensions.Y / 2), (int)dimensions.X, (int)slitWidth);
            }
        }
        public Rectangle handleRectangle
        {
            get
            {
                return new Rectangle((int)(position.X - handleWidth / 2 + dimensions.X * distance), (int)position.Y, (int)handleWidth, (int)dimensions.Y);
            }
        }


        #endregion

        #region Methods

        public Slide(Texture2D slitTexture, Texture2D handleTexture)
        {
            _slitTexture = slitTexture;
            _handleTexture = handleTexture;
        }

        //draws button at location, color dependent on whether mouse is hovering
        public override void Draw(GameTime _gameTime, SpriteBatch _spriteBatch)
        {

            var colour = (_isHovering) ? Color.Gray : Color.White; // chooses button colour based on mouse hover

            _spriteBatch.Draw(_slitTexture, slitRectangle, Color.White); // draws button

            _spriteBatch.Draw(_handleTexture, handleRectangle, colour);



        }

        //checks if mouse is hovering, then invokes click event if mouse leftbutton pressed
        public override void Update(GameTime gameTime)
        {
            _previousMouse = _currentMouse;
            _currentMouse = Game1.CURRENTMOUSE;

            var mouseRectangle = new Rectangle(_currentMouse.X, _currentMouse.Y, 1, 1);
            var oldMouseRectangle = new Rectangle(_previousMouse.X, _previousMouse.Y, 1, 1);

            _isHovering = false;

            if (mouseRectangle.Intersects(handleRectangle) || (_currentMouse.LeftButton == ButtonState.Pressed && oldMouseRectangle.Intersects(handleRectangle)))
            {
                _isHovering = true;
                Game1.HOVERING = true;

                if (_currentMouse.LeftButton == ButtonState.Pressed & _previousMouse.LeftButton == ButtonState.Pressed)
                {
                    var x = handleRectangle.X + handleRectangle.Width/2 + _currentMouse.X - _previousMouse.X + 1;
                    
                    if(x < slitRectangle.X) x = slitRectangle.X;
                    else if (x > slitRectangle.X + slitRectangle.Width) x = slitRectangle.X + slitRectangle.Width;
                    
                    distance = (float)(x - slitRectangle.X) / (float)slitRectangle.Width;
                }
            }
            Debug.WriteLine(distance);
        }

        #endregion
    }
}
