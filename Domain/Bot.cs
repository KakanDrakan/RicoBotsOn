namespace Domain
{
    public class Bot
    {
        public string Id { get; }
        public int X { get; set; }
        public int Y { get; set; }

        public Bot(string id, int x, int y)
        {
            Id = id;
            X = x;
            Y = y;
        }
    }
}
