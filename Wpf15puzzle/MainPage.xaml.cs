using System;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static System.Math;
using static CycWpfLibrary.NativeMethod;
using static CycWpfLibrary.Math;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Diagnostics;
using MahApps.Metro.Controls;
using MaterialDesignThemes.Wpf;
using System.Media;
using CycWpfLibrary;

namespace Wpf15puzzle
{
  /// <summary>
  /// MainPage.xaml 的互動邏輯
  /// </summary>
  public partial class MainPage : Page
  {
    public MainPage()
    {
      InitializeComponent();
    }
    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
      //窗體按鍵觸發頁面按鍵
      var window = Window.GetWindow(this);
      if (window != null)
        window.KeyDown += Page_KeyDown;
      else
        throw new Exception("Cannot find window!");

      Initialize();
    }
    private void btnNewGame_Click(object sender, RoutedEventArgs e)
    {
      if (timer.Enabled) timer.Stop();
      Initialize();
    }
    private void Initialize()
    {
      InitializeHeader();
      InitializeCanvas();
      InitializeBoard();
      InitializePuzzles();
      InitializeTimer();
      InitializeXmlRecord();
      //InitializeSettingRecord();
      StartUpAnimation();
    }

    XmlRecord xmlRecord;
    int? record
    {
      get => xmlRecord.dictionary[boardLength];
      set => xmlRecord.dictionary[boardLength] = value;
    }
    string recordPath = "record.xml";
    private void InitializeXmlRecord()
    {
      ReadXml();
      var (min, sec, ms, tic) = TimeFormat.ToString(record);
      tbBest.Text = $"{min}:{sec}:{tic}";
    }
    private void InitializeXml()
    {
      xmlRecord = new XmlRecord();
      SaveXml();
    }
    private void CheckXml()
    {
      if (!File.Exists(recordPath)) InitializeXml();
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

      if (xmlRecord == null) InitializeXml();
    }

    int _boardLength = 4;
    int boardLength
    {
      get => _boardLength;
      set
      {
        _boardLength = value;
        btnMinus.IsEnabled = _boardLength != 3;
        btnPlus.IsEnabled = _boardLength != 6;
      }
    }
    private void InitializeHeader()
    {
      tbHeaderN.Text = (boardLength * boardLength - 1).ToString();
    }
    private void btnMinus_Click(object sender, RoutedEventArgs e)
    {
      boardLength--;
      Initialize();
    }
    private void btnPlus_Click(object sender, RoutedEventArgs e)
    {
      boardLength++;
      Initialize();
    }

    double lineWidth;
    double puzzleWidth;
    private void InitializeCanvas()
    {
      var p = boardLength + 1; // p > n
      var l = (boardLength + 1) * p / (p - boardLength);
      lineWidth = canvasBoard.ActualWidth / l;
      puzzleWidth = canvasBoard.ActualWidth / p;
    }

    Random random = new Random();
    int[,] board;
    private void InitializeBoard()
    {
      board = new int[boardLength, boardLength];
      var L = boardLength * boardLength - 1;
      var numbers = Enumerable.Range(1, L).ToArray();
      do
      {
        numbers = numbers.OrderBy(x => random.Next()).ToArray();
      } while (!IsValid(numbers));
      for (int i = 0, k = 0; i < boardLength && k < L; i++)
      for (var j = 0; j < boardLength && k < L; j++, k++)
        board[i, j] = numbers[k];
    }
    private bool IsValid(int[] numbers)
    {
      var invariant = 0;
      for (var i = 0; i < numbers.Length; i++)
      for (var j = i + 1; j < numbers.Length; j++)
        if (numbers[i] > numbers[j])
          invariant++;

      return invariant % 2 == 0;
    }

    List<Button> puzzles = new List<Button>();
    private void InitializePuzzles()
    {
      canvasBoard.Children.Clear();
      puzzles.Clear();
      for (var row = 0; row < boardLength; row++)
      for (var col = 0; col < boardLength; col++)
      {
        if (board[row, col] == 0) continue;
        var puzzle = new Button
        {
          Content = board[row, col].ToString(),
          Width = puzzleWidth,
          Height = puzzleWidth,
        };
        puzzle.Click += Puzzle_Click;
        canvasBoard.Children.Add(puzzle);
        puzzles.Add(puzzle);
        Canvas.SetTop(puzzle, BoardRow2CanvasTop(row));
        Canvas.SetLeft(puzzle, BoardCol2CanvasLeft(col));
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
        var sb = new Storyboard();

        var animationX = animation.Clone();
        var animationY = animation.Clone();
        Storyboard.SetTarget(animationX, puzzle);
        Storyboard.SetTarget(animationY, puzzle);
        Storyboard.SetTargetProperty(animationX, new PropertyPath("RenderTransform.ScaleX"));
        Storyboard.SetTargetProperty(animationY, new PropertyPath("RenderTransform.ScaleY"));
        sb.Children.Add(animationX);
        sb.Children.Add(animationY);
        sb.Begin(this);
      }
    }

