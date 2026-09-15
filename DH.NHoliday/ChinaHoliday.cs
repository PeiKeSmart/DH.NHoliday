using System.Globalization;
using System.Reflection;
using NewLife.IO;

namespace NewLife.Holiday;

/// <summary>中国假期</summary>
public class ChinaHoliday : IHoliday
{
    #region 属性
    /// <summary>所有假期信息</summary>
    public IList<HolidayInfo> Infos { get; set; }
    #endregion

    #region 构造
    /// <summary>实例化</summary>
    public ChinaHoliday()
    {
        Load("China");

        // 排序，先按照日期排，再按照状态排，便于放假优先覆盖调休
        if (Infos is List<HolidayInfo> list) Infos = list.OrderBy(x => x.Date).ThenBy(e => e.Status).ToList();
    }

    /// <summary>加载指定前缀的资源</summary>
    /// <param name="category"></param>
    protected virtual void Load(String category)
    {
        // 加载所有嵌入式资源
        var asm = Assembly.GetExecutingAssembly();
        var names = asm.GetManifestResourceNames();

        foreach (var item in names)
        {
            if (item.EndsWithIgnoreCase(".csv") && item.Contains($".{category}."))
            {
                var ms = asm.GetManifestResourceStream(item);
                Load(ms, category);
            }
        }
    }
    #endregion

    #region 方法
    /// <summary>加载Csv数据流中的数据</summary>
    /// <param name="stream"></param>
    /// <param name="category"></param>
    public void Load(Stream stream, String category)
    {
        using var csv = new CsvFile(stream, true);

        var list = Infos ??= [];

        while (true)
        {
            var line = csv.ReadLine();
            if (line == null) break;
            if (line.Length == 0 || line[0].EqualIgnoreCase("Name")) continue;

            var info = new HolidayInfo
            {
                Name = line[0],
                Category = category,
                Date = line[1].ToDateTime().Date,
                Days = line[2].ToInt(),
                Status = (HolidayStatus)line[3].ToInt(),
            };

            // 有效假期数据
            if (info.Date.Year > 1000)
            {
                if (info.Days <= 0) info.Days = 1;

                list.Add(info);
            }
        }
    }

    /// <summary>查询指定日期的假期信息</summary>
    /// <param name="date">指定日期</param>
    /// <returns>假期信息</returns>
    public virtual IEnumerable<HolidayInfo> Query(DateTime date)
    {
        var list = Infos;
        if (list == null || list.Count == 0) yield break;

        // 列表数据少于1000行，直接遍历性能很快
        date = date.Date;
        var count = 0;
        foreach (var inf in list)
        {
            // 后面的数据不会再匹配
            if (inf.Date > date) break;

            if (inf.Date == date)
            {
                count++;
                yield return inf;
            }

            // 多天
            if (inf.Days > 1)
            {
                var d = (Int32)(date - inf.Date).TotalDays;
                if (d >= 0 && d < inf.Days)
                {
                    count++;
                    yield return inf;
                }
            }
        }

        // 如果前面没有匹配，强制性假期
        if (count == 0)
        {
            if (TryGetYuandan(date, out var holiday)) yield return holiday;
            if (TryGetQingming(date, out holiday)) yield return holiday;
            if (TryGetLaodong(date, out holiday)) yield return holiday;
            if (TryGetGuoqing(date, out holiday)) yield return holiday;
            if (TryGetChunjie(date, out holiday)) yield return holiday;
            if (TryGetDuanwu(date, out holiday)) yield return holiday;
            if (TryGetZhongqiu(date, out holiday)) yield return holiday;
        }
    }

    /// <summary>尝试获取元旦假期</summary>
    /// <param name="date"></param>
    /// <param name="holiday"></param>
    /// <returns></returns>
    public Boolean TryGetYuandan(DateTime date, out HolidayInfo holiday)
    {
        holiday = null;
        if (date.Month != 1 || date.Day != 1) return false;

        holiday = new HolidayInfo
        {
            Name = "元旦",
            Date = date.Date,
            Days = 1,
            Status = HolidayStatus.On
        };

        return true;
    }

    /// <summary>尝试获取清明节假期</summary>
    /// <param name="date"></param>
    /// <param name="holiday"></param>
    /// <returns></returns>
    public Boolean TryGetQingming(DateTime date, out HolidayInfo holiday)
    {
        holiday = null;
        if (date.Month != 4 || date.Day != 5) return false;

        holiday = new HolidayInfo
        {
            Name = "清明节",
            Date = date.Date,
            Days = 1,
            Status = HolidayStatus.On
        };

        return true;
    }

