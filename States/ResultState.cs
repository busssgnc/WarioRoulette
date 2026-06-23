using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace WarioRoulette.States
{
    public class ResultState : State
    {
        private KeyboardState _currentKey;
        private KeyboardState _prevKey;
        
        
        private int _finalScore;
        private Texture2D _pixelTexture;

        public ResultState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content, int score) 
            : base(game, graphicsDevice, content)
        {
            _finalScore = score;

            _pixelTexture = new Texture2D(_graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
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
            
            _graphicsDevice.Clear(Color.DarkRed);

            spriteBatch.Begin();

           
           
            spriteBatch.Draw(_pixelTexture, new Rectangle(380, 280, 40, 40), Color.Yellow);

            spriteBatch.End();
        }
    }
}