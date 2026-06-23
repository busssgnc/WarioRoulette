using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace WarioRoulette.States
{
    public class MenuState : State
    {
        private KeyboardState _currentKey;
        private KeyboardState _prevKey;
        
      

        public MenuState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content) 
            : base(game, graphicsDevice, content)
        {
        }

        public override void Update(GameTime gameTime)
        {
            _currentKey = Keyboard.GetState();

         
            if (_currentKey.IsKeyDown(Keys.Space) && _prevKey.IsKeyUp(Keys.Space))
            {
                _game.ChangeState(new PlayState(_game, _graphicsDevice, _content));
            }

            _prevKey = _currentKey;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            
            _graphicsDevice.Clear(Color.DimGray);
            
            spriteBatch.Begin();
            
            spriteBatch.End();
        }
    }
}