namespace NewLife.Holiday;

/// <summary>
/// 24节气枚举（按公历月份顺序：1月小寒起，至12月冬至止）
/// </summary>
public enum SolarTerm
{
    /// <summary>小寒（1月）</summary>
    小寒 = 0,
    /// <summary>大寒（1月）</summary>
    大寒 = 1,
    /// <summary>立春（2月）</summary>
    立春 = 2,
    /// <summary>雨水（2月）</summary>
    雨水 = 3,
    /// <summary>惊蛰（3月）</summary>
    惊蛰 = 4,
    /// <summary>春分（3月）</summary>
    春分 = 5,
    /// <summary>清明（4月）</summary>
    清明 = 6,
    /// <summary>谷雨（4月）</summary>
    谷雨 = 7,
    /// <summary>立夏（5月）</summary>
    立夏 = 8,
    /// <summary>小满（5月）</summary>
    小满 = 9,
    /// <summary>芒种（6月）</summary>
    芒种 = 10,
    /// <summary>夏至（6月）</summary>
    夏至 = 11,
    /// <summary>小暑（7月）</summary>
    小暑 = 12,
    /// <summary>大暑（7月）</summary>
    大暑 = 13,
    /// <summary>立秋（8月）</summary>
    立秋 = 14,
    /// <summary>处暑（8月）</summary>
    处暑 = 15,
    /// <summary>白露（9月）</summary>
    白露 = 16,
    /// <summary>秋分（9月）</summary>
    秋分 = 17,
    /// <summary>寒露（10月）</summary>
    寒露 = 18,
    /// <summary>霜降（10月）</summary>
    霜降 = 19,
    /// <summary>立冬（11月）</summary>
    立冬 = 20,
    /// <summary>小雪（11月）</summary>
    小雪 = 21,
    /// <summary>大雪（12月）</summary>
    大雪 = 22,
    /// <summary>冬至（12月）</summary>
    冬至 = 23,
}

/// <summary>
/// 指定时间点相对最近节气的结果。
/// </summary>
/// <remarks>
/// 使用指定节气日期与参考时间构造结果。
/// </remarks>
/// <param name="term">节气。</param>
/// <param name="termTime">节气的公历日期（按本地时区）。</param>
/// <param name="from">参考时间。</param>
public readonly struct SolarTermResult(SolarTerm term, DateTime termTime, DateTime from)
{
    /// <summary>最近的节气。</summary>
    public SolarTerm Term { get; } = term;

    /// <summary>节气对应的公历日期（本地时区）。</summary>
    public DateTime TermTime { get; } = termTime;

    /// <summary>
    /// 与节气日零点的带符号天数差。
    /// 指定时间在节气之前为正数，在节气之后为负数，可为小数。
    /// </summary>
    public Double DaysTo { get; } = (termTime - from).TotalDays;

    /// <summary>指定时间当天是否就是该节气日。</summary>
    public Boolean IsTermDay { get; } = from.Date == termTime.Date;

    /// <summary>指定日期是否在节气日前后一天（含节气日）。</summary>
    public Boolean IsWithinOneDay { get; } = Math.Abs((termTime - from).TotalDays) <= 1;

    /// <summary>格式化为“节气 yyyy-MM-dd ±X天”。</summary>
    public override String ToString() => $"{Term} {TermTime:yyyy-MM-dd HH:mm:ss} {(DaysTo >= 0 ? "+" : "")}{DaysTo:0.#}天";
}

/// <summary>
/// 24节气计算（1901-2100 近似算法，按日精度）。
/// </summary>
/// <remarks>
/// 使用常见近似算法 day = floor(Y * D + C) - floor((Y - 1)/4)。
/// 其中 Y 为公历年后两位；C 为20/21世纪常数；D = 0.2422。
/// 范围 1901-2100，按日精度，个别年份存在 ±1 天误差，代码中包含少量修正项。
/// 结果为本地时区的公历日期（时间部分为 00:00:00）。
/// </remarks>
public static class SolarTerms
{
    // 近似算法：day = floor(Y * D + C) - floor((Y - 1)/4)
    // 其中 Y 为公历年后两位；C 为各节气常数（20世纪 / 21世纪），D = 0.2422。
    // 该算法能在 1901-2100 年范围内给出节气的公历日期（少数年份可能会有 ±1 天误差）。

    private const Double D = 0.2422;

    // 与枚举顺序完全一致（小寒..冬至）
    private static readonly Double[] C20 = // 1901-2000
    {
        6.11, 20.84, 4.6295, 19.4599, 6.3826, 21.4155, 5.59, 20.888, 6.318, 21.86, 6.5, 22.20,
        7.928, 23.65, 8.35, 23.95, 8.44, 23.822, 9.098, 24.218, 8.218, 23.08, 7.9, 22.60
    };

