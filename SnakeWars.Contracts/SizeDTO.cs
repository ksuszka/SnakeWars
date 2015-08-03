namespace SnakeWars.Contracts
{
    public struct SizeDTO
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public override string ToString() => $"({Width},{Height})";
    }
}