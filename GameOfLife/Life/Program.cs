using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using ScottPlot;

namespace Life
{
    public class SimulationConfig
    {
        public int Width        { get; set; } = 40;
        public int Height       { get; set; } = 20;
        public double Density   { get; set; } = 0.3;
        public int Delay        { get; set; } = 100;
        public int MaxSteps     { get; set; } = 1000;
        public int StableWindow { get; set; } = 10;

        public static SimulationConfig Load(string path)
        {
            if (!File.Exists(path))
                return new SimulationConfig();
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<SimulationConfig>(json) ?? new SimulationConfig();
        }

        public void Save(string path)
        {
            var opts = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(path, JsonSerializer.Serialize(this, opts));
        }
    }

    public class Cell
    {
        public bool Alive { get; set; }

        public Cell(bool alive = false) => Alive = alive;

        public Cell Clone() => new Cell(Alive);

        public override string ToString() => Alive ? "#" : ".";
    }

    public class Board
    {
        public int Width  { get; }
        public int Height { get; }

        private Cell[,] _grid;

        public Board(int width, int height)
        {
            Width  = width;
            Height = height;
            _grid  = new Cell[height, width];
            for (int r = 0; r < height; r++)
                for (int c = 0; c < width; c++)
                    _grid[r, c] = new Cell();
        }

        public Cell this[int row, int col] => _grid[row, col];

        public void Randomize(double density, int? seed = null)
        {
            var rng = seed.HasValue ? new Random(seed.Value) : new Random();
            for (int r = 0; r < Height; r++)
                for (int c = 0; c < Width; c++)
                    _grid[r, c].Alive = rng.NextDouble() < density;
        }

        public int CountNeighbours(int row, int col)
        {
            int count = 0;
            for (int dr = -1; dr <= 1; dr++)
            for (int dc = -1; dc <= 1; dc++)
            {
                if (dr == 0 && dc == 0) continue;
                int nr = (row + dr + Height) % Height;
                int nc = (col + dc + Width)  % Width;
                if (_grid[nr, nc].Alive) count++;
            }
            return count;
        }

        public void Step()
        {
            var next = new Cell[Height, Width];
            for (int r = 0; r < Height; r++)
            for (int c = 0; c < Width; c++)
            {
                int n     = CountNeighbours(r, c);
                bool alive = _grid[r, c].Alive;
                next[r, c] = new Cell(alive ? (n == 2 || n == 3) : (n == 3));
            }
            _grid = next;
        }

        public int CountAlive()
        {
            int sum = 0;
            for (int r = 0; r < Height; r++)
                for (int c = 0; c < Width; c++)
                    if (_grid[r, c].Alive) sum++;
            return sum;
        }

        public int CountClusters()
        {
            bool[,] visited = new bool[Height, Width];
            int clusters = 0;
            for (int r = 0; r < Height; r++)
            for (int c = 0; c < Width; c++)
            {
                if (_grid[r, c].Alive && !visited[r, c])
                {
                    FloodFill(r, c, visited);
                    clusters++;
                }
            }
            return clusters;
        }

        private void FloodFill(int row, int col, bool[,] visited)
        {
            var stack = new Stack<(int, int)>();
            stack.Push((row, col));
            while (stack.Count > 0)
            {
                var (r, c) = stack.Pop();
                if (r < 0 || r >= Height || c < 0 || c >= Width) continue;
                if (visited[r, c] || !_grid[r, c].Alive) continue;
                visited[r, c] = true;
                for (int dr = -1; dr <= 1; dr++)
                for (int dc = -1; dc <= 1; dc++)
                    if (dr != 0 || dc != 0)
                        stack.Push((r + dr, c + dc));
            }
        }

        public void SaveToFile(string path)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{Width} {Height}");
            for (int r = 0; r < Height; r++)
            {
                for (int c = 0; c < Width; c++)
                    sb.Append(_grid[r, c].Alive ? '1' : '0');
                sb.AppendLine();
            }
            File.WriteAllText(path, sb.ToString());
        }

