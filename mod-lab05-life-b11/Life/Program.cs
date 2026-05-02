<<<<<<< HEAD
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
=======
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
>>>>>>> 1a4727838cc9b33eabb82c022b99ea634815a6c3
using System.Threading;

namespace cli_life
{
<<<<<<< HEAD
    public class Settings
    {
        public int Width      { get; set; } = 50;
        public int Height     { get; set; } = 20;
        public int CellSize   { get; set; } = 1;
        public double Density { get; set; } = 0.5;
        public int DelayMs    { get; set; } = 1000;

        public static Settings Load(string path)
        {
            if (!File.Exists(path)) return new Settings();
            return JsonSerializer.Deserialize<Settings>(File.ReadAllText(path)) ?? new Settings();
        }

        public void Save(string path)
        {
            File.WriteAllText(path, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        }
    }

=======
>>>>>>> 1a4727838cc9b33eabb82c022b99ea634815a6c3
    public class Cell
    {
        public bool IsAlive;
        public readonly List<Cell> neighbors = new List<Cell>();
        private bool IsAliveNext;
<<<<<<< HEAD

        public void DetermineNextLiveState()
        {
            int liveNeighbors = neighbors.Count(x => x.IsAlive);
=======
        public void DetermineNextLiveState()
        {
            int liveNeighbors = neighbors.Where(x => x.IsAlive).Count();
>>>>>>> 1a4727838cc9b33eabb82c022b99ea634815a6c3
            if (IsAlive)
                IsAliveNext = liveNeighbors == 2 || liveNeighbors == 3;
            else
                IsAliveNext = liveNeighbors == 3;
        }
<<<<<<< HEAD

=======
>>>>>>> 1a4727838cc9b33eabb82c022b99ea634815a6c3
        public void Advance()
        {
            IsAlive = IsAliveNext;
        }
    }
<<<<<<< HEAD

    public static class FigureClassifier
    {
        private static readonly List<(string Name, List<(int, int)> Cells)> Patterns =
            new List<(string, List<(int, int)>)>
        {
            ("Block",   new List<(int,int)> { (0,0),(1,0),(0,1),(1,1) }),
            ("Beehive", new List<(int,int)> { (1,0),(2,0),(0,1),(3,1),(1,2),(2,2) }),
            ("Loaf",    new List<(int,int)> { (1,0),(2,0),(0,1),(3,1),(1,2),(3,2),(2,3) }),
            ("Boat",    new List<(int,int)> { (0,0),(1,0),(0,1),(2,1),(1,2) }),
            ("Tub",     new List<(int,int)> { (1,0),(0,1),(2,1),(1,2) }),
            ("Blinker", new List<(int,int)> { (0,0),(1,0),(2,0) }),
            ("Glider",  new List<(int,int)> { (1,0),(2,1),(0,2),(1,2),(2,2) }),
        };

        public static string Classify(List<(int col, int row)> combo)
        {
            int minC = combo.Min(p => p.col);
            int minR = combo.Min(p => p.row);
            var normalized = combo
                .Select(p => (p.col - minC, p.row - minR))
                .OrderBy(p => p.Item1).ThenBy(p => p.Item2)
                .ToList();

            foreach (var (name, pattern) in Patterns)
            {
                var candidate = pattern;
                for (int rot = 0; rot < 4; rot++)
                {
                    if (Matches(normalized, candidate)) return name;
                    candidate = Rotate(candidate);
                }
            }
            return "Unknown";
        }

        private static bool Matches(List<(int, int)> a, List<(int, int)> b)
        {
            if (a.Count != b.Count) return false;
            int minC = b.Min(p => p.Item1);
            int minR = b.Min(p => p.Item2);
            var bNorm = b
                .Select(p => (p.Item1 - minC, p.Item2 - minR))
                .OrderBy(p => p.Item1).ThenBy(p => p.Item2)
                .ToList();
            return a.SequenceEqual(bNorm);
        }

        private static List<(int, int)> Rotate(List<(int, int)> pts) =>
            pts.Select(p => (-p.Item2, p.Item1)).ToList();
    }

