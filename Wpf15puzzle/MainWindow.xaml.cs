using System;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MyLibrary.Extensions;
using static MyLibrary.Methods.Math;
using MyLibrary.Methods;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Diagnostics;
using MahApps.Metro.Controls;
using MaterialDesignThemes.Wpf;

namespace Wpf15puzzle
{
  public class XmlRecord
  {
    /// <summary>
    /// 儲存完成時間的陣列，須確保按照時間由短到長排列
    /// </summary>
    public int?[] Records { get; set; }

    private int n = 10;
    public XmlRecord()
    {
      Records = Enumerable.Repeat<int?>(null, n).ToArray();
    }

    public bool AddRecord(int record)
    {
      for (int i = 0; i < n; i++)
      {
        if (Records[i] == null || Records[i] > record)
        {
          for (int j = n - 1; j > i; j--)
          {
            Records[j] = Records[j - 1];
          }
          Records[i] = record;
          return true;
        }
      }
      return false;
    }
  }

  /// <summary>
  /// MainWindow.xaml 的互動邏輯
  /// </summary>
  public partial class MainWindow : MetroWindow
  {
    int n = 4;
    public MainWindow()
    {
      InitializeComponent();
    }
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      Initialize();

    }
    private void btnNewGame_Click(object sender, RoutedEventArgs e)
    {
      if (timer.Enabled)
      {
        timer.Stop();
      }
      Initialize();
    }
    private void Initialize()
    {
      InitializeCanvas();
      InitializeBoard();
      InitializePuzzles();
      InitializeTimer();
      //InitializeXmlRecord();
      InitializeSettingRecord();
      StartUpAnimation();
    }

    int? _record;
    int? record
    {
      get => _record;
      set
      {
        if (value != _record)
        {
          if (value == 0)
          {
            _record = null;
          }
          else
          {
            _record = value;
          }
        }
      }
    }
    private void InitializeSettingRecord()
    {
      record = Properties.Settings.Default.record;
      var (min, sec, ms, tic) = TimeConverter.ToString(record);
      tbBest.Text = $"{min}:{sec}:{tic}";
    }

    double lineWidth;
    double puzzleWidth;
    private void InitializeCanvas()
    {
      var p = n+1; // p > n
      var l = ((n+1) * p / (p - n));
      lineWidth = canvasBoard.ActualWidth / l;
      puzzleWidth = canvasBoard.ActualWidth / p;
    }

    Random random = new Random();
    int[,] board;
    private void InitializeBoard()
    {
      board = new int[n, n];
      var L = n * n - 1;
      var numbers = Enumerable.Range(1, L).ToArray();
      do
      {
#if DEBUG
        break;
#endif
        numbers = numbers.OrderBy(x => random.Next()).ToArray();
      } while (!IsValid(numbers));
      for (int i = 0, k = 0; i < n && k < L; i++)
      {
        for (int j = 0; j < n && k < L; j++, k++)
        {
          board[i, j] = numbers[k];
        }
      }
    }
    private bool IsValid(int[] numbers)
    {
      int invariant = 0;
      for (int i = 0; i < numbers.Length; i++)
      {
        for (int j = i + 1; j < numbers.Length; j++)
        {
          if (numbers[i] > numbers[j])
          {
            invariant++;
          }
        }
      }
      return invariant % 2 == 0 ? true : false;
    }

