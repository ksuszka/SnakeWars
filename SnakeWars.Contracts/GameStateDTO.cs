using System;
using System.Collections.Generic;

namespace SnakeWars.Contracts
{
    public class GameStateDTO : IGameState
    {
        public int Turn { get; set; }
        public IEnumerable<ISnake> Snakes { get; set; }
        public IEnumerable<Point> Walls { get; set; }
        public IEnumerable<Point> Food { get; set; }
        public Size BoardSize { get; set; }
        public TimeSpan TurnTime { get; set; }
    }
}