    public static class StabilityAnalyzer
    {
        public static int GenerationsToStable(
            int width, int height, int cellSize, double density,
            int windowSize = 10, int maxGenerations = 2000)
        {
            var board = new Board(width, height, cellSize, density);
            var history = new Queue<int>();

            for (int gen = 0; gen < maxGenerations; gen++)
            {
                int alive = board.CountAlive();
                history.Enqueue(alive);
                if (history.Count > windowSize) history.Dequeue();
                if (history.Count == windowSize && history.All(v => v == history.Peek()))
                    return gen;
                board.Advance();
            }
            return maxGenerations;
        }

        public static Dictionary<double, double> RunExperiment(
            double[] densities,
            int trials         = 10,
            int width          = 50,
            int height         = 20,
            int cellSize       = 1,
            int windowSize     = 10,
            int maxGenerations = 2000)
        {
            var result = new Dictionary<double, double>();
            foreach (double d in densities)
            {
                double sum = 0;
                for (int i = 0; i < trials; i++)
                    sum += GenerationsToStable(width, height, cellSize, d, windowSize, maxGenerations);
                result[d] = sum / trials;
            }
            return result;
        }
    }

=======
>>>>>>> 1a4727838cc9b33eabb82c022b99ea634815a6c3
    public class Board
    {
        public readonly Cell[,] Cells;
        public readonly int CellSize;

<<<<<<< HEAD
        public int Columns => Cells.GetLength(0);
        public int Rows    => Cells.GetLength(1);
        public int Width   => Columns * CellSize;
        public int Height  => Rows * CellSize;

        private readonly Random rand = new Random();

        public Board(int width, int height, int cellSize, double liveDensity = 0.1)
        {
            CellSize = cellSize;
=======
        public int Columns { get { return Cells.GetLength(0); } }
        public int Rows { get { return Cells.GetLength(1); } }
        public int Width { get { return Columns * CellSize; } }
        public int Height { get { return Rows * CellSize; } }

        public Board(int width, int height, int cellSize, double liveDensity = .1)
        {
            CellSize = cellSize;

>>>>>>> 1a4727838cc9b33eabb82c022b99ea634815a6c3
            Cells = new Cell[width / cellSize, height / cellSize];
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    Cells[x, y] = new Cell();
<<<<<<< HEAD
=======

>>>>>>> 1a4727838cc9b33eabb82c022b99ea634815a6c3
            ConnectNeighbors();
            Randomize(liveDensity);
        }

<<<<<<< HEAD
=======
        readonly Random rand = new Random();
>>>>>>> 1a4727838cc9b33eabb82c022b99ea634815a6c3
        public void Randomize(double liveDensity)
        {
            foreach (var cell in Cells)
                cell.IsAlive = rand.NextDouble() < liveDensity;
        }

        public void Advance()
        {
            foreach (var cell in Cells)
                cell.DetermineNextLiveState();
            foreach (var cell in Cells)
                cell.Advance();
        }
<<<<<<< HEAD

        public int CountAlive() => Cells.Cast<Cell>().Count(c => c.IsAlive);

=======
>>>>>>> 1a4727838cc9b33eabb82c022b99ea634815a6c3
        private void ConnectNeighbors()
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    int xL = (x > 0) ? x - 1 : Columns - 1;
                    int xR = (x < Columns - 1) ? x + 1 : 0;
<<<<<<< HEAD
=======

>>>>>>> 1a4727838cc9b33eabb82c022b99ea634815a6c3
                    int yT = (y > 0) ? y - 1 : Rows - 1;
                    int yB = (y < Rows - 1) ? y + 1 : 0;

                    Cells[x, y].neighbors.Add(Cells[xL, yT]);
<<<<<<< HEAD
                    Cells[x, y].neighbors.Add(Cells[x,  yT]);
                    Cells[x, y].neighbors.Add(Cells[xR, yT]);
                    Cells[x, y].neighbors.Add(Cells[xL, y ]);
                    Cells[x, y].neighbors.Add(Cells[xR, y ]);
                    Cells[x, y].neighbors.Add(Cells[xL, yB]);
                    Cells[x, y].neighbors.Add(Cells[x,  yB]);
=======
                    Cells[x, y].neighbors.Add(Cells[x, yT]);
                    Cells[x, y].neighbors.Add(Cells[xR, yT]);
                    Cells[x, y].neighbors.Add(Cells[xL, y]);
                    Cells[x, y].neighbors.Add(Cells[xR, y]);
                    Cells[x, y].neighbors.Add(Cells[xL, yB]);
                    Cells[x, y].neighbors.Add(Cells[x, yB]);
>>>>>>> 1a4727838cc9b33eabb82c022b99ea634815a6c3
                    Cells[x, y].neighbors.Add(Cells[xR, yB]);
                }
            }
        }
<<<<<<< HEAD