    List<Button> puzzles = new List<Button>();
    private void InitializePuzzles()
    {
      canvasBoard.Children.Clear();
      puzzles.Clear();
      for (int row = 0; row < n; row++)
      {
        for (int col = 0; col < n; col++)
        {
          if (board[row, col] == 0)
          {
            continue;
          }
          var puzzle = new Button
          {
            Content = board[row, col].ToString(),
            Width = puzzleWidth,
            Height = puzzleWidth,
          };
          canvasBoard.Children.Add(puzzle);
          puzzles.Add(puzzle);
          Canvas.SetTop(puzzle, BoardRow2CanvasTop(row));
          Canvas.SetLeft(puzzle, BoardCol2CanvasLeft(col));
        }
      }
    }
    private void StartUpAnimation()
    {
      var animation = new DoubleAnimation
      {
        From = 0,
        To = 1,
        Duration = TimeSpan.FromSeconds(0.4),
        BeginTime = TimeSpan.FromSeconds(0.2),
      };
      foreach (var puzzle in puzzles)
      {
        puzzle.RenderTransform = new ScaleTransform(0, 0);
        puzzle.RenderTransformOrigin = new Point(0.5, 0.5);
        var storyboard = new Storyboard();

        var animationX = animation.Clone();
        var animationY = animation.Clone();
        Storyboard.SetTarget(animationX, puzzle);
        Storyboard.SetTarget(animationY, puzzle);
        Storyboard.SetTargetProperty(animationX, new PropertyPath("RenderTransform.ScaleX"));
        Storyboard.SetTargetProperty(animationY, new PropertyPath("RenderTransform.ScaleY"));
        storyboard.Children.Add(animationX);
        storyboard.Children.Add(animationY);
        storyboard.Begin(this);
      }
    }

    XmlRecord xmlRecord;
    string recordPath = "record.xml";
    private void InitializeXmlRecord()
    {
      ReadXml();
      var time = xmlRecord.Records[0];
      var (min, sec, ms, tic) = TimeConverter.ToString(time);
      tbBest.Text = $"{min}:{sec}:{tic}";
    }
    private void InitializeXml()
    {
      xmlRecord = new XmlRecord();
      SaveXml();
    }
    private void CheckXml()
    {
      if (!File.Exists(recordPath))
      {
        InitializeXml();
      }
    }
    private void SaveXml()
    {
      var xmlString = XmlGenericSerializer.Serialize(xmlRecord);
      File.WriteAllText(recordPath, xmlString);
    }
    private void ReadXml()
    {
      CheckXml();
      var xmlString = File.ReadAllText(recordPath);
      xmlRecord = XmlGenericSerializer.Deserialize<XmlRecord>(xmlString);

      if (xmlRecord == null)
      {
        InitializeXml();
      }
    }

    int elapsedTime;
    int fps = 24; //Timer 幀數
    Timer timer;
    private void InitializeTimer()
    {
      elapsedTime = 0;

      //非同步計時，較精確(仍有延遲問題)
      timer = new System.Timers.Timer();
      timer.Interval = 1d / fps * 1e3;
      timer.Elapsed += timer_Elapsed;
      timer.AutoReset = true; //重複執行
      UpdateTimerText(0);
    }
    private void timer_Elapsed(object sender, ElapsedEventArgs e)
    {
      elapsedTime += (int)timer.Interval;
      UpdateTimerText(elapsedTime);
    }
    private void UpdateTimerText(int time)
    {
      var (min, sec, ms, tic) = TimeConverter.ToString(time);
      this.Dispatcher.InvokeAsync(() => { tbTimer.Text = $"{min}:{sec}:{tic}"; });
    }

    List<Key> keyList = new List<Key> { Key.Left, Key.Right, Key.Up, Key.Down };
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
      var (IsMove, from, to) = CheckMove(e.Key);
      if (!IsMove)
        return;

