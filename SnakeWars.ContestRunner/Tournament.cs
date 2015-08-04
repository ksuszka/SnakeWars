using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SnakeWars.ContestRunner
{
    internal class Tournament
    {
        private static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private static readonly string PlayersDefaultFileName = "players.json";
        private static readonly string PlayersTournamentFileName = "players_tournament.json";
        private readonly List<RemotePlayer> _players;
        private int _gameNumber;

        public Tournament()
        {
            _gameNumber = 0;
            _players = LoadPlayers();
        }

        public IEnumerable<RemotePlayer> Players => _players;
        public TimeSpan GameBreakTime { get; set; } = TimeSpan.FromSeconds(2);
        public event Action<string> StateUpdated;

        private void ReportGameState(GameState gameState)
        {
            var data = Mapping.CreateTournamentStateDTO(_gameNumber, gameState, _players);
            var serializedData = JsonConvert.SerializeObject(data, jsonSerializerSettings);
            StateUpdated?.Invoke(serializedData);
        }

        public void Run(Func<bool> stopPredicate)
        {
            Console.WriteLine("Starting tournament...");
            while (!stopPredicate())
            {
                _gameNumber++;
                Console.WriteLine($"Game {_gameNumber}");
                var game = new Game(BoardFactory.EmptyBoard(30, 20), _players, ReportGameState);
                game.Run();
                UpdatePlayersScores(game.Scores);
                Thread.Sleep(GameBreakTime);
            }
            Console.WriteLine("Tournament stopped.");
        }

        private void UpdatePlayersScores(IDictionary<string, int> scores)
        {
            var playerMap = _players.ToDictionary(p => p.Id);
            // Assign tournament points based on order of scored points
            var sortedScores = scores.ToList().GroupBy(k => k.Value, k => k.Key).OrderByDescending(g => g.Key);
            var points = scores.Count();
            foreach (var score in sortedScores)
            {
                score.ToList().ForEach(playerId => playerMap[playerId].TotalScore += points);
                points -= score.Count();
            }
            SavePlayers(_players);
        }

        private static void SavePlayers(IEnumerable<RemotePlayer> players)
        {
            var serializedData = JsonConvert.SerializeObject(players, Formatting.Indented, jsonSerializerSettings);
            File.WriteAllText(PlayersTournamentFileName, serializedData);
        }

        private static List<RemotePlayer> LoadPlayers()
        {
            var playersFileName = File.Exists(PlayersTournamentFileName)
                ? PlayersTournamentFileName
                : PlayersDefaultFileName;
            Console.WriteLine($"Loading players list from {playersFileName}...");
            var players = JsonConvert.DeserializeObject<List<RemotePlayer>>(File.ReadAllText(playersFileName));
            var index = 0;
            var idGenerator = new Func<int, string>(i =>
            {
                if (i < 'Z' - 'A') return $"{(char) ('A' + i)}";
                return $"{i + 1}";
            });
            players.ForEach(player => player.Id = idGenerator(index++));
            Console.WriteLine($"Loaded {players.Count} players: {string.Join(", ", players)}");
            return players;
        }
    }
}