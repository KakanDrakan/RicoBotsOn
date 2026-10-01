using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RicoBotsOn.Tests
{
    public class MoveResolverTests
    {
        [Fact]
        public void Resolve_SlidesToBoardEdge_WhenNothingBlocksIt()
        {
            var board = TestHelpers.FiveByFiveNoWalls();
            var robot = new Bot("b1", 0, 0);

            var (x, y) = MoveResolver.Resolve(board, new[] { robot }, robot, Direction.East);

            Assert.Equal((4, 0), (x, y));
        }

        [Fact]
        public void Resolve_StopsAtWall()
        {
            var board = TestHelpers.FiveByFiveNoWalls();
            board.SetWall(2, 0, Direction.East);
            var robot = new Bot("b1", 0, 0);

            var (x, y) = MoveResolver.Resolve(board, new[] { robot }, robot, Direction.East);

            Assert.Equal((2, 0), (x, y));
        }

        [Fact]
        public void Resolve_StopsAdjacentToAnotherRobot()
        {
            var board = TestHelpers.FiveByFiveNoWalls();
            var mover = new Bot("b1", 0, 0);
            var blocker = new Bot("b2", 3, 0);

            var (x, y) = MoveResolver.Resolve(board, new[] { mover, blocker }, mover, Direction.East);

            Assert.Equal((2, 0), (x, y));
        }

        [Fact]
        public void Resolve_DoesNotMove_WhenAlreadyAgainstTheEdge()
        {
            var board = TestHelpers.FiveByFiveNoWalls();
            var robot = new Bot("b1", 0, 0);

            var (x, y) = MoveResolver.Resolve(board, new[] { robot }, robot, Direction.West);

            Assert.Equal((0, 0), (x, y));
        }

        [Fact]
        public void Resolve_DoesNotTreatItsOwnCell_AsAnObstacle()
        {
            var board = TestHelpers.FiveByFiveNoWalls();
            var robot = new Bot("b1", 2, 2);

            var (x, y) = MoveResolver.Resolve(board, [robot], robot, Direction.North);

            Assert.Equal((2, 0), (x, y));
        }
    }
}
