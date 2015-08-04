using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SnakeWars.ContestRunner
{
    internal class Game
    {
        public delegate void GameStateViewHandler(GameState gameState);

        private readonly GameState _gameState;
        private readonly GameStateViewHandler _gameStateWatcher;
        private readonly IReadOnlyList<IPlayer> _players;

        public Game(BoardDefinition boardDefinition, IReadOnlyList<IPlayer> players,
            GameStateViewHandler gameStateWatcher)
        {
            if (players.Count > boardDefinition.StartingPositions.Count)
                throw new ArgumentException(
                    $"Board does not have enough starting positions (found {boardDefinition.StartingPositions.Count}, needed {players.Count}).");
            _players = players;
            _gameStateWatcher = gameStateWatcher;
            _gameState = GameState.FromBoardDefinition(boardDefinition, players);
        }

        public IDictionary<string, int> Scores => _gameState.Snakes.ToDictionary(s => s.Id, s => s.Score);

//        private Func<bool> _shouldGameContinue => () => _gameState.IsAnySnakeAlive;
        private Func<bool> _shouldGameContinue => () =>
        {
            // End game when there is only one snake left and it has most points.
            var aliveSnakes = _gameState.Snakes.Where(s => s.IsAlive);
            if (aliveSnakes.Count() < 1) return false;
            if (aliveSnakes.Count() > 1) return true;
            var lastSnake = aliveSnakes.First();
            return _gameState.Snakes.Count(s => s.Score >= lastSnake.Score) > 1;
        };

        public void Run()
        {
            while (_shouldGameContinue())
            {
                Console.WriteLine($"Turn {_gameState.Turn}");

                // 1. report board state to players and viewers
                _gameStateWatcher(_gameState);
                _players.ToList().ForEach(player => player.NewTurn(_gameState));

                // 2. wait for move request
                Thread.Sleep(_gameState.TurnTime);
                var moves = _players.ToDictionary(player => player.Id, player => player.GetNextMove());

                // 3. perform moves
                _gameState.ApplyMoves(moves);
            }

            // Report state last time
            _gameStateWatcher(_gameState);
            _players.ToList().ForEach(player => player.NewTurn(_gameState));
        }
    }
}