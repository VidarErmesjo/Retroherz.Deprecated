namespace MonoGame
{
    public class ExpiryComponent
    {
        public float TimeRemaining { get; set; }
        public bool isPersistent { get; set; }

        public ExpiryComponent(float timeRemaining, bool isPersistent = false)
        {
            TimeRemaining = timeRemaining;
            this.isPersistent = isPersistent;
        }
    }
}