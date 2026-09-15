using NewLife;
using NewLife.Holiday;
using NewLife.Log;

namespace Test;

internal class Program
{
    private static void Main(String[] args)
    {
        //MachineInfo.RegisterAsync();
        XTrace.UseConsole();

        XTrace.Log.Level = LogLevel.Debug;

        //Console.Write("输出要执行的测试方法序号：");
        //var idx = Console.ReadLine().ToInt();

        try
        {
            Test2();
            //var mi = typeof(Program).GetMethod("Test" + idx, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            //if (mi != null) mi.Invoke(null, null);
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
        }

        Console.WriteLine("OK");
        Console.Read();
    }

    private static void Test1()
    {
        var ch = new ChinaHoliday();
        //var ch = new GuangxiHoliday();
        foreach (var item in ch.Infos)
        {
            XTrace.WriteLine("{0} {1} {2} {3}", item.Name, item.Date, item.Days, item.Status.GetDescription());
        }
    }

    private static void Test2()
    {
        //var dt = new DateTime(1984, 1, 1);
        var dt = DateTime.Now.AddDays(-100);
        XTrace.WriteLine("公历 农历 生肖");

        for (var i = 0; i < 365; i++)
        {
            var lunar = Lunar.FromDateTime(dt);
            var term = lunar.GetNearestSolarTerm();

            XTrace.WriteLine("{0} {1}年 {2} {3}", dt, lunar.Zodiac, lunar.ToString(), term);

            dt = dt.AddDays(1);
            Thread.Sleep(10);
        }
    }
}
