using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SnakeTest;
using SnakeWars.Contracts;

namespace SnakeWars.ContestRunner
{
    public class GameState : IGameState
    {
        private const int InitialLength = 5;
        public static Random rng = new Random();
        public List<Snake> Snakes { get; private set; }
        public bool IsAnySnakeAlive => Snakes.Any(snake => snake.IsAlive);
        private ISet<Point> FreeCells { get; set; }
        public ISet<Point> Walls { get; private set; }
        public ISet<Point> Food { get; } = new HashSet<Point>();
        public int Turn { get; private set; }
        public Size BoardSize { get; private set; }
        public TimeSpan TurnTime { get; set; }
        IEnumerable<ISnake> IGameState.Snakes => Snakes;
        IEnumerable<Point> IGameState.Walls => Walls;
        IEnumerable<Point> IGameState.Food => Food;

        public static GameState FromBoardDefinition(BoardDefinition board, IEnumerable<IPlayerPublicInfo> players)
        {
            Trace.Assert(board.StartingPositions.Count >= players.Count());
            var startingPositions = board.StartingPositions.ToList();
            startingPositions.Shuffle();
            var snakes = players
                .Zip(startingPositions,
                    (player, position) =>
                        new Snake(player.Id, InitialLength, position, Snake.Directions[rng.Next(Snake.Directions.Count)],
                            board.MaxSnakeWeight,
                            board.BoardSize)).ToList();
            var gs = new GameState
            {
                BoardSize = board.BoardSize,
                Walls = new HashSet<Point>(board.Walls),
                Snakes = snakes,
                TurnTime = board.TurnTime
            };

            var allCells =
                from x in Enumerable.Range(0, board.BoardSize.Width)
                from y in Enumerable.Range(0, board.BoardSize.Height)
                select new Point(x, y);

            gs.FreeCells = allCells.ToSet().Except(gs.Walls).ToSet();

            return gs;
        }

        public void ApplyMoves(IDictionary<string, MoveDisposition> moves)
        {
            Turn++;

            // Move snakes
            Snakes.ForEach(snake => snake.UpdateDirection(moves[snake.Id]));
            Snakes.ForEach(snake => snake.Move());

            // Check snakes' collisions
            var obstacles = Walls.ToList()
                .Concat(Snakes.SelectMany(s => s.Cells))
                .ToLookup(o => o);
            Snakes.ForEach(snake =>
            {
                if (obstacles[snake.Head].Count() > 1)
                {
                    snake.Kill();
                }
            });

            // Eat food
            Snakes.ForEach(snake =>
            {
                if (Food.Contains(snake.Head))
                {
                    Food.Remove(snake.Head);
                    snake.Grow();
                }
            });

            // Generate new food
            if (Turn%5 == 0)
            {
                var emptyCells = FreeCells.Except(obstacles.Select(k => k.Key).Concat(Food)).ToList();
                Food.Add(emptyCells[rng.Next(0, emptyCells.Count)]);
            }
        }

        public void PrintBoard()
        {
            var map = new Dictionary<Point, char>();
            Walls.ToList().ForEach(p => map[p] = 'X');
            Food.ToList().ForEach(p => map[p] = '@');
            for (var i = 0; i < Snakes.Count; i++)
            {
                var snake = Snakes[i];
                var cells = snake.Cells.ToList();
                map[cells.Last()] = (char) ('A' + i);
                cells.Take(cells.Count() - 1).ToList().ForEach(p => map[p] = (char) ('a' + i));
            }
            for (var y = 0; y < BoardSize.Height; y++)
            {
                for (var x = 0; x < BoardSize.Width; x++)
                {
                    char symbol;
                    if (!map.TryGetValue(new Point(x, y), out symbol))
                        symbol = '.';
                    Console.Write(symbol);
                }
                Console.WriteLine();
            }
            Snakes.ForEach(
                snake =>
                    Console.WriteLine($"{snake.Id} {(snake.IsAlive ? "ALIVE" : "DEAD ")} {snake.Weight} {snake.Score}"));
        }
    }
}