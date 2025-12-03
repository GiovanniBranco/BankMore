namespace BankMore.ContaCorrente.API.Domain.Exceptions;

public class InvalidDocumentException : Exception
{
    public string Type { get; }

    public InvalidDocumentException(string message) : base(message)
    {
        Type = "INVALID_DOCUMENT";
    }
}

public class UnauthorizedUserException : Exception
{
    public string Type { get; }

    public UnauthorizedUserException(string message) : base(message)
    {
        Type = "USER_UNAUTHORIZED";
    }
}

public class InsufficientBalanceException : Exception
{
    public string Type { get; }

    public InsufficientBalanceException(string message) : base(message)
    {
        Type = "INSUFFICIENT_BALANCE";
    }
}

public class DuplicateRequestException : Exception
{
    public string Type { get; }
    public object? PreviousResult { get; }

    public DuplicateRequestException(string message, object? previousResult = null) : base(message)
    {
        Type = "DUPLICATE_REQUEST";
        PreviousResult = previousResult;
    }
}

public class InactiveAccountException : Exception
{
    public string Type { get; }

    public InactiveAccountException(string message) : base(message)
    {
        Type = "INACTIVE_ACCOUNT";
    }
}

public class InvalidValueException : Exception
{
    public string Type { get; }

    public InvalidValueException(string message) : base(message)
    {
        Type = "INVALID_VALUE";
    }
}

public class InvalidTypeException : Exception
{
    public string Type { get; }

    public InvalidTypeException(string message) : base(message)
    {
        Type = "INVALID_TYPE";
    }
}
