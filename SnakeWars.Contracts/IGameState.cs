using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SnakeWars.Contracts
{
    public interface IGameState
    {
        int Turn { get; }

        [JsonConverter(typeof (ConcreteConverter<IEnumerable<SnakeDTO>>))]
        IEnumerable<ISnake> Snakes { get; }

        IEnumerable<Point> Walls { get; }
        IEnumerable<Point> Food { get; }
        Size BoardSize { get; }
        TimeSpan TurnTime { get; }
    }
}