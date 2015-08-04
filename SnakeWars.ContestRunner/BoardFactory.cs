using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SnakeWars.ContestRunner
{
    public class BoardFactory
    {
        public static BoardDefinition EmptyBoard(int width, int height, int turnTimeMilliseconds)
        {
            if (width < 3 || height < 3) throw new ArgumentException($"Board side length must be greater than 2.");

            var turnTime = TimeSpan.FromMilliseconds(turnTimeMilliseconds);

            var offset = Math.Max((int) Math.Sqrt(width*height)/6, 2);
            var margin = offset/2;
            var startingPositions = new List<Point>();
            for (var x = margin; x <= width - margin; x += offset)
            {
                for (var y = margin; y <= height - margin; y += offset)
                {
                    startingPositions.Add(new Point(x, y));
                }
            }
            return new BoardDefinition(width, height, new List<Point>(), startingPositions, turnTime, 5, 100, 20, 5);
        }

        public static BoardDefinition BoardWithOuterWalls(int width, int height, int turnTimeMilliseconds)
        {
            if (width < 3 || height < 3) throw new ArgumentException($"Board side length must be greater than 2.");

            var turnTime = TimeSpan.FromMilliseconds(turnTimeMilliseconds);

            var walls = new List<Point>();
            walls.AddRange(Enumerable.Range(0, width).SelectMany(x => new[] {new Point(x, 0), new Point(x, height - 1)}));
            walls.AddRange(Enumerable.Range(0, height).SelectMany(y => new[] {new Point(0, y), new Point(width - 1, y)}));

            var offset = Math.Max((int) Math.Sqrt((width - 2)*(height - 2))/6, 2);
            var margin = 1 + offset/2;
            var startingPositions = new List<Point>();
            for (var x = margin; x <= width - margin; x += offset)
            {
                for (var y = margin; y <= height - margin; y += offset)
                {
                    startingPositions.Add(new Point(x, y));
                }
            }
            return new BoardDefinition(width, height, walls, startingPositions, turnTime, 5, 100, 20, 5);
        }

        public static Func<BoardDefinition> CreateComplexBoardGenerator(int turnTimeMilliseconds, int repetitionCount)
        {
            var turnTime = TimeSpan.FromMilliseconds(turnTimeMilliseconds);
            var boardDefinitions = new List<BoardDefinition>();

            var boardsFile = "boards.txt";
            var lineIndex = 0;
            using (var reader = new StreamReader(boardsFile))
            {
                while (!reader.EndOfStream)
                {
                    try
                    {
                        lineIndex++;
                        var line = reader.ReadLine().TrimStart();
                        if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;

                        // Board header
                        var match = Regex.Match(line, @"Board\s+([a-zA-Z0-9]+)\s+(.*)");
                        if (!match.Success)
                            throw new InvalidDataException($"Error parsing line: {line}");
                        var boardId = match.Groups[1].Value;
                        var boardName = match.Groups[2].Value.Trim();

                        // Board parameters
                        lineIndex++;
                        line = reader.ReadLine().TrimStart();
                        match = Regex.Match(line, @"(\d+)\s+(\d+)\s+(\d+)\s+(\d+)\s+(\d+)\s+(\d+)");
                        if (!match.Success)
                            throw new InvalidDataException($"Error parsing line: {line}");
                        var width = int.Parse(match.Groups[1].Value);
                        var height = int.Parse(match.Groups[2].Value);
                        var initialSnakeLength = int.Parse(match.Groups[3].Value);
                        var maxSnakeWeight = int.Parse(match.Groups[4].Value);
                        var maxFoodCount = int.Parse(match.Groups[5].Value);
                        var foodSpawnRate = int.Parse(match.Groups[6].Value);

                        var walls = new List<Point>();
                        var startingPositions = new List<Point>();
                        // Board map
                        for (var y = height - 1; y >= 0; y--)
                        {
                            lineIndex++;
                            line = reader.ReadLine().Trim();
                            for (var x = 0; x < Math.Min(line.Length, width); x++)
                            {
                                var c = line[x];
                                switch (c)
                                {
                                    case '#':
                                    case 'X':
                                        walls.Add(new Point(x, y));
                                        break;
                                    case 's':
                                        startingPositions.Add(new Point(x, y));
                                        break;
                                }
                            }
                        }
                        boardDefinitions.Add(new BoardDefinition(width, height, walls, startingPositions, turnTime,
                            initialSnakeLength, maxSnakeWeight, maxFoodCount, foodSpawnRate));
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidDataException($"Found invalid line at {lineIndex} in {boardsFile} file.", ex);
                    }
                }
            }

            {
                var boardIndex = -1;
                var i = -1;
                return () =>
                {
                    i++;
                    if (i%repetitionCount == 0)
                    {
                        boardIndex = (boardIndex + 1)%boardDefinitions.Count;
                    }
                    return boardDefinitions[boardIndex];
                };
            }
        }
    }
}