using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealTimeVisualiser
{
    /// <summary>
    /// parent class for controls used in the GUI 
    /// </summary>
    public abstract class Component
    {
        public abstract void Draw(GameTime _gameTime, SpriteBatch _spriteBatch);

        public abstract void Update(GameTime gameTime);
    }
}
