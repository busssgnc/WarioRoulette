using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using System.IO;

namespace WarioRoulette.Objects
{
    public class RouletteWheel
    {
        private GraphicsDevice _graphicsDevice;
        private Texture2D _backgroundTexture;

        private float _eyeScroll;
        private float _noseScroll;
        private float _mouthScroll;

        private float _eyeSpeed = 180f;
        private float _noseSpeed = 220f;
        private float _mouthSpeed = 200f;

        private float _laneStartX = 280f;
        private float _laneEndX = 520f;
        private float _laneWidth;

        private Texture2D _eyeTexture;
        private Texture2D _noseTexture;
        private Texture2D _mouthTexture;
        private Texture2D _lightTexture;
        private Texture2D _coinTexture;
        private Texture2D _pixelTexture;

        private int _lightFrame = 0;
        private float _lightAnimTimer = 0f;

        private int _stopStage = 0;
        private Random _rnd = new Random();
        private int _playerHP = 3;

        private int _money = 206;
        private int _level = 0;
        private int _topLevel = 10;

        private float _moneyScale = 1f;
        private float _levelScale = 1f;

        private bool _waitingNextRound = false;
        private float _nextRoundTimer = 0f;

        public int TargetEyeIndex { get; private set; }
        public int TargetNoseIndex { get; private set; }
        public int TargetMouthIndex { get; private set; }

        public int CurrentEyeIndex { get; private set; }
        public int CurrentNoseIndex { get; private set; }
        public int CurrentMouthIndex { get; private set; }

        public bool IsEyeStopped { get; private set; }
        public bool IsNoseStopped { get; private set; }
        public bool IsMouthStopped { get; private set; }

        public bool IsShowingTarget { get; private set; }
        private float _targetShowTimer;
        private float _targetShowDuration = 3.0f;

        private int _eyeTargetW = 170;
        private int _eyeTargetH = 70;

        private int _noseTargetW = 110;
        private int _noseTargetH = 95;

        private int _mouthTargetW = 190;
        private int _mouthTargetH = 80;

        private Vector2 _eyeTargetPos = new Vector2(400, 295);
        private Vector2 _noseTargetPos = new Vector2(400, 350);
        private Vector2 _mouthTargetPos = new Vector2(400, 400);

