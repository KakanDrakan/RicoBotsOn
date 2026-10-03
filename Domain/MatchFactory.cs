using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public static class MatchFactory
    {
        public static Match CreateDefault(Random random)
        {
            var board = new Board(16, 16);

            board.SetWall(4, 0, Direction.East);
            board.SetWall(10, 0, Direction.East);
            board.SetWall(2, 1, Direction.East);
            board.SetWall(8, 1, Direction.East);
            board.SetWall(14, 2, Direction.East);
            board.SetWall(0, 3, Direction.East);
            board.SetWall(5, 4, Direction.East);
            board.SetWall(10, 4, Direction.East);
            board.SetWall(5, 6, Direction.East);
            board.SetWall(11, 6, Direction.East);
            board.SetWall(3, 7, Direction.East);
            board.SetWall(6, 7, Direction.East);
            board.SetWall(8, 7, Direction.East);
            board.SetWall(6, 8, Direction.East);
            board.SetWall(8, 8, Direction.East);
            board.SetWall(3, 9, Direction.East);
            board.SetWall(14, 9, Direction.East);
            board.SetWall(8, 10, Direction.East);
            board.SetWall(0, 11, Direction.East);
            board.SetWall(12, 11, Direction.East);
            board.SetWall(6, 12, Direction.East);
            board.SetWall(9, 13, Direction.East);
            board.SetWall(1, 14, Direction.East);
            board.SetWall(5, 15, Direction.East);
            board.SetWall(11, 15, Direction.East);

            board.SetWall(2, 2, Direction.North);
            board.SetWall(9, 2, Direction.North);
            board.SetWall(14, 2, Direction.North);
            board.SetWall(1, 4, Direction.North);
            board.SetWall(6, 4, Direction.North);
            board.SetWall(0, 5, Direction.North);
            board.SetWall(10, 5, Direction.North);
            board.SetWall(15, 5, Direction.North);
            board.SetWall(5, 6, Direction.North);
            board.SetWall(12, 6, Direction.North);
            board.SetWall(7, 7, Direction.North);
            board.SetWall(8, 7, Direction.North);
            board.SetWall(3, 8, Direction.North);
            board.SetWall(3, 9, Direction.North);
            board.SetWall(7, 9, Direction.North);
            board.SetWall(8, 9, Direction.North);
            board.SetWall(8, 10, Direction.North);
            board.SetWall(14, 10, Direction.North);
            board.SetWall(15, 11, Direction.North);
            board.SetWall(1, 12, Direction.North);
            board.SetWall(13, 12, Direction.North);
            board.SetWall(6, 13, Direction.North);
            board.SetWall(10, 13, Direction.North);
            board.SetWall(0, 14, Direction.North);
            board.SetWall(2, 14, Direction.North);

            board.AddTarget(2, 1);
            board.AddTarget(9, 1);
            board.AddTarget(14, 2);
            board.AddTarget(1, 3);
            board.AddTarget(6, 4);
            board.AddTarget(10, 4);
            board.AddTarget(5, 6);
            board.AddTarget(12, 6);
            board.AddTarget(3, 7);
            board.AddTarget(3, 9);
            board.AddTarget(14, 9);
            board.AddTarget(8, 10);
            board.AddTarget(1, 11);
            board.AddTarget(6, 12);
            board.AddTarget(10, 13);
            board.AddTarget(2, 14);
            board.AddTarget(13, 11);

            var bots = new List<Bot>
            {
                new Bot("bot1", 0, 0),
                new Bot("bot2", 15, 0),
                new Bot("bot3", 0, 15),
                new Bot("bot4", 15, 15),
                new Bot("bot5", 9, 9)
            };

            var match = new Match(board, bots);
            match.SelectNewTarget(random);
            return match;
        }
    }
}
