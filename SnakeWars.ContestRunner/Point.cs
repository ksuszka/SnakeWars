namespace SnakeWars.ContestRunner
{
    public struct Point
    {
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }

        public Point Offset(int dx, int dy)
        {
            return new Point(X + dx, Y + dy);
        }

        public override string ToString() => $"({X},{Y})";
    }
}