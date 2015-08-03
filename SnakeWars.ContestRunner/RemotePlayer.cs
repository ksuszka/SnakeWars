using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SnakeWars.Contracts;

namespace SnakeWars.ContestRunner
{
    public class RemotePlayer : IPlayer
    {
        private static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public string Id { get; set; }

        public string Name { get; set; }

        public int TotalScore { get; set; }

        public string LoginId { get; set; }

        public event Action<string> GameStateUpdated;
        public void NewTurn(GameState gameState)
        {
            _moveDisposition = MoveDisposition.GoStraight;
            GameStateUpdated?.Invoke(JsonConvert.SerializeObject(gameState));
        }

        public MoveDisposition GetNextMove() => _moveDisposition;

        private MoveDisposition _moveDisposition = MoveDisposition.GoStraight;
        public void SetNextMove(MoveDisposition moveDisposition)
        {
            // Called from various threads by PlayersConnector.
            _moveDisposition = moveDisposition;
        }
    }
}