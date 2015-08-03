using System.Collections.Generic;
using Newtonsoft.Json;

namespace SnakeWars.Contracts
{
    public class TournamentStateDTO
    {
        [JsonConverter(typeof (ConcreteConverter<GameStateDTO>))]
        public IGameState GameState { get; set; }

        public int GameNumber { get; set; }

        [JsonConverter(typeof (ConcreteConverter<IEnumerable<PlayerPublicInfoDTO>>))]
        public IEnumerable<IPlayerPublicInfo> Players { get; set; }
    }
}