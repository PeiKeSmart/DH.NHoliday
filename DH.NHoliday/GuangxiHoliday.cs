using System.Globalization;

namespace NewLife.Holiday;

/// <summary>广西假期。特有三月三</summary>
public class GuangxiHoliday : ChinaHoliday
{
    #region 构造
    /// <summary>实例化</summary>
    public GuangxiHoliday()
    {
        // 加载所有嵌入式资源
        //Load("China");
        Load("Guangxi");

        // 排序，先按照日期排，再按照状态排，便于放假优先覆盖调休
        if (Infos is List<HolidayInfo> list) Infos = list.OrderBy(x => x.Date).ThenBy(e => e.Status).ToList();
    }
    #endregion

    #region 方法
    /// <summary>查询指定日期的假期信息</summary>
    /// <param name="date">指定日期</param>
    /// <returns>假期信息</returns>
    public override IEnumerable<HolidayInfo> Query(DateTime date)
    {
        var count = 0;
        foreach (var inf in base.Query(date))
        {
            count++;
            yield return inf;
        }

        if (count == 0)
        {
            if (TryGetSanyuesan(date, out var holiday)) yield return holiday;
        }
    }

    /// <summary>尝试获取三月三假期</summary>
    /// <param name="date"></param>
    /// <param name="holiday"></param>
    /// <returns></returns>
    public Boolean TryGetSanyuesan(DateTime date, out HolidayInfo holiday)
    {
        holiday = null;

        var cal = new ChineseLunisolarCalendar();
        var year = cal.GetYear(date);
        var month = cal.GetMonth(date);
        var day = cal.GetDayOfMonth(date);
        var leapMonth = cal.GetLeapMonth(year);
        if (month == 3 && day == 3 && month != leapMonth)
        {
            holiday = new HolidayInfo
            {
                Category = "Guangxi",
                Name = "三月三",
                Date = date.Date,
                Days = 2,
                Status = HolidayStatus.On
            };

            return true;
        }

        return false;
    }
    #endregion
}