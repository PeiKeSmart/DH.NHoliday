namespace NewLife.Holiday;

/// <summary>假期接口</summary>
public interface IHoliday
{
    /// <summary>查询指定日期的假期信息</summary>
    /// <param name="date">指定日期</param>
    /// <returns>假期信息</returns>
    IEnumerable<HolidayInfo> Query(DateTime date);
}