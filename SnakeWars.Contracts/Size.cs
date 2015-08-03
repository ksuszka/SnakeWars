namespace SnakeWars.Contracts
{
    public struct Size
    {
        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width { get; set; }
        public int Height { get; set; }
        public override string ToString() => $"({Width},{Height})";
    }
}