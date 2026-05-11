using System.IO;
using Life;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LifeTests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void Cell_DefaultState_IsDead()
        {
            var cell = new Cell();
            Assert.IsFalse(cell.Alive);
        }

        [TestMethod]
        public void Cell_CreatedAlive_IsAlive()
        {
            var cell = new Cell(true);
            Assert.IsTrue(cell.Alive);
        }

        [TestMethod]
        public void Cell_Clone_ReturnsCopy()
        {
            var cell  = new Cell(true);
            var clone = cell.Clone();
            Assert.AreEqual(cell.Alive, clone.Alive);
            Assert.AreNotSame(cell, clone);
        }

        [TestMethod]
        public void Cell_ToString_Dead_ReturnsDot()
        {
            var cell = new Cell(false);
            Assert.AreEqual(".", cell.ToString());
        }

        [TestMethod]
        public void Cell_ToString_Alive_ReturnsHash()
        {
            var cell = new Cell(true);
            Assert.AreEqual("#", cell.ToString());
        }

        [TestMethod]
        public void Board_Dimensions_CorrectAfterCreate()
        {
            var board = new Board(10, 5);
            Assert.AreEqual(10, board.Width);
            Assert.AreEqual(5,  board.Height);
        }

        [TestMethod]
        public void Board_Randomize_FillsExpectedDensity()
        {
            var board = new Board(100, 100);
            board.Randomize(0.5, seed: 42);
            int alive = board.CountAlive();
            Assert.IsTrue(alive > 3000 && alive < 7000);
        }

        [TestMethod]
        public void Board_Randomize_ZeroDensity_AllDead()
        {
            var board = new Board(20, 20);
            board.Randomize(0.0, seed: 1);
            Assert.AreEqual(0, board.CountAlive());
        }

        [TestMethod]
        public void Board_Randomize_FullDensity_AllAlive()
        {
            var board = new Board(20, 20);
            board.Randomize(1.0, seed: 1);
            Assert.AreEqual(400, board.CountAlive());
        }

        [TestMethod]
        public void Board_Block_IsStable()
        {
            var board = new Board(6, 6);
            board[1, 1].Alive = true;
            board[1, 2].Alive = true;
            board[2, 1].Alive = true;
            board[2, 2].Alive = true;
            int before = board.CountAlive();
            board.Step();
            Assert.AreEqual(before, board.CountAlive());
        }

        [TestMethod]
        public void Board_Blinker_OscillatesPeriod2()
        {
            var board = new Board(7, 7);
            board[3, 2].Alive = true;
            board[3, 3].Alive = true;
            board[3, 4].Alive = true;
            board.Step();
            Assert.IsTrue(board[2, 3].Alive);
            Assert.IsTrue(board[3, 3].Alive);
            Assert.IsTrue(board[4, 3].Alive);
            board.Step();
            Assert.IsTrue(board[3, 2].Alive);
            Assert.IsTrue(board[3, 3].Alive);
            Assert.IsTrue(board[3, 4].Alive);
        }

        [TestMethod]
        public void Board_SingleCell_DiesNextStep()
        {
            var board = new Board(5, 5);
            board[2, 2].Alive = true;
            board.Step();
            Assert.AreEqual(0, board.CountAlive());
        }

        [TestMethod]
        public void Board_CountNeighbours_CornerUsesTorus()
        {
            var board = new Board(5, 5);
            board[0, 1].Alive = true;
            board[1, 0].Alive = true;
            board[1, 1].Alive = true;
            int n = board.CountNeighbours(0, 0);
            Assert.AreEqual(3, n);
        }

        [TestMethod]
        public void Board_CountClusters_TwoSeparateGroups()
        {
            var board = new Board(10, 10);
            board[1, 1].Alive = true;
            board[1, 2].Alive = true;
            board[7, 7].Alive = true;
            board[7, 8].Alive = true;
            Assert.AreEqual(2, board.CountClusters());
        }

        [TestMethod]
        public void Board_SaveLoad_PreservesState()
        {
            var board = new Board(8, 8);
            board.Randomize(0.4, seed: 99);
            string path = Path.GetTempFileName();
            board.SaveToFile(path);
            var loaded = Board.LoadFromFile(path);
            Assert.AreEqual(board.CountAlive(), loaded.CountAlive());
            File.Delete(path);
        }

        [TestMethod]
        public void SimulationConfig_DefaultValues_AreReasonable()
        {
            var cfg = new SimulationConfig();
            Assert.IsTrue(cfg.Width > 0);
            Assert.IsTrue(cfg.Height > 0);
            Assert.IsTrue(cfg.Density > 0 && cfg.Density < 1);
        }

        [TestMethod]
        public void SimulationConfig_SaveLoad_RoundTrip()
        {
            string path = Path.GetTempFileName();
            var cfg = new SimulationConfig { Width = 55, Height = 30, Density = 0.42 };
            cfg.Save(path);
            var loaded = SimulationConfig.Load(path);
            Assert.AreEqual(55,   loaded.Width);
            Assert.AreEqual(30,   loaded.Height);
            Assert.AreEqual(0.42, loaded.Density, 1e-9);
            File.Delete(path);
        }

        [TestMethod]
        public void GameController_RunUntilStable_ReturnsPositiveGen()
        {
            var cfg  = new SimulationConfig { Width = 30, Height = 20, Density = 0.3, MaxSteps = 300, StableWindow = 5 };
            var ctrl = new GameController(cfg);
            ctrl.StartRandom(seed: 7);
            int gen = ctrl.RunUntilStable();
            Assert.IsTrue(gen > 0);
        }

        [TestMethod]
        public void GameController_Tick_IncrementsGeneration()
        {
            var cfg  = new SimulationConfig();
            var ctrl = new GameController(cfg);
            ctrl.StartRandom(seed: 1);
            ctrl.Tick();
            ctrl.Tick();
            Assert.AreEqual(2, ctrl.Generation);
        }

        [TestMethod]
        public void Board_ThreeCellsInRow_SpawnsCellAboveMiddle()
        {
            var board = new Board(5, 5);
            board[2, 1].Alive = true;
            board[2, 2].Alive = true;
            board[2, 3].Alive = true;
            board.Step();
            Assert.IsTrue(board[1, 2].Alive);
        }

        [TestMethod]
        public void Board_EmptyBoard_StaysEmpty()
        {
            var board = new Board(10, 10);
            board.Step();
            Assert.AreEqual(0, board.CountAlive());
        }

        [TestMethod]
        public void Board_CountClusters_EmptyBoard_Zero()
        {
            var board = new Board(10, 10);
            Assert.AreEqual(0, board.CountClusters());
        }
        [TestMethod]
        public void PatternClassifier_Block_Recognized()
        {
            var board = new Board(6, 6);
            board[2, 2].Alive = true;
            board[2, 3].Alive = true;
            board[3, 2].Alive = true;
            board[3, 3].Alive = true;
            var result = PatternClassifier.Classify(board);
            Assert.AreEqual(1, result["Block"]);
        }

        [TestMethod]
        public void PatternClassifier_BlinkerH_Recognized()
        {
            var board = new Board(7, 7);
            board[3, 2].Alive = true;
            board[3, 3].Alive = true;
            board[3, 4].Alive = true;
            var result = PatternClassifier.Classify(board);
            Assert.AreEqual(1, result["Blinker-H"]);
        }

        [TestMethod]
        public void PatternClassifier_BlinkerV_Recognized()
        {
            var board = new Board(7, 7);
            board[2, 3].Alive = true;
            board[3, 3].Alive = true;
            board[4, 3].Alive = true;
            var result = PatternClassifier.Classify(board);
            Assert.AreEqual(1, result["Blinker-V"]);
        }

        [TestMethod]
        public void PatternClassifier_TwoBlocks_CountsTwo()
        {
            var board = new Board(10, 10);
            board[1, 1].Alive = true; board[1, 2].Alive = true;
            board[2, 1].Alive = true; board[2, 2].Alive = true;
            board[6, 6].Alive = true; board[6, 7].Alive = true;
            board[7, 6].Alive = true; board[7, 7].Alive = true;
            var result = PatternClassifier.Classify(board);
            Assert.AreEqual(2, result["Block"]);
        }

        [TestMethod]
        public void PatternClassifier_UnknownShape_CountedAsUnknown()
        {
            var board = new Board(10, 10);
            board[1, 1].Alive = true;
            board[2, 3].Alive = true;
            board[4, 2].Alive = true;
            var result = PatternClassifier.Classify(board);
            Assert.IsTrue(result["Unknown"] > 0);
        }
    }
}
