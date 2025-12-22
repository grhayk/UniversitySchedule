using Domain.Interfaces;

namespace Infrastructure.Implementations
{
    public class SystemClock : IClock
    {
        public DateTime Now => DateTime.Now;

        public DateTime UtcNow => DateTime.UtcNow;
    }
}
