namespace Application.Shared.Exceptions
{
    public class IdentityException : ApiException
    {
        public IdentityException(string message) : base("Identity Error", message) { }

        public IdentityException(IReadOnlyDictionary<string, string[]> errorsDictionary) : base("Identity Error",
            "One or more Identity errors occurred")
        {
            ErrorsDictionary = errorsDictionary;
        }
        public IReadOnlyDictionary<string, string[]>? ErrorsDictionary { get; }
    }
}
