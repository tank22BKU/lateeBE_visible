namespace VirtualPatientService.Application.Commands.FetchDiscoveryCases;

public class LearnerNotFoundException : Exception
{
    public LearnerNotFoundException(string message)
        : base(message) { }
}

public class NoMoreCasesAvailableException : Exception
{
    public NoMoreCasesAvailableException(string message)
        : base(message) { }
}

public class FetchCasesValidationException : Exception
{
    public FetchCasesValidationException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public string ErrorCode { get; }
}
