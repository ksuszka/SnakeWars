using System;
using System.Collections.Generic;

namespace SnakeWars.ContestRunner
{
    public class BoardDefinition
    {
        public BoardDefinition(int width, int height, IReadOnlyList<Point> walls,
            IReadOnlyList<Point> startingPositions, TimeSpan turnTime, int maxSnakeWeight)
        {
            BoardSize = new Size(width, height);
            Walls = walls;
            StartingPositions = startingPositions;
            TurnTime = turnTime;
            MaxSnakeWeight = maxSnakeWeight;
        }

        public IReadOnlyList<Point> Walls { get; }
        public IReadOnlyList<Point> StartingPositions { get; }
        public Size BoardSize { get; }
        public TimeSpan TurnTime { get; }
        public int MaxSnakeWeight { get; }
    }
}