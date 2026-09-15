namespace NewLife.Holiday;

/// <summary>假期扩展</summary>
public static class HolidayExtensions
{
    /// <summary>中国假期</summary>
    public static IHoliday China { get; set; } = new ChinaHoliday();

    /// <summary>广西假期</summary>
    public static IHoliday Guangxi { get; set; } = new GuangxiHoliday();

    /// <summary>是否中国假期（放假无需上班）</summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static Boolean IsChinaHoliday(this DateTime date)
    {
        var inf = China.Query(date).FirstOrDefault();
        if (inf != null)
        {
            switch (inf.Status)
            {
                case HolidayStatus.Normal:
                    break;
                case HolidayStatus.On:
                    return true;
                case HolidayStatus.Off:
                    return false;
                default:
                    break;
            }
        }

        return date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
    }

    /// <summary>是否广西假期（放假无需上班）</summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static Boolean IsGuangxiHoliday(this DateTime date)
    {
        var infs = Guangxi.Query(date).ToList();
        if (infs.Any())
        {
            // 优先广西
            var inf = infs.FirstOrDefault(e => e.Category == "Guangxi");
            inf ??= infs[0];

            switch (inf.Status)
            {
                case HolidayStatus.Normal:
                    break;
                case HolidayStatus.On:
                    return true;
                case HolidayStatus.Off:
                    return false;
                default:
                    break;
            }
        }

        return date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
    }
}