    /// <summary>尝试获取劳动节假期（2025年及以后：5月1-2日，共2天；之前：5月1日，共1天）</summary>
    /// <param name="date"></param>
    /// <param name="holiday"></param>
    /// <returns></returns>
    public Boolean TryGetLaodong(DateTime date, out HolidayInfo holiday)
    {
        holiday = null;
        if (date.Month != 5) return false;

        var year = date.Year;
        if (year >= 2025)
        {
            // 2025 起：5/1-5/2 两天
            if (date.Day != 1 && date.Day != 2) return false;

            var start = new DateTime(year, 5, 1);
            holiday = new HolidayInfo
            {
                Name = "劳动节",
                Date = start,
                Days = 2,
                Status = HolidayStatus.On
            };
            return true;
        }
        else
        {
            // 2024 及之前：仅 5/1 一天
            if (date.Day != 1) return false;

            holiday = new HolidayInfo
            {
                Name = "劳动节",
                Date = date.Date,
                Days = 1,
                Status = HolidayStatus.On
            };
            return true;
        }
    }

    /// <summary>尝试获取国庆节假期（10月1-3日，共3天）</summary>
    /// <param name="date"></param>
    /// <param name="holiday"></param>
    /// <returns></returns>
    public Boolean TryGetGuoqing(DateTime date, out HolidayInfo holiday)
    {
        holiday = null;
        if (date.Month != 10 || (date.Day < 1 || date.Day > 3)) return false;

        var start = new DateTime(date.Year, 10, 1);
        holiday = new HolidayInfo
        {
            Name = "国庆节",
            Date = start,
            Days = 3,
            Status = HolidayStatus.On
        };

        return true;
    }

    /// <summary>尝试获取春节假期（农历除夕至正月初三，共4天）</summary>
    /// <param name="date"></param>
    /// <param name="holiday"></param>
    /// <returns></returns>
    public Boolean TryGetChunjie(DateTime date, out HolidayInfo holiday)
    {
        holiday = null;

        var cal = new ChineseLunisolarCalendar();
        var year = cal.GetYear(date);
        var leapMonth = cal.GetLeapMonth(year);

        var lastMonth = leapMonth > 0 ? 13 : 12;
        var month = cal.GetMonth(date);
        if (month != lastMonth && month != 1) return false;

        // 有些年十二月最后一天是二十九，没有三十
        var day = cal.GetDayOfMonth(date);
        var isLastDay = cal.GetDayOfMonth(date.AddDays(1)) == 1;

        // 命中范围：除夕 或 正月初一..初三（排除闰月位）
        var inRange = (month == lastMonth && isLastDay && month != leapMonth) ||
                      (month == 1 && day >= 1 && day <= 3 && month != leapMonth);
        if (!inRange) return false;

        // 统一以“除夕”为起始日，跨度 4 天
        DateTime start;
        if (month == lastMonth && isLastDay)
        {
            start = date.Date;
        }
        else
        {
            // 正月初 N -> 起始 = 当天减去 N 天
            start = date.Date.AddDays(-day);
        }

        holiday = new HolidayInfo
        {
            Name = "春节",
            Date = start,
            Days = 4,
            Status = HolidayStatus.On
        };

        return true;
    }

    /// <summary>尝试获取端午节假期</summary>
    /// <param name="date"></param>
    /// <param name="holiday"></param>
    /// <returns></returns>
    public Boolean TryGetDuanwu(DateTime date, out HolidayInfo holiday)
    {
        holiday = null;

        var cal = new ChineseLunisolarCalendar();
        var year = cal.GetYear(date);
        var month = cal.GetMonth(date);
        var day = cal.GetDayOfMonth(date);
        var leapMonth = cal.GetLeapMonth(year);

        // 考虑有闰月
        var targetMonth = leapMonth > 0 && leapMonth < month ? 6 : 5;

        if (month == targetMonth && day == 5 && month != leapMonth)
        {
            holiday = new HolidayInfo
            {
                Name = "端午节",
                Date = date.Date,
                Days = 1,
                Status = HolidayStatus.On
            };

            return true;
        }

        return false;
    }

    /// <summary>尝试获取中秋节假期</summary>
    /// <param name="date"></param>
    /// <param name="holiday"></param>
    /// <returns></returns>
    public Boolean TryGetZhongqiu(DateTime date, out HolidayInfo holiday)
    {
        holiday = null;

        var cal = new ChineseLunisolarCalendar();
        var year = cal.GetYear(date);
        var month = cal.GetMonth(date);
        var day = cal.GetDayOfMonth(date);
        var leapMonth = cal.GetLeapMonth(year);

        // 考虑有闰月
        var targetMonth = leapMonth > 0 && leapMonth < month ? 9 : 8;

        if (month == targetMonth && day == 15 && month != leapMonth)
        {
            holiday = new HolidayInfo
            {
                Name = "中秋节",
                Date = date.Date,
                Days = 1,
                Status = HolidayStatus.On
            };

            return true;
        }

        return false;
    }
    #endregion
}