namespace YATsDb.Core;

public class YatsdbDataException : YatsdbException
{
    public YatsdbDataException()
    {
    }

    public YatsdbDataException(string? message) : base(message)
    {
    }

    public YatsdbDataException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}