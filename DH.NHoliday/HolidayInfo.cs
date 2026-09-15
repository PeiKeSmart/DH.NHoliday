namespace NewLife.Holiday;

/// <summary>
/// 假期信息
/// </summary>
public class HolidayInfo
{
    #region 属性
    /// <summary>名称</summary>
    public String Name { get; set; }

    /// <summary>分类</summary>
    public String Category { get;set; }

    /// <summary>日期</summary>
    public DateTime Date { get; set; }

    /// <summary>天数。从Date日期算起的天数，默认1</summary>
    public Int32 Days { get; set; }

    /// <summary>假期状态。常规、放假、调休</summary>
    public HolidayStatus Status { get; set; }
    #endregion
}