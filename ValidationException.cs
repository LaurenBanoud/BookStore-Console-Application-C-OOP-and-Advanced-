namespace BookStoreApp.Validation;

/// <summary>
/// A single exception type for every "bad input" scenario in the domain and
/// the UI layer. The console menu catches this specifically to print a
/// friendly message instead of a stack trace.
/// </summary>
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
    }
}
