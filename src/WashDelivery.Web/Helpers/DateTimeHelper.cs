using System;

namespace WashDelivery.Web.Helpers;

public static class DateTimeHelper
{
    private static readonly TimeZoneInfo _polandTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");

    public static string FormatToLocalTime(DateTime dateTime)
    {
        DateTime localTime;

        if (dateTime.Kind == DateTimeKind.Utc)
        {
            localTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, _polandTimeZone);
        }
        else if (dateTime.Kind == DateTimeKind.Local)
        {
            localTime = dateTime;
        }
        else // Unspecified
        {
            // Assume it's already in local time (Poland timezone)
            localTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
        }

        return localTime.ToString("dd.MM.yyyy HH:mm");
    }
} 