        public RouletteWheel(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;

            _pixelTexture = new Texture2D(_graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }

        private Texture2D LoadPngDirectly(string fileName)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", fileName + ".png");

            if (File.Exists(path))
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open))
                {
                    return Texture2D.FromStream(_graphicsDevice, fileStream);
                }
            }

            string altPath = Path.Combine("Content", fileName + ".png");

            if (File.Exists(altPath))
            {
                using (FileStream fileStream = new FileStream(altPath, FileMode.Open))
                {
                    return Texture2D.FromStream(_graphicsDevice, fileStream);
                }
            }

            return null;
        }

        public void LoadContent(ContentManager content)
        {
            _backgroundTexture = LoadPngDirectly("wario_bg");
            _eyeTexture = LoadPngDirectly("wario_eyes");
            _noseTexture = LoadPngDirectly("wario_noses");
            _mouthTexture = LoadPngDirectly("wario_mouths");
            _lightTexture = LoadPngDirectly("Light");
            _coinTexture = LoadPngDirectly("coin");

            _laneWidth = _laneEndX - _laneStartX;

            NextLevel();
        }

        public void ResetGame()
        {
            TargetEyeIndex = _rnd.Next(0, 4);
            TargetNoseIndex = _rnd.Next(0, 4);
            TargetMouthIndex = _rnd.Next(0, 4);

            CurrentEyeIndex = _rnd.Next(0, 4);
            CurrentNoseIndex = _rnd.Next(0, 4);
            CurrentMouthIndex = _rnd.Next(0, 4);

            _eyeScroll = 0;
            _noseScroll = 0;
            _mouthScroll = 0;

            _stopStage = 0;

            IsEyeStopped = false;
            IsNoseStopped = false;
            IsMouthStopped = false;

            IsShowingTarget = true;
            _targetShowTimer = 0f;

            _waitingNextRound = false;
            _nextRoundTimer = 0f;
        }

        private void NextLevel()
        {
            _level++;

            if (_level > _topLevel)
                _topLevel = _level;

            _levelScale = 1.4f;

            ResetGame();
        }

        private bool IsCorrect()
        {
            return CurrentEyeIndex == TargetEyeIndex &&
                   CurrentNoseIndex == TargetNoseIndex &&
                   CurrentMouthIndex == TargetMouthIndex;
        }

        private void TakeDamage()
        {
            _playerHP--;

            if (_playerHP <= 0)
            {
                _playerHP = 3;
                _level = 0;
                NextLevel();
            }
        }

        public void StopNextPart()
        {
            if (IsShowingTarget)
            {
                IsShowingTarget = false;
                return;
            }

            if (_waitingNextRound)
                return;

            if (_stopStage == 0)
            {
                IsEyeStopped = true;
                _stopStage = 1;
                return;
            }

            if (_stopStage == 1)
            {
                IsNoseStopped = true;
                _stopStage = 2;
                return;
            }

            if (_stopStage == 2)
            {
                IsMouthStopped = true;
                _stopStage = 3;

                if (!IsCorrect())
                {
                    _money -= 5;

                    if (_money < 0)
                        _money = 0;

                    _moneyScale = 1f;
                    TakeDamage();
                }
                else
                {
                    _money += 10;
                    _moneyScale = 1f;
                }

                _waitingNextRound = true;
                _nextRoundTimer = 0f;

                return;
            }
        }

        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _lightAnimTimer += elapsed;

            if (_lightAnimTimer >= 0.12f)
            {
                _lightAnimTimer = 0f;
                _lightFrame++;

                if (_lightFrame >= 4)
                    _lightFrame = 0;
            }

            _moneyScale = MathHelper.Lerp(_moneyScale, 1f, 0.1f);
            _levelScale = MathHelper.Lerp(_levelScale, 1f, 0.1f);

            if (_waitingNextRound)
            {
                _nextRoundTimer += elapsed;

                if (_nextRoundTimer >= 2f)
                {
                    NextLevel();
                }

                return;
            }

            if (IsShowingTarget)
            {
                _targetShowTimer += elapsed;

                if (_targetShowTimer >= _targetShowDuration)
                    IsShowingTarget = false;

                return;
            }

            if (!IsEyeStopped)
            {
                _eyeScroll += _eyeSpeed * elapsed;

                if (_eyeScroll >= 200)
                {
                    _eyeScroll = 0;
                    CurrentEyeIndex = (CurrentEyeIndex + 1) % 4;
                }
            }

            if (!IsNoseStopped)
            {
                _noseScroll += _noseSpeed * elapsed;

                if (_noseScroll >= 200)
                {
                    _noseScroll = 0;
                    CurrentNoseIndex = (CurrentNoseIndex + 1) % 4;
                }
            }

            if (!IsMouthStopped)
            {
                _mouthScroll += _mouthSpeed * elapsed;

                if (_mouthScroll >= 200)
                {
                    _mouthScroll = 0;
                    CurrentMouthIndex = (CurrentMouthIndex + 1) % 4;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_backgroundTexture != null)
                spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, 800, 600), Color.White);
               

            DrawUIPanel(spriteBatch, new Rectangle(35, 20, 210, 65));
             if (_coinTexture != null)
            {
                spriteBatch.Draw(_coinTexture,new Rectangle(35, 8, 90, 90),Color.White);
            }
                DrawNumber(spriteBatch, _money, new Vector2(125, 35), _moneyScale);

                DrawUIPanel(spriteBatch, new Rectangle(310, 20, 180, 65));
                DrawNumber(spriteBatch, _level, new Vector2(360, 35), _levelScale);

                DrawUIPanel(spriteBatch, new Rectangle(570, 20, 200, 65));
                DrawNumber(spriteBatch, _topLevel, new Vector2(650, 35), 1f);

                DrawLight(spriteBatch, new Vector2(105, 440));
                DrawLight(spriteBatch, new Vector2(695, 440));

            if (IsShowingTarget)
            {
                DrawOrganResized(spriteBatch, _eyeTexture, _eyeTargetPos, _eyeTargetW, _eyeTargetH, TargetEyeIndex);
                DrawOrganResized(spriteBatch, _noseTexture, _noseTargetPos, _noseTargetW, _noseTargetH, TargetNoseIndex);
                DrawOrganResized(spriteBatch, _mouthTexture, _mouthTargetPos, _mouthTargetW, _mouthTargetH, TargetMouthIndex);
            }
            else
            {
                float centerX = 400f;
                float ovalPower = 35f;

                float eyeProgress = (_eyeScroll % _laneWidth) / _laneWidth;
                float noseProgress = (_noseScroll % _laneWidth) / _laneWidth;
                float mouthProgress = (_mouthScroll % _laneWidth) / _laneWidth;

                float eyeX = IsEyeStopped ? centerX : _laneStartX + eyeProgress * _laneWidth;
                float noseX = IsNoseStopped ? centerX : _laneStartX + noseProgress * _laneWidth;
                float mouthX = IsMouthStopped ? centerX : _laneStartX + mouthProgress * _laneWidth;

                float eyeY = IsEyeStopped ? _eyeTargetPos.Y : _eyeTargetPos.Y + (float)Math.Sin(eyeProgress * Math.PI) * ovalPower;
                float noseY = IsNoseStopped ? _noseTargetPos.Y : _noseTargetPos.Y + (float)Math.Sin(noseProgress * Math.PI) * ovalPower;
                float mouthY = IsMouthStopped ? _mouthTargetPos.Y : _mouthTargetPos.Y + (float)Math.Sin(mouthProgress * Math.PI) * ovalPower;

                DrawOrganResized(spriteBatch, _eyeTexture, new Vector2(eyeX, eyeY), _eyeTargetW, _eyeTargetH, CurrentEyeIndex);
                DrawOrganResized(spriteBatch, _noseTexture, new Vector2(noseX, noseY), _noseTargetW, _noseTargetH, CurrentNoseIndex);
                DrawOrganResized(spriteBatch, _mouthTexture, new Vector2(mouthX, mouthY), _mouthTargetW, _mouthTargetH, CurrentMouthIndex);
            }
        }

        private void DrawLight(SpriteBatch spriteBatch, Vector2 centerPos)
        {
            if (_lightTexture == null)
                return;

            int frameWidth = _lightTexture.Width / 4;
            int frameHeight = _lightTexture.Height;

            Rectangle sourceRect = new Rectangle(_lightFrame * frameWidth,0,frameWidth,frameHeight);

            Rectangle destRect = new Rectangle((int)centerPos.X - 35,(int)centerPos.Y - 170,70,340);

            spriteBatch.Draw(_lightTexture, destRect, sourceRect, Color.White);
        }

        private void DrawUIPanel(SpriteBatch spriteBatch, Rectangle rect)
        {
            spriteBatch.Draw(_pixelTexture, rect, Color.Cyan);
            spriteBatch.Draw(_pixelTexture, new Rectangle(rect.X + 5, rect.Y + 5, rect.Width - 10, rect.Height - 10), Color.DarkGreen);
        }

        private void DrawNumber(SpriteBatch spriteBatch, int number, Vector2 pos, float scale)
        {
            string text = number.ToString("000");

            for (int i = 0; i < text.Length; i++)
            {
                DrawDigit(spriteBatch, text[i] - '0', new Vector2(pos.X + i * 35 * scale, pos.Y), scale);
            }
        }

        private void DrawDigit(SpriteBatch spriteBatch, int digit, Vector2 pos, float scale)
        {
            int w = (int)(22 * scale);
            int h = (int)(36 * scale);
            int t = (int)(5 * scale);

            bool[] seg = new bool[7];

            if (digit == 0) seg = new[] { true, true, true, true, true, true, false };
            if (digit == 1) seg = new[] { false, true, true, false, false, false, false };
            if (digit == 2) seg = new[] { true, true, false, true, true, false, true };
            if (digit == 3) seg = new[] { true, true, true, true, false, false, true };
            if (digit == 4) seg = new[] { false, true, true, false, false, true, true };
            if (digit == 5) seg = new[] { true, false, true, true, false, true, true };
            if (digit == 6) seg = new[] { true, false, true, true, true, true, true };
            if (digit == 7) seg = new[] { true, true, true, false, false, false, false };
            if (digit == 8) seg = new[] { true, true, true, true, true, true, true };
            if (digit == 9) seg = new[] { true, true, true, true, false, true, true };

            Color c = Color.White;

            if (seg[0]) spriteBatch.Draw(_pixelTexture, new Rectangle((int)pos.X, (int)pos.Y, w, t), c);
            if (seg[1]) spriteBatch.Draw(_pixelTexture, new Rectangle((int)pos.X + w - t, (int)pos.Y, t, h / 2), c);
            if (seg[2]) spriteBatch.Draw(_pixelTexture, new Rectangle((int)pos.X + w - t, (int)pos.Y + h / 2, t, h / 2), c);
            if (seg[3]) spriteBatch.Draw(_pixelTexture, new Rectangle((int)pos.X, (int)pos.Y + h, w, t), c);
            if (seg[4]) spriteBatch.Draw(_pixelTexture, new Rectangle((int)pos.X, (int)pos.Y + h / 2, t, h / 2), c);
            if (seg[5]) spriteBatch.Draw(_pixelTexture, new Rectangle((int)pos.X, (int)pos.Y, t, h / 2), c);
            if (seg[6]) spriteBatch.Draw(_pixelTexture, new Rectangle((int)pos.X, (int)pos.Y + h / 2, w, t), c);
        }

        private void DrawOrganResized(SpriteBatch spriteBatch, Texture2D texture, Vector2 centerPosition, int targetW, int targetH, int frameIndex)
        {
            if (texture == null) return;

            int frameWidth = texture.Width / 4;
            int frameHeight = texture.Height;

            Rectangle sourceRect = new Rectangle(frameIndex * frameWidth, 0, frameWidth, frameHeight);

            Rectangle destRect = new Rectangle((int)(centerPosition.X - (targetW / 2f)),(int)(centerPosition.Y - (targetH / 2f)),targetW, targetH);

            spriteBatch.Draw(texture, destRect, sourceRect, Color.White);
        }
    }
}