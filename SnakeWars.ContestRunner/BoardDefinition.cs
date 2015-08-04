using System;
using System.Collections.Generic;

namespace SnakeWars.ContestRunner
{
    public struct BoardDefinition
    {
        public BoardDefinition(int width, int height, IReadOnlyList<Point> walls,
            IReadOnlyList<Point> startingPositions, TimeSpan turnTime, int initialSnakeLength, int maxSnakeWeight, int maxFoodCount, int foodSpawnRate)
        {
            BoardSize = new Size(width, height);
            Walls = walls;
            StartingPositions = startingPositions;
            TurnTime = turnTime;
            InitialSnakeLength = initialSnakeLength;
            MaxSnakeWeight = maxSnakeWeight;
            MaxFoodCount = maxFoodCount;
            FoodSpawnRate = foodSpawnRate;
        }

        public IReadOnlyList<Point> Walls { get; set; }
        public IReadOnlyList<Point> StartingPositions { get; set; }
        public Size BoardSize { get; set; }
        public TimeSpan TurnTime { get; set; }
        public int MaxSnakeWeight { get; set; }
        public int InitialSnakeLength { get; set; }
        public int MaxFoodCount { get; set; }
        public int FoodSpawnRate { get; set; }
    }
}