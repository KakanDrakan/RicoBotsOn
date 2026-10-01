namespace Domain
{
    public class Board
    {
        public int Width { get; }
        public int Height { get; }
        private readonly int[,] _walls;

        public Board(int width, int height)
        {
            Width = width;
            Height = height;
            _walls = new int[width, height];
        }

        public bool HasWall(int x, int y, Direction direction)
        {
            return (_walls[x, y] & 1 << (int)direction) != 0;
        }

        // Sets the wall on both sides of the edge, so neighboring cells can never disagree.
        public void SetWall(int x, int y, Direction direction)
        {
            _walls[x, y] |= 1 << (int)direction;

            var (nx, ny, opposite) = GetNeighbor(x, y, direction);
            if (InBounds(nx, ny))
            {
                _walls[nx, ny] |= 1 << (int)opposite;
            }
        }

        public bool InBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

        private readonly List<(int X, int Y)> _targets = new();
        public IReadOnlyList<(int X, int Y)> Targets => _targets;

        public void AddTarget(int x, int y)
        {
            if (!InBounds(x, y))
                throw new ArgumentOutOfRangeException(nameof(x), "Target cell is outside the board.");

            _targets.Add((x, y));
        }

        private static (int x, int y, Direction opposite) GetNeighbor(int x, int y, Direction direction)
        {
            return direction switch
            {
                Direction.North => (x, y - 1, Direction.South),
                Direction.East => (x + 1, y, Direction.West),
                Direction.South => (x, y + 1, Direction.North),
                Direction.West => (x - 1, y, Direction.East),
                _ => throw new ArgumentOutOfRangeException(nameof(direction))
            };
        }
    }
}
