namespace BuildingBlocks.SharedKernel;

public record Error
{
    public string Code { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public int Status { get; private set; } = 0;


    protected internal Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    protected internal Error(string code, string message, int status)
    {
        Code = code;
        Message = message;
        Status = status;
    }
    
    
    
    public static Error Create(string code,string message) => new(code, message);
    
    public static Error Create(string code,string message, int status) => new(code, message, status);
    
    public static implicit operator Error (string message) => new(string.Empty, message);
}



public static class ErrorCache
{
    public static readonly Error None = new(string.Empty, string.Empty);
    
    public static readonly Error Unauthorized = new("Error.Unauthorized", "Unauthorized",401);
    
    public static readonly Error BadRequest = new("Error.BadRequest", "Bad Request",400);
    
    public static readonly Error NotFound = new("Error.NotFound", "Not Found",404);
    
    public static readonly Error Forbidden = new("Error.Forbidden", "Forbidden",403);
}