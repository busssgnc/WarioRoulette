namespace WarioRoulette.Managers
{
    public class ScoreManager
    {
        public int Score { get; private set; }
        public int Lives { get; private set; }
        public bool IsGameOver => Lives <= 0;

        public ScoreManager(int startingLives = 3)
        {
            Lives = startingLives;
            Score = 0;
        }

        public void AddPoint()
        {
            Score += 100;
        }

        public void TakeDamage()
        {
            Lives--;
            if (Lives < 0) Lives = 0;
        }

        public void Reset()
        {
            Score = 0;
            Lives = 3;
        }
    }
}