using System.Collections.Generic;

namespace SnakeWars.Contracts
{
    public interface ISnake
    {
        IEnumerable<Point> Cells { get; }
        string Id { get; }
        Point Head { get; }
        bool IsAlive { get; }
        int Score { get; }
        int Weight { get; }
        int MaxWeight { get; }
    }
}