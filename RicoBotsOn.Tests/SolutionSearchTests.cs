using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace RicoBotsOn.Tests
{
    public class SolutionSearchTests
    {
        [Fact]
        public void CanReach_RespectsTheMoveLimit()
        {
            var board = TestHelpers.FiveByFiveNoWalls();
            var positions = new[] { (0, 0) };  // (4, 4) needs exactly 2 moves

            Assert.False(SolutionSearch.CanReach(board, positions, 0, (4, 4), 1));
            Assert.True(SolutionSearch.CanReach(board, positions, 0, (4, 4), 2));
        }

        [Fact]
        public void CanReach_CountsMovesByOtherBots()
        {
            var board = TestHelpers.FiveByFiveNoWalls();
            // b1 slides north to (4, 0), then b0 slides east and stops at (3, 0)
            var positions = new[] { (0, 0), (4, 4) };

            Assert.False(SolutionSearch.CanReach(board, positions, 0, (3, 0), 1));
            Assert.True(SolutionSearch.CanReach(board, positions, 0, (3, 0), 2));
        }

        [Fact]
        public void CanReach_IgnoresAnotherBotLandingOnTheTarget()
        {
            var board = TestHelpers.FiveByFiveNoWalls();
            // b1 reaches (4, 0) in one move, but b0 is the target bot and needs two
            var positions = new[] { (0, 2), (4, 4) };

            Assert.False(SolutionSearch.CanReach(board, positions, 0, (4, 0), 1));
            Assert.True(SolutionSearch.CanReach(board, positions, 0, (4, 0), 2));
        }

        [Fact]
        public void CanReach_LeavesThePositionsArrayUnchanged()
        {
            var board = TestHelpers.FiveByFiveNoWalls();
            var positions = new[] { (0, 0), (4, 4) };
            var before = positions.ToArray();

            SolutionSearch.CanReach(board, positions, 0, (2, 2), 3);

            Assert.Equal(before, positions);
        }
    }
}
