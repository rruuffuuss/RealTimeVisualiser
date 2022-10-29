using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealTimeVisualiser
{
    public class Button : Component
    {

        #region Fields
        private MouseState _currentMouse;
        private bool _isHovering;
        private MouseState _previousMouse;

        private Texture2D _texture;
        private SpriteFont _font;
        #endregion

        #region Properties

        public EventHandler click;
        public bool clicked { get; private set; }

        public Color penColour { get; set; }

        public Vector2 position { get; set; }

        public Rectangle rectangle
        {
            get
            {
                return new Rectangle((int)position.X, (int)position.Y, _texture.Width, _texture.Height);
            }
        }

        public string text { get; set; }

        #endregion

        #region Methods

        public Button (Texture2D texture, SpriteFont font)
        {
            _texture = texture;
            _font = font;
        }

        //draws button at location, color dependent on whether mouse is hovering
        public override void Draw(GameTime _gameTime, SpriteBatch _spriteBatch)
        {

            var colour = (_isHovering) ? Color.Gray : Color.White; // chooses button colour based on mouse hover

            _spriteBatch.Draw(_texture, rectangle, colour); // draws button

            if (!string.IsNullOrEmpty(text))
            {
                var x = (rectangle.X + (rectangle.Width / 2) - _font.MeasureString(text).X / 2);
                var y = (rectangle.Y + (rectangle.Height / 2) - _font.MeasureString(text).Y / 2);

                _spriteBatch.DrawString(_font, text, new Vector2(x, y), penColour);// draws button text
            }

            
          
        }

        //checks if mouse is hovering, then invokes click event if mouse leftbutton pressed
        public override void Update(GameTime gameTime)
        {
            _previousMouse = _currentMouse;
            _currentMouse = Game1.CURRENTMOUSE;

            var mouseRectangle = new Rectangle(_currentMouse.X, _currentMouse.Y, 1, 1);

            _isHovering = false;

            if (mouseRectangle.Intersects(rectangle))
            {
                _isHovering = true;
                Game1.HOVERING = true;

                if(_currentMouse.LeftButton == ButtonState.Released && _previousMouse.LeftButton == ButtonState.Pressed)
                {
                    click?.Invoke(this, new EventArgs());
                }
            }

        }

        #endregion
    }
}
