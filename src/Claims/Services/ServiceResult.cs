namespace Claims.Services;

public sealed class ServiceResult<T>
{
    private ServiceResult(T? value, Dictionary<string, string[]> errors)
    {
        Value = value;
        Errors = errors;
    }

    public T? Value { get; }
    public Dictionary<string, string[]> Errors { get; }
    public bool Succeeded => Errors.Count == 0;

    public static ServiceResult<T> Success(T value) => new(value, []);
    public static ServiceResult<T> Failure(Dictionary<string, string[]> errors) => new(default, errors);
}
