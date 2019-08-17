using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfNpuzzle
{
  /// <summary>
  /// SplashPage.xaml 的互動邏輯
  /// </summary>
  public partial class SplashPage : Page
  {
    public SplashPage()
    {
      InitializeComponent();
    }

    public Frame Container { get; set; }

    private void Storyboard_Completed(object sender, EventArgs e)
    {

    }

    private void TextWave_Completed(object sender, EventArgs e)
    {
#if DEBUG
      Console.WriteLine("text");
#endif

    }

    private void RobotVoice_Completed(object sender, EventArgs e)
    {
#if DEBUG
      Console.WriteLine("robot");
#endif      

    }

    private void IconFade_Completed(object sender, EventArgs e)
    {
#if DEBUG
      Console.WriteLine("icon");
#endif
      SpinWait.SpinUntil(() => false, 1000);
      Container.Content = new MainPage();

    }
  }
}