      MoveBoard(from, to);
      if (IsComplete)
      {
        CompleteGame();
      }
    }

    Button movingPuzzle;
    (int row, int col) fromBoard;
    (int row, int col) toBoard;
    private (bool, (int row, int col), (int row, int col)) CheckMove(Key key)
    {
      (int row, int col) to = board.IndexOf(0);
      (int row, int col) from = to;
      switch (key)
      {
        case Key.Left:
          from.col++;
          break;
        case Key.Up:
          from.row++;
          break;
        case Key.Right:
          from.col--;
          break;
        case Key.Down:
          from.row--;
          break;
        default:
          //無效的按鍵
          return (false, from, to);
      }
      var IsMove = (!IsIn(from.row, n - 1, 0) ||
        !IsIn(from.col, n - 1, 0)) ? false : true;
      return (IsMove, from, to);
    }
    private void MoveBoard((int row, int col) from, (int row, int col) to)
    {
      //第一次移動，開始計時
      if (!timer.Enabled)
      {
        timer.Start();
      }
      //中斷前次移動的動畫
      if (storyboard != null && IsAnimating)
      {
        storyboard.Stop();
        InitializePuzzles();
      }
      //開始移動
      IsAnimating = true;
      fromBoard = from; toBoard = to; //紀錄此次移動，中斷用
      movingPuzzle = puzzles.Find(p => int.Parse(p.Content as string) == board[from.row, from.col]);
      MovePuzzle(Board2Canvas(from), Board2Canvas(to));
      Swap(ref board[from.row, from.col], ref board[to.row, to.col]);
    }
    Storyboard storyboard;

    private void MovePuzzle((double? Top, double? Left) fromCanvas, (double? Top, double? Left) toCanvas)
    {
      PropertyPath path;
      double? from, to;
      if (fromCanvas.Left == toCanvas.Left)
      {
        path = new PropertyPath(Canvas.TopProperty);
        from = fromCanvas.Top;
        to = toCanvas.Top;
      }
      else if (fromCanvas.Top == toCanvas.Top)
      {
        path = new PropertyPath(Canvas.LeftProperty);
        from = fromCanvas.Left;
        to = toCanvas.Left;
      }
      else
      {
        throw new Exception("??");
      }

      var animation = new DoubleAnimation
      {
        From = from,
        To = to,
        Duration = TimeSpan.FromMilliseconds(100)
      };
      Storyboard.SetTarget(animation, movingPuzzle);
      Storyboard.SetTargetProperty(animation, path);
      storyboard = new Storyboard();
      storyboard.Completed += storyboard_Completed;

      storyboard.Children.Add(animation);
      storyboard.Begin();
    }
    bool IsAnimating = false;
    private void storyboard_Completed(object sender, EventArgs e)
    {
      IsAnimating = false;
    }

    bool IsComplete
    {
      get
      {
        for (int i = 0; i < n; i++)
        {
          for (int j = 0; j < n; j++)
          {
            if (i == n - 1 && j == n - 1)
            {
              //最後一格必為0
              break;

            }
            if (board[i, j] != n * i + (j + 1))
            {
              return false;
            }

          }
        }
        return true;
      }
    }
    private void CompleteGame()
    {
      timer.Stop();

      var (min, sec, ms, tic) = TimeConverter.ToString(elapsedTime);
      var message = "You completed this puzzle in " + $"{min}:{sec}:{tic}.";
      if (record == null || record > elapsedTime)
      {
        Properties.Settings.Default.record = elapsedTime;
        Properties.Settings.Default.Save();

        message = "Congradulations, you made a new record!\n\n" + message;
      }

      //ReadXml();
      //xmlRecord.AddRecord(elapsedTime);
      //SaveXml();

      var messageBox = new MessageBox
      {
        DataContext = message
      };
      DialogHost.Show(messageBox, "dialogMain", closingEventHandler);

      Initialize();
    }

    private void closingEventHandler(object sender, DialogClosingEventArgs eventArgs)
    {
      //throw new NotImplementedException();
    }

    #region Helper Methods
    private double BoardRow2CanvasTop(int row) => lineWidth * (row + 1) + puzzleWidth * row;
    private double BoardCol2CanvasLeft(int col) => lineWidth * (col + 1) + puzzleWidth * col;
    private (double? Top, double? Left) Board2Canvas((int row, int col) board) =>
      (BoardRow2CanvasTop(board.row), BoardCol2CanvasLeft(board.col));
    #endregion

  }
}
