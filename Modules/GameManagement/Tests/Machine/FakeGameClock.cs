namespace AbstractPixel.GameManagement.Tests
{
    /// <summary>A clock the test moves by hand.</summary>
    public class FakeGameClock : IGameClock
    {
        public double Now { get; private set; }

        public void Advance(double _seconds)
        {
            Now += _seconds;
        }
    }
}
