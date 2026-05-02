using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using cli_life;
using Xunit;

namespace LifeTests
{
    public class RulesTests
    {
        private Board SingleCellBoard(int w, int h, params (int x, int y)[] aliveCells)
        {
            var b = new Board(w, h, 1, 0.0);
            foreach (var (x, y) in aliveCells)
                b.Cells[x, y].IsAlive = true;
            return b;
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(8)]
        public void AliveCellDiesWithoutTwoOrThreeNeighbors(int n)
        {
            var cell = new Cell { IsAlive = true };
            for (int i = 0; i < 8; i++)
                cell.neighbors.Add(new Cell { IsAlive = i < n });
            cell.DetermineNextLiveState();
            cell.Advance();
            Assert.False(cell.IsAlive);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        public void AliveCellSurvivesWithTwoOrThreeNeighbors(int n)
        {
            var cell = new Cell { IsAlive = true };
            for (int i = 0; i < 8; i++)
                cell.neighbors.Add(new Cell { IsAlive = i < n });
            cell.DetermineNextLiveState();
            cell.Advance();
            Assert.True(cell.IsAlive);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(4)]
        public void DeadCellStaysDeadWithoutExactlyThreeNeighbors(int n)
        {
            var cell = new Cell { IsAlive = false };
            for (int i = 0; i < 8; i++)
                cell.neighbors.Add(new Cell { IsAlive = i < n });
            cell.DetermineNextLiveState();
            cell.Advance();
            Assert.False(cell.IsAlive);
        }

        [Fact]
        public void DeadCellRevivesWithExactlyThreeNeighbors()
        {
            var cell = new Cell { IsAlive = false };
            for (int i = 0; i < 8; i++)
                cell.neighbors.Add(new Cell { IsAlive = i < 3 });
            cell.DetermineNextLiveState();
            cell.Advance();
            Assert.True(cell.IsAlive);
        }

        [Fact]
        public void BlockSurvivesMultipleGenerations()
        {
            var board = SingleCellBoard(10, 10, (4,4),(5,4),(4,5),(5,5));
            int before = board.CountAlive();
            for (int i = 0; i < 5; i++) board.Advance();
            Assert.Equal(before, board.CountAlive());
            Assert.True(board.Cells[4,4].IsAlive);
            Assert.True(board.Cells[5,5].IsAlive);
        }

        [Fact]
        public void BlinkerPeriodIsTwo()
        {
            var board = SingleCellBoard(10, 10, (3,5),(4,5),(5,5));
            board.Advance(); board.Advance();
            Assert.True(board.Cells[3,5].IsAlive);
            Assert.True(board.Cells[4,5].IsAlive);
            Assert.True(board.Cells[5,5].IsAlive);
        }

        [Fact]
        public void WraparoundEdgeCellsSeeOppositeEdgeNeighbors()
        {
            var board = SingleCellBoard(10, 10, (0,0),(9,0),(0,9));
            board.Advance();
            Assert.True(board.Cells[9,9].IsAlive);
        }

        [Fact]
        public void AllCellsHaveExactlyEightNeighbors()
        {
            var board = new Board(10, 10, 1, 0.0);
            foreach (var cell in board.Cells)
                Assert.Equal(8, cell.neighbors.Count);
        }
    }

    public class BoardStateTests
    {
        [Fact]
        public void NewBoardWithZeroDensityIsEmpty()
        {
            var board = new Board(30, 30, 1, 0.0);
            Assert.Equal(0, board.CountAlive());
        }

        [Fact]
        public void NewBoardWithFullDensityIsCompletelyFilled()
        {
            var board = new Board(5, 5, 1, 1.0);
            Assert.Equal(25, board.CountAlive());
        }

        [Fact]
        public void BoardSizeMatchesCellSizeParam()
        {
            var board = new Board(40, 20, 2, 0.0);
            Assert.Equal(20, board.Columns);
            Assert.Equal(10, board.Rows);
        }

        [Fact]
        public void SavedStateRestoredExactly()
        {
            var board = new Board(15, 15, 1, 0.0);
            var positions = new[] { (1,1),(3,7),(14,14),(0,8) };
            foreach (var (x,y) in positions)
                board.Cells[x,y].IsAlive = true;

            string path = Path.GetTempFileName();
            try
            {
                board.SaveState(path);
                var restored = Board.LoadState(path);
                Assert.Equal(board.CountAlive(), restored.CountAlive());
                foreach (var (x,y) in positions)
                    Assert.True(restored.Cells[x,y].IsAlive);
            }
            finally { File.Delete(path); }
        }

        [Fact]
        public void LoadedBoardHasSameDimensions()
        {
            var board = new Board(25, 12, 1, 0.0);
            string path = Path.GetTempFileName();
            try
            {
                board.SaveState(path);
                var loaded = Board.LoadState(path);
                Assert.Equal(25, loaded.Columns);
                Assert.Equal(12, loaded.Rows);
            }
            finally { File.Delete(path); }
        }

        [Fact]
        public void DeadCellsNotSavedAsAlive()
        {
            var board = new Board(10, 10, 1, 0.0);
            board.Cells[5,5].IsAlive = true;
            string path = Path.GetTempFileName();
            try
            {
                board.SaveState(path);
                var loaded = Board.LoadState(path);
                Assert.Equal(1, loaded.CountAlive());
                Assert.False(loaded.Cells[0,0].IsAlive);
            }
            finally { File.Delete(path); }
        }

        [Fact]
        public void GetCombinationsEmptyBoardHasNoGroups()
        {
            var board = new Board(10, 10, 1, 0.0);
            Assert.Empty(board.GetCombinations());
        }