    int elapsedTime;
    int fps = 24; //Timer 幀數
    Timer timer;
    private void InitializeTimer()
    {
      elapsedTime = 0;
      timer?.Stop();

      //非同步計時，較精確(仍有延遲問題)
      timer = new Timer
      {
        Interval = 1d / fps * 1e3
      };
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
      var (min, sec, ms, tic) = TimeFormat.ToString(time);
      Dispatcher?.InvokeAsync(() => { tbTimer.Text = $"{min}:{sec}:{tic}"; });
    }

    List<Key> keyList = new List<Key> { Key.Left, Key.Right, Key.Up, Key.Down };
    Button movingPuzzle;
    private (bool, (int row, int col), (int row, int col)) CheckKey(Key key)
    {
      (int row, int col) to = board.IndexOf(0);
      var from = to;
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
      var isMove = IsIn(from.row, boardLength - 1, 0) && IsIn(from.col, boardLength - 1, 0);
      return (isMove, from, to);
    }
    private void Page_KeyDown(object sender, KeyEventArgs e)
    {
      var (IsMove, from, to) = CheckKey(e.Key);
      if (!IsMove)
        return;

      MoveBoard(from, to);
      if (IsComplete) CompleteGame();
    }

    private (bool, (int row, int col), (int row, int col)) CheckClick(Button puzzle)
    {
      (int row, int col) to = board.IndexOf(0);
      (int row, int col) from = board.IndexOf(int.Parse(puzzle.Content.ToString()));
      if ((to.row == from.row || to.col == from.col) &&
        Abs(to.row - from.row) <= 1 &&
        Abs(to.col - from.col) <= 1)
        return (true, from, to);
      return (false, (0, 0), (0, 0));
    }
    private void Puzzle_Click(object sender, RoutedEventArgs e)
    {
      var (IsMove, from, to) = CheckClick(sender as Button);
      if (!IsMove)
        return;

      MoveBoard(from, to);
      if (IsComplete) CompleteGame();
    }
    private void MoveBoard((int row, int col) from, (int row, int col) to)
    {
      //第一次移動，開始計時
      if (!timer.Enabled) timer.Start();
      //中斷前次移動的動畫
      if (storyboard != null && IsAnimating)
      {
        storyboard.Stop();
        InitializePuzzles();
      }
      //開始移動
      IsAnimating = true;
      movingPuzzle = puzzles.Find(p => int.Parse(p.Content.ToString()) == board[from.row, from.col]);
      MovePuzzle(Board2Canvas(from), Board2Canvas(to));
      Swap(ref board[from.row, from.col], ref board[to.row, to.col]);
    }

    Storyboard storyboard;
    private void MovePuzzle((double? Top, double? Left) fromCanvas, (double? Top, double? Left) toCanvas)
    {
      PropertyPath path;
      double? from, to;
      if (Equals(fromCanvas.Left, toCanvas.Left))
      {
        path = new PropertyPath(Canvas.TopProperty);
        from = fromCanvas.Top;
        to = toCanvas.Top;
      }
      else if (Equals(fromCanvas.Top, toCanvas.Top))
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
    bool IsAnimating;
    private void storyboard_Completed(object sender, EventArgs e)
    {
      IsAnimating = false;
    }

    bool IsComplete
    {
      get
      {
        for (var i = 0; i < boardLength; i++)
        for (var j = 0; j < boardLength; j++)
        {
          if (i == boardLength - 1 && j == boardLength - 1)
          //最後一格必為0
            break;
          if (board[i, j] != boardLength * i + j + 1) return false;
        }

        return true;
      }
    }
    private void CompleteGame()
    {
      timer.Stop();

      var (min, sec, ms, tic) = TimeFormat.ToString(elapsedTime);
      var message = "You completed this puzzle in " + $"{min}:{sec}:{tic}.";

      ReadXml();
      if (record == null || record > elapsedTime)
      {
        record = elapsedTime;
        message = "Congradulations, you made a new record!\n\n" + message;
      }
      SaveXml();

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
    private (double? Top, double? Left) Board2Canvas((int row, int col) b) =>
      (BoardRow2CanvasTop(b.row), BoardCol2CanvasLeft(b.col));
    #endregion

  }
}
