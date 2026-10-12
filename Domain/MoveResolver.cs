namespace Domain
{
    public class MoveResolver
    {
        public static (int x, int y) Resolve(Board board, IReadOnlyList<Bot> bots, Bot bot, Direction direction)
        {
            var positions = new (int X, int Y)[bots.Count];
            var index = -1;
            for (int i = 0; i < bots.Count; i++)
            {
                positions[i] = (bots[i].X, bots[i].Y);
                if (bots[i].Id == bot.Id) index = i;
            }
            return Resolve(board, positions, index, direction);
        }

        public static (int x, int y) Resolve(Board board, (int X, int Y)[] positions, int botIndex, Direction direction)
        {
            int x = positions[botIndex].X;
            int y = positions[botIndex].Y;

            while (true)
            {
                if (board.HasWall(x, y, direction))
                    break;

                var (nx, ny) = Step(x, y, direction);

                if (!board.InBounds(nx, ny))
                    break;

                bool blocked = false;
                for (int i = 0; i < positions.Length; i++)
                {
                    if (i != botIndex && positions[i].X == nx && positions[i].Y == ny)
                    {
                        blocked = true;
                        break;
                    }
                }
                if (blocked)
                    break;

                x = nx;
                y = ny;
            }

            return (x, y);
        }

        private static (int x, int y) Step(int x, int y, Direction direction) => direction switch
        {
            Direction.North => (x, y - 1),
            Direction.East => (x + 1, y),
            Direction.South => (x, y + 1),
            Direction.West => (x - 1, y),
            _ => throw new ArgumentOutOfRangeException(nameof(direction))
        };
    }
}
