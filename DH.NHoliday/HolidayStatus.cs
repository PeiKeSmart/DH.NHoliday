using System.ComponentModel;

namespace NewLife.Holiday;

/// <summary>
/// 假期状态。常规、放假、调休
/// </summary>
public enum HolidayStatus
{
    /// <summary>常规。工作日正常上班，周末休息</summary>
    [Description("常规")]
    Normal = 0,

    /// <summary>放假。无需上班</summary>
    [Description("放假")]
    On = 1,

    /// <summary>调休。周末也要上班</summary>
    [Description("调休")]
    Off = 2,
}