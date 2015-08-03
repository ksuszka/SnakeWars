using System.Collections.Generic;

namespace SnakeWars.Contracts
{
    public class GameStateDTO
    {
        public int Turn { get; set; }
        public IEnumerable<SnakeDTO> Snakes { get; set; }
        public IEnumerable<PointDTO> Walls { get; set; }
        public IEnumerable<PointDTO> Food { get; set; }
        public SizeDTO BoardSize { get; set; }
        public int TurnTimeMilliseconds { get; set; }
    }
}