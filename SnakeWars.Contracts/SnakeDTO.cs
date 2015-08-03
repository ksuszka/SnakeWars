using System.Collections.Generic;

namespace SnakeWars.Contracts
{
    public class SnakeDTO
    {
        public IEnumerable<PointDTO> Cells { get; set; }
        public string Id { get; set; }
        public PointDTO Head { get; set; }
        public bool IsAlive { get; set; }
        public int Score { get; set; }
        public int Weight { get; set; }
        public int MaxWeight { get; set; }
    }
}