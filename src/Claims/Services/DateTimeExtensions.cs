namespace Claims.Services;

public static class DateTimeExtensions
{
    public static DateTime ToUtcDate(this DateTime value) => DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);
}
