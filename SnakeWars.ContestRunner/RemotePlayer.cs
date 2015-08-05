using System;
using System.Threading;
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

        private DispositionRequest _dispositionRequest;
        private DateTime _lastTurnStartTime = DateTime.UtcNow;
        public string Name { get; set; }
        public int TotalScore { get; set; }
        public string LoginId { get; set; }

        public int? LastMoveTimeMilliseconds
        {
            get
            {
                var request = _dispositionRequest;
                Thread.MemoryBarrier();
                if (request == null) return null;
                return (request.Timestamp - _lastTurnStartTime).Milliseconds;
            }
        }

        public string Id { get; set; }

        public void NewTurn(GameState gameState)
        {
            _lastTurnStartTime = DateTime.UtcNow;
            _dispositionRequest = null;
            GameStateUpdated?.Invoke(JsonConvert.SerializeObject(Mapping.CreateGameStateDTO(gameState),
                jsonSerializerSettings));
        }

        public MoveDisposition GetNextMove()
        {
            var request = _dispositionRequest;
            Thread.MemoryBarrier();
            if (request == null) return MoveDisposition.GoStraight;
            return request.Disposition;
        }

        public event Action<string> GameStateUpdated;

        public void SetNextMove(MoveDisposition moveDisposition)
        {
            // Called from various threads by PlayersConnector.
            var request = new DispositionRequest(DateTime.UtcNow, moveDisposition);
            Thread.MemoryBarrier();
            _dispositionRequest = request;
        }

        private class DispositionRequest
        {
            public DispositionRequest(DateTime timestamp, MoveDisposition disposition)
            {
                Timestamp = timestamp;
                Disposition = disposition;
            }

            public DateTime Timestamp { get; }
            public MoveDisposition Disposition { get; }
        }
    }
}