using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealTimeVisualiser
{
    /// <summary>
    /// draws custom cursor
    /// </summary>
    class Cursor : Component
    {


        private MouseState _currentMouse;

        private Texture2D normalTexture;
        private Texture2D hoverTexture;

        private int cursorWidth;
        private int cursorHeight;   

        public bool hovering { get; set; }


        public Cursor(Texture2D _normal, Texture2D _hover)
        {
            normalTexture = _normal;
            hoverTexture = _hover;

            cursorWidth = normalTexture.Width;
            cursorHeight = normalTexture.Height;
        }

        //draws the custom cursor at coords of the system cursor, different texture depending on whether cursor is hovering
        public override void Draw(GameTime _gameTime, SpriteBatch _spriteBatch)
        {
            if (Game1.HOVERING)
            {
                _spriteBatch.Draw(hoverTexture, new Rectangle(Game1.CURRENTMOUSE.X- cursorWidth/2, Game1.CURRENTMOUSE.Y - cursorHeight/2, cursorWidth,cursorHeight),Color.White);
            }
            else
            {
                _spriteBatch.Draw(normalTexture, new Rectangle(Game1.CURRENTMOUSE.X - cursorWidth/2, Game1.CURRENTMOUSE.Y - cursorHeight/2, cursorWidth, cursorHeight), Color.White);
            }        
        }
        public override void Update(GameTime gameTime)
        {
            _currentMouse = Mouse.GetState();
            
        }
    }
}
