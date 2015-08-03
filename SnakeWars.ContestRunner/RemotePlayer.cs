using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SnakeWars.ContestRunner
{
    public class RemotePlayer : IPlayer
    {
        private static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private MoveDisposition _moveDisposition = MoveDisposition.GoStraight;
        public string Name { get; set; }
        public int TotalScore { get; set; }
        public string LoginId { get; set; }
        public string Id { get; set; }

        public void NewTurn(GameState gameState)
        {
            _moveDisposition = MoveDisposition.GoStraight;

            GameStateUpdated?.Invoke(JsonConvert.SerializeObject(Mapping.CreateGameStateDTO(gameState),
                jsonSerializerSettings));
        }

        public MoveDisposition GetNextMove() => _moveDisposition;
        public event Action<string> GameStateUpdated;

        public void SetNextMove(MoveDisposition moveDisposition)
        {
            // Called from various threads by PlayersConnector.
            _moveDisposition = moveDisposition;
        }
    }
}