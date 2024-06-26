namespace YATsDb.Core;

public class YatsdbSyntaxException : YatsdbException
{
    public YatsdbSyntaxException()
    {
    }

    public YatsdbSyntaxException(string? message)
        : base(message)
    {
    }

    public YatsdbSyntaxException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