        public void SaveState(string path)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{Columns} {Rows} {CellSize}");
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                    sb.Append(Cells[col, row].IsAlive ? '1' : '0');
                sb.AppendLine();
            }
            File.WriteAllText(path, sb.ToString());
        }

        public static Board LoadState(string path)
        {
            string[] lines = File.ReadAllLines(path);
            string[] header = lines[0].Split(' ');
            int cols     = int.Parse(header[0]);
            int rows     = int.Parse(header[1]);
            int cellSize = int.Parse(header[2]);

            var board = new Board(cols * cellSize, rows * cellSize, cellSize, 0.0);
            for (int row = 0; row < rows && row + 1 < lines.Length; row++)
                for (int col = 0; col < cols && col < lines[row + 1].Length; col++)
                    board.Cells[col, row].IsAlive = lines[row + 1][col] == '1';
            return board;
        }

        public List<List<(int col, int row)>> GetCombinations()
        {
            var visited = new bool[Columns, Rows];
            var result  = new List<List<(int, int)>>();

            for (int col = 0; col < Columns; col++)
            {
                for (int row = 0; row < Rows; row++)
                {
                    if (!Cells[col, row].IsAlive || visited[col, row]) continue;

                    var group = new List<(int, int)>();
                    var queue = new Queue<(int, int)>();
                    queue.Enqueue((col, row));
                    visited[col, row] = true;

                    while (queue.Count > 0)
                    {
                        var (c, r) = queue.Dequeue();
                        group.Add((c, r));
                        for (int dc = -1; dc <= 1; dc++)
                        {
                            for (int dr = -1; dr <= 1; dr++)
                            {
                                if (dc == 0 && dr == 0) continue;
                                int nc = (c + dc + Columns) % Columns;
                                int nr = (r + dr + Rows)    % Rows;
                                if (!visited[nc, nr] && Cells[nc, nr].IsAlive)
                                {
                                    visited[nc, nr] = true;
                                    queue.Enqueue((nc, nr));
                                }
                            }
                        }
                    }
                    result.Add(group);
                }
            }
            return result;
        }
    }

    class Program
    {
        static Board board;
        static Settings settings;

        static readonly string SettingsPath  = "settings.json";
        static readonly string SavePath      = Path.Combine("Data", "state.txt");
        static readonly string DataPath      = Path.Combine("Data", "data.txt");
        static readonly string ColoniesDir   = "colonies";

        static void EnsureDataDir() => Directory.CreateDirectory("Data");

        static void Reset()
        {
            board = new Board(settings.Width, settings.Height, settings.CellSize, settings.Density);
        }

=======
    }
    class Program
    {
        static Board board;
        static private void Reset()
        {
            board = new Board(
                width: 50,
                height: 20,
                cellSize: 1,
                liveDensity: 0.5);
        }
>>>>>>> 1a4727838cc9b33eabb82c022b99ea634815a6c3
        static void Render()
        {
            for (int row = 0; row < board.Rows; row++)
            {
<<<<<<< HEAD
                for (int col = 0; col < board.Columns; col++)
                    Console.Write(board.Cells[col, row].IsAlive ? '*' : ' ');
                Console.WriteLine();
            }
        }

        static void RunSimulation()
        {
            int gen = 0;
            while (true)
            {
                Console.Clear();
                Render();
                Console.WriteLine($"Gen: {gen}  Alive: {board.CountAlive()}  Groups: {board.GetCombinations().Count}");
                Console.WriteLine("[S] Save  [Q] Quit");
                board.Advance();
                gen++;

                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.S)
                    {
                        EnsureDataDir();
                        board.SaveState(SavePath);
                        Console.WriteLine($"Сохранено в {SavePath}");
                        Thread.Sleep(600);
                    }
                    else if (key == ConsoleKey.Q) break;
                }
                Thread.Sleep(settings.DelayMs);
            }
        }

        static void LoadColony()
        {
            if (!Directory.Exists(ColoniesDir))
            {
                Console.WriteLine("Папка colonies/ не найдена.");
                return;
            }
            var files = Directory.GetFiles(ColoniesDir, "*.txt");
            if (files.Length == 0) { Console.WriteLine("Нет файлов колоний."); return; }

            Console.WriteLine("Доступные колонии:");
            for (int i = 0; i < files.Length; i++)
                Console.WriteLine($"  {i + 1}. {Path.GetFileName(files[i])}");
            Console.Write("Выберите номер: ");

            if (!int.TryParse(Console.ReadLine(), out int idx) || idx < 1 || idx > files.Length)
            {
                Console.WriteLine("Неверный номер.");
                return;
            }
            board = Board.LoadState(files[idx - 1]);
            Console.WriteLine($"Загружена: {Path.GetFileName(files[idx - 1])}");
            Thread.Sleep(400);
            RunSimulation();
        }

        static void AnalyzeField()
        {
            var combos = board.GetCombinations();
            Console.WriteLine($"Живых клеток: {board.CountAlive()}");
            Console.WriteLine($"Связных групп: {combos.Count}");

            var counts = new Dictionary<string, int>();
            foreach (var combo in combos)
            {
                string name = FigureClassifier.Classify(combo);
                counts[name] = counts.GetValueOrDefault(name) + 1;
            }

            Console.WriteLine("Классификация:");
            foreach (var kv in counts.OrderByDescending(k => k.Value))
                Console.WriteLine($"  {kv.Key}: {kv.Value}");
        }

        static void RunResearch()
        {
            Console.WriteLine("Исследование стабилизации...");
            double[] densities = { 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9 };
            var results = StabilityAnalyzer.RunExperiment(
                densities, trials: 10,
                width: settings.Width, height: settings.Height, cellSize: settings.CellSize);

            EnsureDataDir();
            var sb = new StringBuilder();
            sb.AppendLine("Density\tAvgGenerations");
            foreach (var kv in results.OrderBy(k => k.Key))
            {
                sb.AppendLine($"{kv.Key:F1}\t{kv.Value:F2}");
                Console.WriteLine($"  Density={kv.Key:F1}  AvgGen={kv.Value:F1}");
            }
            File.WriteAllText(DataPath, sb.ToString());
            Console.WriteLine($"Данные сохранены в {DataPath}");
        }

        static void Main(string[] args)
        {
            EnsureDataDir();

            settings = Settings.Load(SettingsPath);
            settings.Save(SettingsPath);

            Reset();

            while (true)
            {
                Console.WriteLine("\n=== Game of Life ===");
                Console.WriteLine("1 - Новая симуляция");
                Console.WriteLine("2 - Загрузить сохранение");
                Console.WriteLine("3 - Загрузить колонию");
                Console.WriteLine("4 - Анализ поля");
                Console.WriteLine("5 - Исследование стабилизации");
                Console.WriteLine("6 - Пересоздать поле");
                Console.WriteLine("0 - Выход");
                Console.Write("> ");

                switch (Console.ReadLine()?.Trim())
                {
                    case "1": RunSimulation(); break;
                    case "2":
                        if (File.Exists(SavePath)) { board = Board.LoadState(SavePath); RunSimulation(); }
                        else Console.WriteLine("Файл сохранения не найден.");
                        break;
                    case "3": LoadColony(); break;
                    case "4": AnalyzeField(); Console.ReadLine(); break;
                    case "5": RunResearch(); Console.ReadLine(); break;
                    case "6": Reset(); Console.WriteLine("Поле пересоздано."); break;
                    case "0": return;
                    default: Console.WriteLine("Неверный выбор."); break;
                }
            }
        }
    }
}
=======
                for (int col = 0; col < board.Columns; col++)   
                {
                    var cell = board.Cells[col, row];
                    if (cell.IsAlive)
                    {
                        Console.Write('*');
                    }
                    else
                    {
                        Console.Write(' ');
                    }
                }
                Console.Write('\n');
            }
        }
        static void Main(string[] args)
        {
            Reset();
            while(true)
            {
                Console.Clear();
                Render();
                board.Advance();
                Thread.Sleep(1000);
            }
        }
    }
}
>>>>>>> 1a4727838cc9b33eabb82c022b99ea634815a6c3