    private static readonly Double[] C21 = // 2001-2100
    {
        5.4055, 20.12, 3.87, 18.73, 5.63, 20.646, 4.81, 20.1, 5.52, 21.04, 5.678, 21.37,
        7.108, 22.83, 7.5, 23.13, 7.646, 23.042, 8.318, 23.438, 7.438, 22.36, 7.18, 21.94
    };

    // 每个节气对应的公历月份（与枚举顺序一一对应）
    private static readonly Int32[] TermMonths =
    {
        1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6,
        7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12
    };

    // 目标太阳黄经（度），与枚举顺序一一对应。
    private static readonly Double[] TargetLongitudes =
    {
        285, 300, 315, 330, 345, 0, 15, 30, 45, 60, 75, 90,
        105, 120, 135, 150, 165, 180, 195, 210, 225, 240, 255, 270
    };

    /// <summary>
    /// 计算指定年份的某个节气在公历中的日期（零点）。
    /// </summary>
    /// <param name="year">公历年。</param>
    /// <param name="term">节气。</param>
    /// <returns>节气的公历日期（本地时区零点）。</returns>
    public static DateTime GetTermDate(Int32 year, SolarTerm term)
    {
        // 兼容旧 API：基于精确时间取日期部分。
        return GetTermTime(year, term).Date;
    }

    /// <summary>
    /// 计算指定年份某个节气的本地时间（精确到分钟）。
    /// </summary>
    /// <param name="year">公历年。</param>
    /// <param name="term">节气。</param>
    /// <returns>节气发生的本地时间。</returns>
    public static DateTime GetTermTime(Int32 year, SolarTerm term)
    {
        // 先用日级近似算法得到一个接近的日期，作为搜索起点，避免跨年索引不稳定。
        var idx = (Int32)term;
        var month = TermMonths[idx];
        var y = year % 100;
        var c = year >= 2001 ? C21[idx] : C20[idx];
        var l = (Int32)Math.Floor((y - 1) / 4.0);
        var day = (Int32)Math.Floor(y * D + c) - l;
        day += SpecialOffset(year, term);

        // 本地日期的中午作为搜索中心更稳（避免夏令时边界影响）。
        var localApprox = new DateTime(year, month, day, 12, 0, 0, DateTimeKind.Local);
        // 以该时间前后各两天构造搜索区间，在 UTC 下做天文计算。
        var utcStart = localApprox.AddDays(-2).ToUniversalTime();
        var utcEnd = localApprox.AddDays(2).ToUniversalTime();

        var target = TargetLongitudes[idx];

        // 先粗扫找符号变化区间
        var bracketFound = TryFindBracket(utcStart, utcEnd, target, out var a, out var b);
        if (!bracketFound)
        {
            // 若未找到，放宽到前后 7 天且步长加粗，防止个别年份近似日与真实日相差较大
            var wideStart = localApprox.AddDays(-7).ToUniversalTime();
            var wideEnd = localApprox.AddDays(7).ToUniversalTime();
            if (!TryFindBracket(wideStart, wideEnd, target, out a, out b))
            {
                // 极端情况：回退到近似日期的当天做二分近似（此时精度可能较低）
                a = localApprox.ToUniversalTime().AddHours(-12);
                b = localApprox.ToUniversalTime().AddHours(12);
            }
        }

        // 在括区内进行二分逼近，直到达到分钟级精度
        var resultUtc = BinarySearchLongitude(a, b, target, TimeSpan.FromMinutes(0.5));

        // 转回本地时间
        var local = resultUtc.ToLocalTime();
        // 舍入到分钟
        return new DateTime(local.Year, local.Month, local.Day, local.Hour, local.Minute, 0, DateTimeKind.Local);
    }

    /// <summary>
    /// 返回某年的全部24节气日期。
    /// </summary>
    /// <param name="year">公历年。</param>
    /// <returns>按节气顺序（小寒..冬至）的列表。</returns>
    public static IReadOnlyList<(SolarTerm Term, DateTime Date)> GetAll(Int32 year)
    {
        var list = new List<(SolarTerm, DateTime)>(24);
        for (var i = 0; i < 24; i++)
        {
            var t = (SolarTerm)i;
            list.Add((t, GetTermDate(year, t)));
        }
        return list;
    }

    /// <summary>
    /// 返回某年的全部24节气本地时间（精确到分钟）。
    /// </summary>
    public static IReadOnlyList<(SolarTerm Term, DateTime Time)> GetAllTimes(Int32 year)
    {
        var list = new List<(SolarTerm, DateTime)>(24);
        for (var i = 0; i < 24; i++)
        {
            var t = (SolarTerm)i;
            list.Add((t, GetTermTime(year, t)));
        }
        return list;
    }

