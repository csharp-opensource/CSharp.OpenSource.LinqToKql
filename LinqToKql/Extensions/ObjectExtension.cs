﻿using System.Collections;

namespace CSharp.OpenSource.LinqToKql.Extensions;

public static class ObjectExtension
{
    public static string GetKQLValue<T>(this T? value)
        => value switch
        {
            bool boolean => boolean.ToString().ToLower(),
            TimeOnly time => $"timespan({time:HH:mm:ss.f})",
            TimeSpan timeSpan => $"timespan({timeSpan.Days}.{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2})",
            DateTime dateTime => $"datetime({dateTime:yyyy-MM-dd HH:mm:ss.f})",
            DateOnly date => $"datetime({date:yyyy-MM-dd})",
            string str => $"'{str.Replace("'", "\\'")}'",
            IEnumerable enumerable => $"({string.Join(", ", enumerable.Cast<object>().Select(GetKQLValue))})",
            _ => value?.ToString() ?? "dynamic(null)",
        };

    public static bool KqlLike(this string str, string pattern, char wildCardSymbol = '%')
    {
        throw new Exception("This method should not be called. It is used for translation purposes only.");
    }
}
