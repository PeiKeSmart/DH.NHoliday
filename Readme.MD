# DH.NHoliday - 中国假期识别库

中国假期识别库，内置假期调休数据库，每年维护更新，支持自动识别农历节假日。  
包名 `DH.NHoliday`，命名空间保持 `NewLife.Holiday`，同步自 [NewLife.Holiday](https://github.com/NewLifeX/NewLife.Holiday)。支持 net45 / net461 / netstandard2.0 / netstandard2.1 / net11.0。

## 使用方法
从 NuGet 引用 `DH.NHoliday`，代码中引入命名空间 `NewLife.Holiday` 即可使用 DateTime 扩展方法 IsChinaHoliday。  
``` csharp
using NewLife.Holiday;

// var dt = DateTime.Now;
var dt = "2022-09-10".ToDateTime();
var rs = dt.IsChinaHoliday();
```

## 广西三月三
从 NuGet 引用 `DH.NHoliday`，代码中引入命名空间 `NewLife.Holiday` 即可使用 DateTime 扩展方法 IsGuangxiHoliday。  
``` csharp
using NewLife.Holiday;

// var dt = DateTime.Now;
var dt = "2022-04-02".ToDateTime();
var rs = dt.IsGuangxiHoliday();
```

## 农历与节气
- `Lunar.FromDateTime(dt)` 公历转农历，提供生肖 `Zodiac`、`YearGanzhi` 干支纪年、`ToString()` 中文农历字符串  
- `lunar.GetNearestSolarTerm()` 查询最近 24 节气，节气时间精确到时分秒，`DaysTo` 天数差可含小数，需要精确判断日期时使用 `TermTime.Date`  

``` csharp
using NewLife.Holiday;

var lunar = Lunar.FromDateTime(DateTime.Now);
var term = lunar.GetNearestSolarTerm();
```

## 数据维护
- 全国假期调休数据：`DH.NHoliday/China/*.csv`  
- 广西假期数据：`DH.NHoliday/Guangxi/*.csv`  
- 数据以嵌入式资源打包进程序集，每年根据国务院办公厅节假日安排更新  
