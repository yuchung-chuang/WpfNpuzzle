using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro;

namespace Wpf15puzzle
{
  /// <summary>
  /// App.xaml 的互動邏輯
  /// </summary>
  public partial class App : Application
  {
    protected override void OnStartup(StartupEventArgs e)
    {
      //複寫MahApps設定
      // add custom accent and theme resource dictionaries to the ThemeManager
      // you should replace MahAppsMetroThemesSample with your application name
      // and correct place where your custom accent lives
      ThemeManager.AddAccent("CustomAccent1", new Uri("pack://application:,,,/Wpf15puzzle;component/MahAppsOverrides.xaml"));

      // get the current app style (theme and accent) from the application
      var theme = ThemeManager.DetectAppStyle(Application.Current);

      // now change app style to the custom accent and current theme
      ThemeManager.ChangeAppStyle(Application.Current,
                                  ThemeManager.GetAccent("CustomAccent1"),
                                  theme.Item1);
      
      base.OnStartup(e);
    }
  }
}
