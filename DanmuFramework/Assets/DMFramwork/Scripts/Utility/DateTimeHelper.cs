using System;

/// <summary>
///     这是一个时间工具类，存放着一些时间相关的方法
/// </summary>
public class DateTimeHelper
{
    /// <summary>
    ///     这是一个转换时间戳为本地时间的方法
    /// </summary>
    /// <param name="timeStamp">时间戳</param>
    /// <returns></returns>
    public static DateTime GetTimeByTimeStamp(long timeStamp)
    {
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var dateTime = epoch.AddSeconds(timeStamp);
        var localDateTime = dateTime.ToLocalTime();
        return localDateTime;
    }
}