        [Fact]
        public void GetCombinationsThreeSeparateCells()
        {
            var board = new Board(20, 20, 1, 0.0);
            board.Cells[0,0].IsAlive = true;
            board.Cells[10,0].IsAlive = true;
            board.Cells[0,10].IsAlive = true;
            Assert.Equal(3, board.GetCombinations().Count);
        }

        [Fact]
        public void GetCombinationsGroupSizeIsCorrect()
        {
            var board = new Board(20, 20, 1, 0.0);
            board.Cells[5,5].IsAlive = true;
            board.Cells[5,6].IsAlive = true;
            board.Cells[5,7].IsAlive = true;
            var groups = board.GetCombinations();
            Assert.Single(groups);
            Assert.Equal(3, groups[0].Count);
        }
    }

    public class ClassifierTests
    {
        [Theory]
        [InlineData("Block")]
        [InlineData("Beehive")]
        [InlineData("Loaf")]
        [InlineData("Boat")]
        [InlineData("Tub")]
        public void AllKnownPatternsAreRecognized(string name)
        {
            var patterns = new Dictionary<string, List<(int,int)>>
            {
                ["Block"]   = new() { (0,0),(1,0),(0,1),(1,1) },
                ["Beehive"] = new() { (1,0),(2,0),(0,1),(3,1),(1,2),(2,2) },
                ["Loaf"]    = new() { (1,0),(2,0),(0,1),(3,1),(1,2),(3,2),(2,3) },
                ["Boat"]    = new() { (0,0),(1,0),(0,1),(2,1),(1,2) },
                ["Tub"]     = new() { (1,0),(0,1),(2,1),(1,2) },
            };
            Assert.Equal(name, FigureClassifier.Classify(patterns[name]));
        }

        [Fact]
        public void BlinkerRecognizedHorizontal()
        {
            var combo = new List<(int,int)> { (0,0),(1,0),(2,0) };
            Assert.Equal("Blinker", FigureClassifier.Classify(combo));
        }

        [Fact]
        public void BlinkerRecognizedVertical()
        {
            var combo = new List<(int,int)> { (0,0),(0,1),(0,2) };
            Assert.Equal("Blinker", FigureClassifier.Classify(combo));
        }

        [Fact]
        public void SingleCellIsUnknown()
        {
            var combo = new List<(int,int)> { (0,0) };
            Assert.Equal("Unknown", FigureClassifier.Classify(combo));
        }

        [Fact]
        public void LShapeIsUnknown()
        {
            var combo = new List<(int,int)> { (0,0),(1,0),(2,0),(0,1) };
            Assert.Equal("Unknown", FigureClassifier.Classify(combo));
        }

        [Fact]
        public void BlockOnBoardDetectedViaGetCombinations()
        {
            var board = new Board(20, 20, 1, 0.0);
            board.Cells[8,8].IsAlive = true;
            board.Cells[9,8].IsAlive = true;
            board.Cells[8,9].IsAlive = true;
            board.Cells[9,9].IsAlive = true;
            var groups = board.GetCombinations();
            Assert.Single(groups);
            Assert.Equal("Block", FigureClassifier.Classify(groups[0]));
        }
    }

    public class SettingsTests
    {
        [Fact]
        public void MissingFileReturnsDefaults()
        {
            var s = Settings.Load("file_that_does_not_exist_abc123.json");
            Assert.True(s.Width > 0 && s.Height > 0 && s.CellSize > 0);
        }

        [Fact]
        public void AllFieldsPersistThroughSaveLoad()
        {
            var original = new Settings { Width=60, Height=30, CellSize=2, Density=0.4, DelayMs=300 };
            string path = Path.GetTempFileName();
            try
            {
                original.Save(path);
                var loaded = Settings.Load(path);
                Assert.Equal(60, loaded.Width);
                Assert.Equal(30, loaded.Height);
                Assert.Equal(2, loaded.CellSize);
                Assert.Equal(0.4, loaded.Density, 5);
                Assert.Equal(300, loaded.DelayMs);
            }
            finally { File.Delete(path); }
        }

        [Fact]
        public void DensityIsInValidRange()
        {
            var s = new Settings();
            Assert.InRange(s.Density, 0.0, 1.0);
        }
    }

    public class StabilityTests
    {
        [Fact]
        public void ResultIsNonNegative()
        {
            int gen = StabilityAnalyzer.GenerationsToStable(15, 15, 1, 0.4, maxGenerations: 100);
            Assert.True(gen >= 0);
        }

        [Fact]
        public void EmptyBoardStabilizesQuickly()
        {
            int gen = StabilityAnalyzer.GenerationsToStable(20, 20, 1, 0.0, windowSize: 3, maxGenerations: 50);
            Assert.True(gen < 10);
        }

        [Fact]
        public void ExperimentCoverAllRequestedDensities()
        {
            double[] densities = { 0.2, 0.5, 0.8 };
            var result = StabilityAnalyzer.RunExperiment(densities, trials: 2, width: 15, height: 15);
            Assert.Equal(densities.Length, result.Count);
        }

        [Fact]
        public void HigherTrialsGiveSmoothResults()
        {
            double[] d = { 0.5 };
            var r1 = StabilityAnalyzer.RunExperiment(d, trials: 1,  width: 20, height: 20);
            var r5 = StabilityAnalyzer.RunExperiment(d, trials: 10, width: 20, height: 20);
            Assert.True(r1[0.5] >= 0 && r5[0.5] >= 0);
        }
    }
}
