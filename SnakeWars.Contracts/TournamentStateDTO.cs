using System.Collections.Generic;

namespace SnakeWars.Contracts
{
    public class TournamentStateDTO
    {
        public GameStateDTO GameState { get; set; }
        public int GameNumber { get; set; }
        public IEnumerable<PlayerPublicInfoDTO> Players { get; set; }
    }
}