using System;

namespace WashDelivery.Web.Helpers;

public static class DateTimeHelper
{
    private static readonly TimeZoneInfo _polandTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");

    public static string FormatToLocalTime(DateTime utcDateTime)
    {
        if (utcDateTime.Kind != DateTimeKind.Utc)
        {
            utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        }

        var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, _polandTimeZone);
        return localTime.ToString("dd.MM.yyyy HH:mm");
    }
} 