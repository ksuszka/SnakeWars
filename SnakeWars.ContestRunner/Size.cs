namespace SnakeWars.ContestRunner
{
    public struct Size
    {
        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width { get; }
        public int Height { get; }
        public override string ToString() => $"({Width},{Height})";
    }
}