using System.Linq;
using System.Threading;
using System.Windows;
using MahApps.Metro.Controls;

namespace WpfNpuzzle
{
  //public class XmlRecord
  //{
  //  /// <summary>
  //  /// 儲存完成時間的陣列，須確保按照時間由短到長排列
  //  /// </summary>
  //  public int?[] Records { get; set; }

  //  private int n = 10;
  //  public XmlRecord()
  //  {
  //    Records = Enumerable.Repeat<int?>(null, n).ToArray();
  //  }

  //  public bool AddRecord(int record)
  //  {
  //    for (int i = 0; i < n; i++)
  //    {
  //      if (Records[i] == null || Records[i] > record)
  //      {
  //        for (int j = n - 1; j > i; j--)
  //        {
  //          Records[j] = Records[j - 1];
  //        }
  //        Records[i] = record;
  //        return true;
  //      }
  //    }
  //    return false;
  //  }
  //}

  /// <summary>
  /// MainWindow.xaml 的互動邏輯
  /// </summary>
  public partial class MainWindow : MetroWindow
  {
    public MainWindow()
    {
      InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
    }

    private void frameMain_Loaded(object sender, RoutedEventArgs e)
    {
#if DEBUG
      frameMain.Content = new MainPage();
#else
      SpinWait.SpinUntil(() => false, 600);
      frameMain.Content = new SplashPage { Container = frameMain };
#endif
    }


    //XmlRecord xmlRecord;
    //string recordPath = "record.xml";
    //private void InitializeXmlRecord()
    //{
    //  ReadXml();
    //  var time = xmlRecord.Records[0];
    //  var (min, sec, ms, tic) = TimeConverter.ToString(time);
    //  tbBest.Text = $"{min}:{sec}:{tic}";
    //}
    //private void InitializeXml()
    //{
    //  xmlRecord = new XmlRecord();
    //  SaveXml();
    //}
    //private void CheckXml()
    //{
    //  if (!File.Exists(recordPath))
    //  {
    //    InitializeXml();
    //  }
    //}
    //private void SaveXml()
    //{
    //  var xmlString = XmlGenericSerializer.Serialize(xmlRecord);
    //  File.WriteAllText(recordPath, xmlString);
    //}
    //private void ReadXml()
    //{
    //  CheckXml();
    //  var xmlString = File.ReadAllText(recordPath);
    //  xmlRecord = XmlGenericSerializer.Deserialize<XmlRecord>(xmlString);

    //  if (xmlRecord == null)
    //  {
    //    InitializeXml();
    //  }
    //}
  }
}
