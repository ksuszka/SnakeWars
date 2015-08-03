using System;
using System.Collections.Generic;
using System.Linq;
using SnakeWars.Contracts;

namespace SnakeWars.ContestRunner
{
    public class BoardFactory
    {
        public static BoardDefinition EmptyBoard(int width, int height)
        {
            if (width < 3 || height < 3) throw new ArgumentException($"Board side length must be greater than 2.");

            var turnTime = TimeSpan.FromMilliseconds(200);

            var offset = Math.Max((int)Math.Sqrt(width*height) / 6, 2);
            var margin = offset/2;
            var startingPositions = new List<Point>();
            for (var x = margin; x <= width - margin; x += offset)
            {
                for (var y = margin; y <= height - margin; y += offset)
                {
                    startingPositions.Add(new Point(x, y));
                }
            }
            return new BoardDefinition(width, height, new List<Point>(), startingPositions, turnTime, 100);
        }


        public static BoardDefinition BoardWithOuterWalls(int width, int height)
        {
            if (width < 3 || height < 3) throw new ArgumentException($"Board side length must be greater than 2.");

            var turnTime = TimeSpan.FromMilliseconds(200);

            var walls = new List<Point>();
            walls.AddRange(Enumerable.Range(0, width).SelectMany(x => new [] {new Point(x, 0), new Point(x, height - 1) }));
            walls.AddRange(Enumerable.Range(0, height).SelectMany(y => new[] { new Point(0, y), new Point(width - 1, y) }));

            var offset = Math.Max((int)Math.Sqrt((width-2) * (height-2)) / 6, 2);
            var margin = 1 + offset / 2;
            var startingPositions = new List<Point>();
            for (var x = margin; x <= width - margin; x += offset)
            {
                for (var y = margin; y <= height - margin; y += offset)
                {
                    startingPositions.Add(new Point(x, y));
                }
            }
            return new BoardDefinition(width, height, walls, startingPositions, turnTime, 100);
        }
    }
}