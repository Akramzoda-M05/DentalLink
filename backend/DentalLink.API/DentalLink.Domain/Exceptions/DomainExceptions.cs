namespace DentalLink.Domain.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    protected DomainException(string message, Exception innerException) : base(message, innerException) { }
}

public class NotFoundException : DomainException
{
    public NotFoundException(string entityName, object key)
        : base($"Entity '{entityName}' with key ({key}) was not found.") { }

    public NotFoundException(string message) : base(message) { }
}

public class ConflictException : DomainException
{
    public ConflictException(string message) : base(message) { }
}

public class ForbiddenException : DomainException
{
    public ForbiddenException(string message = "You do not have permission to access this resource.")
        : base(message) { }
}

public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message = "Authentication required or invalid credentials.")
        : base(message) { }
}

public class ValidationException : DomainException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation failures have occurred.")
    {
        Errors = errors;
    }

    public ValidationException(string propertyName, string errorMessage)
        : base(errorMessage)
    {
        Errors = new Dictionary<string, string[]>
        {
            [propertyName] = [errorMessage]
        };
    }
}
