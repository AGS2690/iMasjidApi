namespace iMasjid.Domain.Exceptions;

public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message = "Authentication failed or token is invalid.") : base(message)
    {
    }
}
