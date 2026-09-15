using System.Globalization;

namespace NewLife.Holiday;

/// <summary>
/// 农历（中国农历）
/// </summary>
/// <remarks>
/// 使用 <see cref="ChineseLunisolarCalendar"/> 将公历 <see cref="DateTime"/> 映射为农历日期。
/// - <see cref="Year"/>：农历年（数字），范围约为 1901-2100。
/// - <see cref="Month"/>：农历月（1-12 的数字，不含闰月位移）。
/// - <see cref="Day"/>：农历日（1-30）。
/// - <see cref="IsLeapMonth"/>：是否闰月。
/// 
/// 说明：<see cref="ChineseLunisolarCalendar.GetMonth(DateTime)"/> 返回的月份在遇到闰月的年份会包含“闰月插位”，
/// 例如闰四月时，四月=4，闰四月=5，五月=6 …… 因此本类型内部会根据 <see cref="ChineseLunisolarCalendar.GetLeapMonth(Int32)"/>
/// 将月份规格化为 1-12，并单独提供 <see cref="IsLeapMonth"/> 标识。
/// </remarks>
public readonly struct Lunar
{
    private static readonly ChineseLunisolarCalendar Cal = new();

    /// <summary>对应的公历日期（本地时间）</summary>
    public DateTime Date { get; }

    /// <summary>农历年（数字）</summary>
    public Int32 Year { get; }

    /// <summary>农历月（1-12，若为闰月请结合 <see cref="IsLeapMonth"/>）</summary>
    public Int32 Month { get; }

    /// <summary>农历日（1-30）</summary>
    public Int32 Day { get; }

    /// <summary>是否闰月</summary>
    public Boolean IsLeapMonth { get; }

    /// <summary>天干地支年，例如“甲子”、“乙丑”</summary>
    public String YearGanzhi
    {
        get
        {
            var tg = "甲乙丙丁戊己庚辛壬癸"[(Year - 4) % 10];
            var dz = "子丑寅卯辰巳午未申酉戌亥"[(Year - 4) % 12];
            return new String([tg, dz]);
        }
    }

    /// <summary>生肖，例如“鼠”、“牛”</summary>
    public String Zodiac => ("鼠牛虎兔龙蛇马羊猴鸡狗猪")[(Year - 4) % 12].ToString();

    /// <summary>中文月份文本（不包含“闰”字样），例如“正”、“二”、“十”、“冬”、“腊”。</summary>
    public String MonthText => MonthToText(Month);

    /// <summary>中文日文本，例如“初一”、“廿九”、“三十”。</summary>
    public String DayText => DayToText(Day);

    private Lunar(DateTime date, Int32 year, Int32 month, Int32 day, Boolean isLeap)
    {
        Date = date;
        Year = year;
        Month = month;
        Day = day;
        IsLeapMonth = isLeap;
    }

    #region 工厂方法
    /// <summary>
    /// 根据公历 <paramref name="dateTime"/> 生成对应的 <see cref="Lunar"/>。
    /// </summary>
    /// <remarks>
    /// 依赖 <see cref="ChineseLunisolarCalendar"/>，有效范围大致为 1901-02-19 至 2101-01-28。
    /// 超出范围会抛出 <see cref="ArgumentOutOfRangeException"/>。
    /// </remarks>
    public static Lunar FromDateTime(DateTime dateTime)
    {
        // 取农历年、月（包含闰月插位）、日
        var year = Cal.GetYear(dateTime);
        var monthWithLeap = Cal.GetMonth(dateTime);
        var day = Cal.GetDayOfMonth(dateTime);

        // 获取该农历年的闰月插位（0 表示无闰月，非 0 表示插在该序号处）
        var leapMonth = Cal.GetLeapMonth(year);

        // 规格化月份为 1-12，并计算是否闰月
        var isLeap = false;
        var normalizedMonth = monthWithLeap;
        if (leapMonth > 0)
        {
            if (monthWithLeap == leapMonth)
            {
                isLeap = true;
                normalizedMonth = monthWithLeap - 1; // 闰四月(5) -> 4
            }
            else if (monthWithLeap > leapMonth)
            {
                normalizedMonth = monthWithLeap - 1; // 闰月之后的月份需要回退 1
            }
        }

        return new Lunar(dateTime, year, normalizedMonth, day, isLeap);
    }
    #endregion

    #region 节气
    /// <summary>
    /// 计算并返回距离该实例时间最近的24节气，以及与节气之间的带符号天数差。
    /// 指定时间在目标之前用正数，在目标之后用负数。
    /// </summary>
    public SolarTermResult GetNearestSolarTerm()
    {
        var dt = Date;
        // 取当前年与相邻年（防止最近节气跨年）
        var candidates = new List<(SolarTerm Term, DateTime Time)>(72);
        candidates.AddRange(SolarTerms.GetAllTimes(dt.Year));
        candidates.AddRange(SolarTerms.GetAllTimes(dt.Year - 1));
        candidates.AddRange(SolarTerms.GetAllTimes(dt.Year + 1));

        (SolarTerm Term, DateTime Time) best = default;
        var bestAbs = Double.MaxValue;

        foreach (var (term, time) in candidates)
        {
            var diff = (time - dt).TotalDays; // 正：节气在未来；负：节气已过
            var ad = Math.Abs(diff);
            if (ad < bestAbs)
            {
                bestAbs = ad;
                best = (term, time);
            }
        }

        return new SolarTermResult(best.Term, best.Time, dt);
    }
    #endregion

    #region 格式化
    /// <summary>
    /// 以“农历某某年某某月初几”的格式返回字符串。
    /// 例如："农历癸卯年正月初一"，闰月会显示为"闰正/闰二"等。
    /// </summary>
    public override String ToString()
    {
        var leap = IsLeapMonth ? "闰" : String.Empty;
        return $"{YearGanzhi}年{leap}{MonthText}月{DayText}";
    }

    private static String MonthToText(Int32 month)
    {
        // 基于字符串常量按索引定位
        return (month >= 1 && month <= 12) ? " 正二三四五六七八九十冬腊"[month].ToString() : month.ToString();
    }

    private static String DayToText(Int32 day)
    {
        if (day <= 0 || day > 30) return day.ToString();

        // 标准表达：
        // 1-9: 初一..初九
        // 10: 初十
        // 11-19: 十一..十九
        // 20: 二十
        // 21-29: 廿一..廿九
        // 30: 三十

        // 月份与日数字文本常量，索引从1 开始，0 位占位（空格）
        const String ones = " 一二三四五六七八九";

        if (day == 10) return "初十";
        if (day < 10) return "初" + ones[day];
        if (day < 20) return "十" + ones[day - 10];
        if (day == 20) return "二十";
        if (day == 30) return "三十";
        return "廿" + ones[day - 20]; // 21..29
    }
    #endregion
}
