using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using WarioRoulette.Objects;
using WarioRoulette.Managers;
using System;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace WarioRoulette.States
{
    public class PlayState : State
    {
        private KeyboardState _currentKey;
        private KeyboardState _prevKey;

        private Texture2D _gameStartTexture;
        private Texture2D _gameOverTexture;

        private bool _gameStarted;
        private float _startAnimTimer;

        private RouletteWheel _rouletteWheel;
        private ScoreManager _scoreManager;

        private Texture2D _pixelTexture;
        private bool _roundEnded;
        private SoundEffect _clickSound;
        private Song _mainMusic;
        private Song _gameOverMusic;
        private bool _gameOverMusicPlayed;

        public PlayState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
            : base(game, graphicsDevice, content)
        {
            _rouletteWheel = new RouletteWheel(_graphicsDevice);
            _rouletteWheel.LoadContent(_content);

            _scoreManager = new ScoreManager(1);

            _pixelTexture = new Texture2D(_graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });

            _gameStartTexture = _content.Load<Texture2D>("GameStart");
            _gameOverTexture = _content.Load<Texture2D>("GameOver");

            _clickSound = _content.Load<SoundEffect>("click");

            _mainMusic = _content.Load<Song>("MainMusic");

            _gameOverMusic = _content.Load<Song>("GameOverMusic");

            _gameOverMusicPlayed = false;

            _gameStarted = false;
            _startAnimTimer = 0f;
            _roundEnded = false;

            _rouletteWheel.ResetGame();
        }

        public override void Update(GameTime gameTime)
        {
            _currentKey = Keyboard.GetState();

            if (_currentKey.IsKeyDown(Keys.R) && _prevKey.IsKeyUp(Keys.R))
            {
                  MediaPlayer.Stop();
                _game.ChangeState(new PlayState(_game, _graphicsDevice, _content));
                return;
            }

            _startAnimTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!_gameStarted)
            {
                if (_currentKey.IsKeyDown(Keys.Space) &&
                    _prevKey.IsKeyUp(Keys.Space))
                {
                    _gameStarted = true;
                    MediaPlayer.IsRepeating = true;
                    MediaPlayer.Play(_mainMusic);
                }

                _prevKey = _currentKey;
                return;
            }

            if (_scoreManager.IsGameOver)
            {
                if (!_gameOverMusicPlayed)
                {
                    MediaPlayer.Stop();

                    MediaPlayer.IsRepeating = true;
                    MediaPlayer.Play(_gameOverMusic);

                    _gameOverMusicPlayed = true;
                }
                _prevKey = _currentKey;
                return;
            }

            _rouletteWheel.Update(gameTime);

            if (_currentKey.IsKeyDown(Keys.Space) && _prevKey.IsKeyUp(Keys.Space))
            {
                _clickSound.Play(); 
                if (!_rouletteWheel.IsShowingTarget)
                {
                    if (!_roundEnded)
                    {
                        _rouletteWheel.StopNextPart();

                        if (_rouletteWheel.IsEyeStopped &&
                            _rouletteWheel.IsNoseStopped &&
                            _rouletteWheel.IsMouthStopped)
                        {
                            _roundEnded = true;
                            CheckResults();
                        }
                    }
                    else
                    {
                        NextRound();
                    }
                }
            }

            _prevKey = _currentKey;
        }

        private void CheckResults()
        {
            if (_rouletteWheel.CurrentEyeIndex == _rouletteWheel.TargetEyeIndex &&
                _rouletteWheel.CurrentNoseIndex == _rouletteWheel.TargetNoseIndex &&
                _rouletteWheel.CurrentMouthIndex == _rouletteWheel.TargetMouthIndex)
            {
                _scoreManager.AddPoint();
            }
            else
            {
                _scoreManager.TakeDamage();
            }
        }

        private void NextRound()
        {
            _roundEnded = false;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _graphicsDevice.Clear(Color.MidnightBlue);

            spriteBatch.Begin();

            _rouletteWheel.Draw(spriteBatch);

            float scale = 1f + (float)Math.Sin(_startAnimTimer * 6f) * 0.08f;
            int width = (int)(600 * scale);
            int height = (int)(120 * scale);

            Rectangle logoRect = new Rectangle(
                (800 - width) / 2,
                230,
                width,
                height);

            if (!_gameStarted)
            {
                spriteBatch.Draw(_gameStartTexture, logoRect, Color.White);
                spriteBatch.End();
                return;
            }

            if (_scoreManager.IsGameOver)
            {
                spriteBatch.Draw(_gameOverTexture, logoRect, Color.White);
                spriteBatch.End();
                return;
            }

            spriteBatch.End();
        }
    }
}