namespace Utils.Time
{
    public interface IDateTimeProvider
    {
        public DateTime Now();
        public DateTime NowUtc();
        public DateTimeOffset NowOffset();
        public DateTimeOffset NowOffsetUtc();
    }
}
