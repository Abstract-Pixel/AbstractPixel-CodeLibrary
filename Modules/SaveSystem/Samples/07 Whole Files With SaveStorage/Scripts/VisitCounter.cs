using AbstractPixel.SaveSystem;

namespace AbstractPixel.SaveSystem.Samples.WholeFiles
{
    // A little saved data, so the GameProfile file has something in it to look at.
    public class VisitCounter
    {
        [Save] int visits;

        public int Visits => visits;

        public VisitCounter()
        {
            SaveActions.Register(this);
        }

        public void CountAVisit()
        {
            visits++;
        }
    }
}