        public static Board LoadFromFile(string path)
        {
            string[] lines = File.ReadAllLines(path);
            string[] dims  = lines[0].Trim().Split(' ');
            int w = int.Parse(dims[0]);
            int h = int.Parse(dims[1]);
            var board = new Board(w, h);
            for (int r = 0; r < h && r + 1 < lines.Length; r++)
            {
                string row = lines[r + 1].Trim();
                for (int c = 0; c < w && c < row.Length; c++)
                    board._grid[r, c].Alive = row[c] == '1';
            }
            return board;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (int r = 0; r < Height; r++)
            {
                for (int c = 0; c < Width; c++)
                    sb.Append(_grid[r, c]);
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }

    public class GameController
    {
        public Board Board      { get; private set; }
        public int Generation   { get; private set; }

        private readonly SimulationConfig _cfg;

        public GameController(SimulationConfig cfg)
        {
            _cfg  = cfg;
            Board = new Board(cfg.Width, cfg.Height);
        }

        public void LoadColony(string path)
        {
            Board = Board.LoadFromFile(path);
            Generation = 0;
        }

        public void StartRandom(int? seed = null)
        {
            Board.Randomize(_cfg.Density, seed);
            Generation = 0;
        }

        public void Tick()
        {
            Board.Step();
            Generation++;
        }

        public int RunUntilStable()
        {
            var window = new Queue<int>();
            int step = 0;

            while (step < _cfg.MaxSteps)
            {
                Board.Step();
                step++;
                int alive = Board.CountAlive();
                window.Enqueue(alive);

                if (window.Count > _cfg.StableWindow)
                    window.Dequeue();

                if (window.Count == _cfg.StableWindow && IsStable(window))
                    return step;
            }

            return step;
        }

        private static bool IsStable(Queue<int> window)
        {
            int first = -1;
            foreach (int v in window)
            {
                if (first < 0) { first = v; continue; }
                if (v != first) return false;
            }
            return true;
        }
    }

    public class PatternMask
    {
        public string Name   { get; }
        public bool[,] Shape { get; }

        public PatternMask(string name, bool[,] shape)
        {
            Name  = name;
            Shape = shape;
        }
    }

    public static class PatternClassifier
    {
        private static readonly List<PatternMask> _patterns = new List<PatternMask>
        {
            new PatternMask("Block", new bool[,]
            {
                { true, true },
                { true, true }
            }),
            new PatternMask("Beehive", new bool[,]
            {
                { false, true,  true,  false },
                { true,  false, false, true  },
                { false, true,  true,  false }
            }),
            new PatternMask("Loaf", new bool[,]
            {
                { false, true,  true,  false },
                { true,  false, false, true  },
                { false, true,  false, true  },
                { false, false, true,  false }
            }),
            new PatternMask("Blinker-H", new bool[,]
            {
                { true, true, true }
            }),
            new PatternMask("Blinker-V", new bool[,]
            {
                { true  },
                { true  },
                { true  }
            }),
            new PatternMask("Glider-A", new bool[,]
            {
                { false, true,  false },
                { false, false, true  },
                { true,  true,  true  }
            }),
            new PatternMask("Glider-B", new bool[,]
            {
                { true,  false, false },
                { false, true,  true  },
                { true,  true,  false }
            }),
        };

        public static Dictionary<string, int> Classify(Board board)
        {
            var counts = new Dictionary<string, int>();
            foreach (var p in _patterns)
                counts[p.Name] = 0;
            counts["Unknown"] = 0;

            bool[,] visited = new bool[board.Height, board.Width];

            for (int r = 0; r < board.Height; r++)
            for (int c = 0; c < board.Width; c++)
            {
                if (!board[r, c].Alive || visited[r, c]) continue;

                var cells = ExtractCluster(board, r, c, visited);
                bool[,] bbox = ExtractBoundingBox(board, cells);
                string name = MatchPattern(bbox);
                if (!counts.ContainsKey(name)) counts[name] = 0;
                counts[name]++;
            }

            return counts;
        }

        private static List<(int r, int c)> ExtractCluster(Board board, int startR, int startC, bool[,] visited)
        {
            var cells = new List<(int, int)>();
            var stack = new Stack<(int, int)>();
            stack.Push((startR, startC));
            while (stack.Count > 0)
            {
                var (r, c) = stack.Pop();
                if (r < 0 || r >= board.Height || c < 0 || c >= board.Width) continue;
                if (visited[r, c] || !board[r, c].Alive) continue;
                visited[r, c] = true;
                cells.Add((r, c));
                for (int dr = -1; dr <= 1; dr++)
                for (int dc = -1; dc <= 1; dc++)
                    if (dr != 0 || dc != 0)
                        stack.Push((r + dr, c + dc));
            }
            return cells;
        }

        private static bool[,] ExtractBoundingBox(Board board, List<(int r, int c)> cells)
        {
            int minR = int.MaxValue, maxR = int.MinValue;
            int minC = int.MaxValue, maxC = int.MinValue;
            foreach (var (r, c) in cells)
            {
                if (r < minR) minR = r;
                if (r > maxR) maxR = r;
                if (c < minC) minC = c;
                if (c > maxC) maxC = c;
            }
            int h = maxR - minR + 1;
            int w = maxC - minC + 1;
            var box = new bool[h, w];
            foreach (var (r, c) in cells)
                box[r - minR, c - minC] = true;
            return box;
        }

        private static string MatchPattern(bool[,] box)
        {
            foreach (var p in _patterns)
            {
                var rotated = p.Shape;
                for (int rot = 0; rot < 4; rot++)
                {
                    if (ShapesEqual(box, rotated)) return p.Name;
                    rotated = Rotate90(rotated);
                }
                var flipped = FlipH(p.Shape);
                rotated = flipped;
                for (int rot = 0; rot < 4; rot++)
                {
                    if (ShapesEqual(box, rotated)) return p.Name;
                    rotated = Rotate90(rotated);
                }
            }
            return "Unknown";
        }

        private static bool ShapesEqual(bool[,] a, bool[,] b)
        {
            if (a.GetLength(0) != b.GetLength(0)) return false;
            if (a.GetLength(1) != b.GetLength(1)) return false;
            for (int r = 0; r < a.GetLength(0); r++)
            for (int c = 0; c < a.GetLength(1); c++)
                if (a[r, c] != b[r, c]) return false;
            return true;
        }

        private static bool[,] Rotate90(bool[,] m)
        {
            int h = m.GetLength(0), w = m.GetLength(1);
            var result = new bool[w, h];
            for (int r = 0; r < h; r++)
            for (int c = 0; c < w; c++)
                result[c, h - 1 - r] = m[r, c];
            return result;
        }

        private static bool[,] FlipH(bool[,] m)
        {
            int h = m.GetLength(0), w = m.GetLength(1);
            var result = new bool[h, w];
            for (int r = 0; r < h; r++)
            for (int c = 0; c < w; c++)
                result[r, w - 1 - c] = m[r, c];
            return result;
        }
    }

    public static class Researcher
    {
        public static void RunDensityExperiment(string dataPath, string plotPath)
        {
            double[] densities = { 0.10, 0.20, 0.30, 0.40, 0.50, 0.60, 0.70, 0.80, 0.90 };
            int trialsPerDensity = 20;
            var results = new List<(double density, double avgGen)>();

            foreach (double d in densities)
            {
                int total = 0;
                for (int t = 0; t < trialsPerDensity; t++)
                {
                    var cfg = new SimulationConfig
                    {
                        Width        = 40,
                        Height       = 20,
                        Density      = d,
                        MaxSteps     = 500,
                        StableWindow = 8
                    };
                    var ctrl = new GameController(cfg);
                    ctrl.StartRandom(t);
                    total += ctrl.RunUntilStable();
                }
                results.Add((d, (double)total / trialsPerDensity));
            }

            SaveData(dataPath, results);
            SavePlot(plotPath, results);
        }

        private static void SaveData(string path, List<(double density, double avgGen)> data)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var sb = new StringBuilder();
            sb.AppendLine("density avg_generations");
            foreach (var (d, g) in data)
                sb.AppendLine($"{d:F2} {g:F2}");
            File.WriteAllText(path, sb.ToString());
        }

        private static void SavePlot(string path, List<(double density, double avgGen)> data)
        {
            var plt = new Plot();
            double[] xs = new double[data.Count];
            double[] ys = new double[data.Count];
            for (int i = 0; i < data.Count; i++)
            {
                xs[i] = data[i].density;
                ys[i] = data[i].avgGen;
            }
            plt.Add.Scatter(xs, ys);
            plt.XLabel("Плотность заполнения");
            plt.YLabel("Среднее число поколений");
            plt.Title("Переход в стабильную фазу");
            plt.SavePng(path, 800, 500);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string cfgPath  = "settings.json";
            string savePath = "state.txt";
            string dataPath = Path.Combine("..", "Data", "data.txt");
            string plotPath = Path.Combine("..", "Data", "plot.png");

            var cfg = SimulationConfig.Load(cfgPath);
            cfg.Save(cfgPath);

            Console.WriteLine("Игра Жизнь");
            Console.WriteLine("1 - случайное поле");
            Console.WriteLine("2 - загрузить колонию");
            Console.WriteLine("3 - загрузить сохранение");
            Console.WriteLine("4 - провести исследование");
            Console.Write("> ");
            string choice = Console.ReadLine() ?? "1";

            var ctrl = new GameController(cfg);

            if (choice == "2")
            {
                Console.Write("Путь к файлу колонии: ");
                string colonyPath = Console.ReadLine() ?? "";
                ctrl.LoadColony(colonyPath);
            }
            else if (choice == "3")
            {
                ctrl.LoadColony(savePath);
            }
            else if (choice == "4")
            {
                Console.WriteLine("Запуск эксперимента...");
                Researcher.RunDensityExperiment(dataPath, plotPath);
                Console.WriteLine($"Данные: {dataPath}");
                Console.WriteLine($"График: {plotPath}");
                return;
            }
            else
            {
                ctrl.StartRandom();
            }

            Console.WriteLine("Нажмите S для сохранения, Q для выхода");

            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Поколение: {ctrl.Generation}  Живых: {ctrl.Board.CountAlive()}  Кластеров: {ctrl.Board.CountClusters()}");

                var classification = PatternClassifier.Classify(ctrl.Board);
                var sb2 = new StringBuilder();
                foreach (var kv in classification)
                    if (kv.Value > 0)
                        sb2.Append($"  {kv.Key}: {kv.Value}");
                if (sb2.Length > 0)
                    Console.WriteLine("Фигуры:" + sb2);

                Console.Write(ctrl.Board.ToString());

                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.Q) break;
                    if (key == ConsoleKey.S)
                    {
                        ctrl.Board.SaveToFile(savePath);
                        Console.WriteLine($"Сохранено в {savePath}");
                    }
                }

                ctrl.Tick();
                System.Threading.Thread.Sleep(cfg.Delay);
            }
        }
    }
}
