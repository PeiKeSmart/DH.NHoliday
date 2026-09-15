using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit;

namespace XUnitTest;

public class LunarTests
{
    [Theory]
    [InlineData("2024-02-10")] // 甲辰年正月初一（春节）
    [InlineData("2023-01-22")] // 癸卯年正月初一
    [InlineData("2020-05-23")] // 农历四月初一（庚子年）
    public void BasicProperties_ShouldMatchChineseLunisolarCalendar(String gregorian)
    {
        var date = DateTime.Parse(gregorian);
        var lunar = NewLife.Holiday.Lunar.FromDateTime(date);

        var cal = new ChineseLunisolarCalendar();
        var year = cal.GetYear(date);
        var monthWithLeap = cal.GetMonth(date);
        var day = cal.GetDayOfMonth(date);
        var leapMonth = cal.GetLeapMonth(year);

        var isLeap = false;
        var normalizedMonth = monthWithLeap;
        if (leapMonth > 0)
        {
            if (monthWithLeap == leapMonth)
            {
                isLeap = true;
                normalizedMonth = monthWithLeap - 1;
            }
            else if (monthWithLeap > leapMonth)
            {
                normalizedMonth = monthWithLeap - 1;
            }
        }

        // 覆盖 Date/Year/Month/Day/IsLeapMonth
        Assert.Equal(date, lunar.Date);
        Assert.Equal(year, lunar.Year);
        Assert.Equal(normalizedMonth, lunar.Month);
        Assert.Equal(day, lunar.Day);
        Assert.Equal(isLeap, lunar.IsLeapMonth);

        // 覆盖 ToString 的基本格式（不含固定前缀“农历”，当前实现不带该前缀）
        var str = lunar.ToString();
        Assert.Contains("年", str);
        Assert.Contains("月", str);
    }

    [Theory]
    [InlineData("2024-02-10", "甲辰", "龙", "正", "初一")]
    [InlineData("2023-01-22", "癸卯", "兔", "正", "初一")]
    [InlineData("2020-01-25", "庚子", "鼠", "正", "初一")]
    public void TextualMembers_ShouldBeCorrect(String gregorian, String ganzhi, String zodiac, String monthText, String dayText)
    {
        var date = DateTime.Parse(gregorian);
        var lunar = NewLife.Holiday.Lunar.FromDateTime(date);

        // YearGanzhi + Zodiac + MonthText + DayText
        Assert.Equal(ganzhi, lunar.YearGanzhi);
        Assert.Equal(zodiac, lunar.Zodiac);
        Assert.Equal(monthText, lunar.MonthText);
        Assert.Equal(dayText, lunar.DayText);

        // YearGanzhi 基本合法性
        Assert.Equal(2, lunar.YearGanzhi.Length);
        Assert.Contains(lunar.YearGanzhi[0].ToString(), "甲乙丙丁戊己庚辛壬癸");
        Assert.Contains(lunar.YearGanzhi[1].ToString(), "子丑寅卯辰巳午未申酉戌亥");
    }

    [Fact]
    public void DayText_BoundarySamples_ShouldBeCorrect()
    {
        // 以 2024-02-10 为农历正月初一为基准
        var d1 = new DateTime(2024, 2, 10);
        var l1 = NewLife.Holiday.Lunar.FromDateTime(d1);
        Assert.Equal(1, l1.Day);
        Assert.Equal("初一", l1.DayText);

        var d10 = d1.AddDays(9); // 初十
        var l10 = NewLife.Holiday.Lunar.FromDateTime(d10);
        Assert.Equal(10, l10.Day);
        Assert.Equal("初十", l10.DayText);

        var d11 = d1.AddDays(10);
        var l11 = NewLife.Holiday.Lunar.FromDateTime(d11);
        Assert.Equal(11, l11.Day);
        Assert.Equal("十一", l11.DayText);

        var d20 = d1.AddDays(19);
        var l20 = NewLife.Holiday.Lunar.FromDateTime(d20);
        Assert.Equal(20, l20.Day);
        Assert.Equal("二十", l20.DayText);

        var d21 = d1.AddDays(20);
        var l21 = NewLife.Holiday.Lunar.FromDateTime(d21);
        Assert.Equal(21, l21.Day);
        Assert.Equal("廿一", l21.DayText);

        // 找到某个 30 的日子
        var probe = d1;
        for (var i = 0; i < 90; i++)
        {
            var l = NewLife.Holiday.Lunar.FromDateTime(probe);
            if (l.Day == 30)
            {
                Assert.Equal("三十", l.DayText);
                break;
            }
            probe = probe.AddDays(1);
        }
    }

    [Fact]
    public void LeapMonthHandling_ShouldIndicateLeapAndNormalizeMonth()
    {
        // 选择一个已知包含闰月的农历年，例如 2012(壬辰年) 闰四月
        var date = new DateTime(2012, 6, 20); // 大致为闰四月期间的一天
        var cal = new ChineseLunisolarCalendar();
        var year = cal.GetYear(date);
        var m = cal.GetMonth(date);
        var leapMonth = cal.GetLeapMonth(year);

        if (leapMonth == 0)
        {
            // 如果运行平台/地区数据不同导致该年无闰月，则跳过
            return;
        }

        var lunar = NewLife.Holiday.Lunar.FromDateTime(date);

        if (m == leapMonth)
        {
            Assert.True(lunar.IsLeapMonth);
            Assert.True(lunar.Month >= 1 && lunar.Month <= 12);
            Assert.Contains("闰", lunar.ToString()); // 覆盖闰月 ToString
        }
        else if (m > leapMonth)
        {
            Assert.False(lunar.IsLeapMonth);
            Assert.Equal(m - 1, lunar.Month);
        }
    }

    [Fact]
    public void MonthText_ShouldMatchMapping_ForManyDates()
    {
        // 扫描一段日期，验证 MonthText 与 Month 的映射关系
        var start = new DateTime(2023, 1, 1);
        var end = new DateTime(2025, 12, 31);

        var map = new Dictionary<int, string>
        {
            [1] = "正",
            [2] = "二",
            [3] = "三",
            [4] = "四",
            [5] = "五",
            [6] = "六",
            [7] = "七",
            [8] = "八",
            [9] = "九",
            [10] = "十",
            [11] = "冬",
            [12] = "腊",
        };

        for (var dt = start; dt <= end; dt = dt.AddDays(1))
        {
            var lunar = NewLife.Holiday.Lunar.FromDateTime(dt);
            Assert.Equal(map[lunar.Month], lunar.MonthText);
        }
    }
}
