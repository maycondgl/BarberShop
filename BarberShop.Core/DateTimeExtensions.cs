namespace BarberShop.Core.Extensions;

public static class DateTimeExtensions
{
    public static DateTime GetFirstDayOfMonth(this DateTime date)
        => new DateTime(date.Year, date.Month, 1);

    public static DateTime GetFirstDayOfMonth(this DateTime date, int month, int weekOfMonth)
    {
        var normalizedWeek = Math.Max(1, weekOfMonth);
        var firstDayOfMonth = new DateTime(date.Year, month, 1);
        var day = ((normalizedWeek - 1) * 7) + 1;

        return day > DateTime.DaysInMonth(date.Year, month)
            ? firstDayOfMonth
            : new DateTime(date.Year, month, day);
    }

    public static DateTime GetLastDayOfMonth(this DateTime date)
        => new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));

    public static DateTime GetLastDayOfMonth(this DateTime date, int month, int weekOfMonth)
    {
        var firstDay = date.GetFirstDayOfMonth(month, weekOfMonth);
        var lastDayOfMonth = DateTime.DaysInMonth(date.Year, month);
        var lastDayOfWeek = Math.Min(lastDayOfMonth, firstDay.Day + 6);

        return new DateTime(date.Year, month, lastDayOfWeek);
    }
}