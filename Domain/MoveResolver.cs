namespace Domain
{
    public class MoveResolver
    {
        public static (int x, int y) Resolve(Board board, IReadOnlyList<Bot> bots, Bot bot, Direction direction)
        {
            int x = bot.X;
            int y = bot.Y;

            while (true)
            {
                if (board.HasWall(x, y, direction))
                    break;

                var (nx, ny) = Step(x, y, direction);

                if (!board.InBounds(nx, ny))
                    break;

                if (bots.Any(r => r.Id != bot.Id && r.X == nx && r.Y == ny))
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
