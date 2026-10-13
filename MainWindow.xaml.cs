using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SimpleTetris
{
    public partial class MainWindow : Window
    {
        private const int Cols = 10;
        private const int Rows = 20;
        private const int Cell = 30;

        // 0 означает пустую клетку
        private readonly int[,] board = new int[Rows, Cols];

        private readonly DispatcherTimer timer = new DispatcherTimer();
        private readonly Random random = new Random();

        private int[,] current = new int[4, 4];
        private int pieceX, pieceY;
        private int colorIndex = 1;
        private int score = 0;
        private bool gameOver = false;

        private readonly Brush[] colors =
        {
            Brushes.Transparent,
            Brushes.Cyan,
            Brushes.Yellow,
            Brushes.Magenta,
            Brushes.LimeGreen,
            Brushes.Red,
            Brushes.DodgerBlue,
            Brushes.Orange
        };

        // Семь классических фигур
        private readonly int[][,] pieces =
        {
            new int[,] { {0,0,0,0}, {1,1,1,1}, {0,0,0,0}, {0,0,0,0} }, // i piece
            new int[,] { {0,1,1,0}, {0,1,1,0}, {0,0,0,0}, {0,0,0,0} }, // o piece
            new int[,] { {0,1,0,0}, {1,1,1,0}, {0,0,0,0}, {0,0,0,0} }, // t piece
            new int[,] { {0,1,1,0}, {1,1,0,0}, {0,0,0,0}, {0,0,0,0} }, // s piece
            new int[,] { {1,1,0,0}, {0,1,1,0}, {0,0,0,0}, {0,0,0,0} }, // z piece
            new int[,] { {1,0,0,0}, {1,1,1,0}, {0,0,0,0}, {0,0,0,0} }, // j piece
            new int[,] { {0,0,1,0}, {1,1,1,0}, {0,0,0,0}, {0,0,0,0} } // l piece
        };

        public MainWindow()
        {
            InitializeComponent();

            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();
            SpawnPiece();
            Draw();
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (gameOver)
                return;

            if (!CanMove(pieceX, pieceY + 1, current))
            {
                LockPiece();
                ClearLines();
                SpawnPiece();
            }
            else
            {
                pieceY++;
            }

            Draw();
        }

        private void SpawnPiece()
        {
            int index = random.Next(pieces.Length);
            current = (int[,])pieces[index].Clone();

            colorIndex = index + 1;
            pieceX = 3;
            pieceY = 0;

            if (!CanMove(pieceX, pieceY, current))
            {
                gameOver = true;
                timer.Stop();

                MessageBox.Show(
                    $"Игра окончена! Очки: {score}",
                    "Тетрис");
            }
        }

        private bool CanMove(int x, int y, int[,] shape)
        {
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    if (shape[r, c] == 0)
                        continue;

                    int bx = x + c;
                    int by = y + r;

                    if (bx < 0 || bx >= Cols ||
                        by < 0 || by >= Rows)
                        return false;

                    if (board[by, bx] != 0)
                        return false;
                }
            }

            return true;
        }

        private void LockPiece()
        {
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    if (current[r, c] == 1)
                    {
                        int bx = pieceX + c;
                        int by = pieceY + r;

                        if (by >= 0 && by < Rows &&
                            bx >= 0 && bx < Cols)
                        {
                            board[by, bx] = colorIndex;
                        }
                    }
                }
            }
        }

        private void ClearLines()
        {
            int cleared = 0;

            for (int r = Rows - 1; r >= 0; r--)
            {
                bool full = true;

                for (int c = 0; c < Cols; c++)
                {
                    if (board[r, c] == 0)
                    {
                        full = false;
                        break;
                    }
                }

                if (full)
                {
                    cleared++;

                    // Сдвигаем все строки выше вниз
                    for (int y = r; y > 0; y--)
                    {
                        for (int x = 0; x < Cols; x++)
                            board[y, x] = board[y - 1, x];
                    }

                    for (int x = 0; x < Cols; x++)
                        board[0, x] = 0;

                    // Проверяем эту же строку повторно
                    r++;
                }
            }

            score += cleared * 100;
            ScoreText.Text = $"Очки: {score}";
        }

        private void RotatePiece()
        {
            int[,] rotated = new int[4, 4];

            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    rotated[c, 3 - r] = current[r, c];
                }
            }

            if (CanMove(pieceX, pieceY, rotated))
                current = rotated;
        }

        private void RotatePieceCounterClockwise()
        {
            int[,] rotated = new int[4, 4];

            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    rotated[3 - c, r] = current[r, c];
                }
            }

            if (CanMove(pieceX, pieceY, rotated))
                current = rotated;
        }

        private void RotatePiece180()
        {
            int[,] rotated = new int[4, 4];

            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    rotated[3 - r, 3 - c] = current[r, c];
                }
            }

            if (CanMove(pieceX, pieceY, rotated))
                current = rotated;
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (gameOver)
                return;

            switch (e.Key)
            {
                case Key.Left:
                    if (CanMove(pieceX - 1, pieceY, current))
                        pieceX--;
                    break;

                case Key.Right:
                    if (CanMove(pieceX + 1, pieceY, current))
                        pieceX++;
                    break;

                case Key.Down:
                    if (CanMove(pieceX, pieceY + 1, current))
                        pieceY++;
                    else
                    {
                        LockPiece();
                        ClearLines();
                        SpawnPiece();
                    }
                    break;

                case Key.X:
                    RotatePiece();
                    break;


                case Key.Z:
                    RotatePieceCounterClockwise();
                    break;

                case Key.A:
                    RotatePiece180();
                    break;

                case Key.Space:
                    // Мгновенное падение до упора
                    while (CanMove(pieceX, pieceY + 1, current))
                        pieceY++;

                    LockPiece();
                    ClearLines();
                    SpawnPiece();
                    break;
            }

            Draw();
            e.Handled = true;
        }

        private void Draw()
        {
            GameCanvas.Children.Clear();

            // Уже упавшие блоки
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    if (board[r, c] != 0)
                        DrawCell(c, r, colors[board[r, c]]);
                }
            }

            // Текущая фигура
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    if (current[r, c] == 1)
                    {
                        DrawCell(
                            pieceX + c,
                            pieceY + r,
                            colors[colorIndex]);
                    }
                }
            }

            // Сетка
            for (int x = 0; x <= Cols; x++)
            {
                var line = new System.Windows.Shapes.Line
                {
                    X1 = x * Cell,
                    X2 = x * Cell,
                    Y1 = 0,
                    Y2 = Rows * Cell,
                    Stroke = Brushes.Gray,
                    StrokeThickness = 0.3
                };

                GameCanvas.Children.Add(line);
            }

            for (int y = 0; y <= Rows; y++)
            {
                var line = new System.Windows.Shapes.Line
                {
                    X1 = 0,
                    X2 = Cols * Cell,
                    Y1 = y * Cell,
                    Y2 = y * Cell,
                    Stroke = Brushes.Gray,
                    StrokeThickness = 0.3
                };

                GameCanvas.Children.Add(line);
            }
        }

        private void DrawCell(int x, int y, Brush color)
        {
            var rect = new Rectangle
            {
                Width = Cell - 1,
                Height = Cell - 1,
                Fill = color,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            Canvas.SetLeft(rect, x * Cell);
            Canvas.SetTop(rect, y * Cell);

            GameCanvas.Children.Add(rect);
        }
    }
}