    /// <summary>
    /// 已知特殊年份的日数微调（覆盖常见误差）。未列出的返回 0。
    /// </summary>
    private static Int32 SpecialOffset(Int32 year, SolarTerm term)
    {
        // 仅列举较为常见的偏差修正，保持轻量。
        // 资料来源于公共领域常用近似法总结，1901-2100 少量年份会发生 ±1 天误差。
        switch (term)
        {
            case SolarTerm.春分:
                if (year == 2084) return -1;
                break;
            case SolarTerm.夏至:
                if (year == 2002) return 1;
                break;
            case SolarTerm.小暑:
                if (year == 2016) return 1;
                break;
            case SolarTerm.白露:
                if (year == 2002) return 1;
                break;
            case SolarTerm.冬至:
                if (year == 1918) return 1;
                if (year == 2021) return -1;
                break;
        }
        return 0;
    }

    #region 天文计算（简化太阳黄经）
    private static Boolean TryFindBracket(DateTime utcStart, DateTime utcEnd, Double targetLongitude, out DateTime a, out DateTime b)
    {
        // 按6 小时步长扫描，找到符号变化的相邻两个采样点
        var step = TimeSpan.FromHours(6);
        var t = utcStart;
        var prevT = t;
        var prevVal = SignedAngleDiff(SunApparentLongitude(t), targetLongitude);
        t = t + step;
        while (t <= utcEnd)
        {
            var val = SignedAngleDiff(SunApparentLongitude(t), targetLongitude);
            if (prevVal <= 0 && val >= 0 || prevVal >= 0 && val <= 0)
            {
                a = prevT;
                b = t;
                return true;
            }
            prevT = t;
            prevVal = val;
            t = t + step;
        }
        a = default;
        b = default;
        return false;
    }

    private static DateTime BinarySearchLongitude(DateTime utcA, DateTime utcB, Double targetLongitude, TimeSpan tolerance)
    {
        var a = utcA;
        var b = utcB;
        while ((b - a) > tolerance)
        {
            var m = a + TimeSpan.FromTicks((b.Ticks - a.Ticks) / 2);
            var val = SignedAngleDiff(SunApparentLongitude(m), targetLongitude);
            var vala = SignedAngleDiff(SunApparentLongitude(a), targetLongitude);
            if (vala <= 0 && val >= 0 || vala >= 0 && val <= 0) b = m; else a = m;
        }
        return a + TimeSpan.FromTicks((b.Ticks - a.Ticks) / 2);
    }

    //计算指定 UTC 时间的太阳视黄经（度，0..360）
    private static Double SunApparentLongitude(DateTime utc)
    {
        // 转为儒略日（UTC）
        var jd = ToJulianDay(utc);
        var T = (jd - 2451545.0) / 36525.0; // 儒略世纪数（J2000.0 起）

        // 平黄经 L0（度）
        var L0 = 280.46646 + 36000.76983 * T + 0.0003032 * T * T;
        L0 = Normalize360(L0);

        // 地球轨道偏心率 e
        var e = 0.016708634 - 0.000042037 * T - 0.0000001267 * T * T;

        // 平近点角 M（度）
        var M = 357.52911 + 35999.05029 * T - 0.0001537 * T * T;
        M = Normalize360(M);

        // 日心几何黄经的中心差 C（度）
        var Mrad = Deg2Rad(M);
        var C = (1.914602 - 0.004817 * T - 0.000014 * T * T) * Math.Sin(Mrad)
               + (0.019993 - 0.000101 * T) * Math.Sin(2 * Mrad)
               + 0.000289 * Math.Sin(3 * Mrad);

        var trueLong = L0 + C; // 真黄经（度）

        //视黄经修正（岁差）
        var Omega = 125.04 - 1934.136 * T;
        var lambda = trueLong - 0.00569 - 0.00478 * Math.Sin(Deg2Rad(Omega));

        return Normalize360(lambda);
    }

    private static Double ToJulianDay(DateTime utc)
    {
        if (utc.Kind != DateTimeKind.Utc) utc = utc.ToUniversalTime();
        // 算法：Meeus, Astronomical Algorithms（简化版）
        var y = utc.Year;
        var m = utc.Month;
        var d = utc.Day + (utc.Hour + (utc.Minute + utc.Second / 60.0) / 60.0) / 24.0;

        if (m <= 2)
        {
            y -= 1;
            m += 12;
        }

        var A = Math.Floor(y / 100.0);
        var B = 2 - A + Math.Floor(A / 4.0);

        var jd = Math.Floor(365.25 * (y + 4716))
                + Math.Floor(30.6001 * (m + 1))
                + d + B - 1524.5;

        return jd;
    }

    private static Double Deg2Rad(Double deg) => deg * (Math.PI / 180.0);

    private static Double Normalize360(Double x)
    {
        var r = x % 360.0;
        if (r < 0) r += 360.0;
        return r;
    }

    private static Double Normalize180(Double x)
    {
        var r = Normalize360(x);
        return r > 180.0 ? r - 360.0 : r;
    }

    private static Double SignedAngleDiff(Double lon, Double target)
    {
        //规范到 -180..180 的带符号差值
        return Normalize180(lon - target);
    }
    #endregion
}
