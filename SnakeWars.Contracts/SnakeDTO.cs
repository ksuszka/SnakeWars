using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SnakeWars.Contracts
{
    public class SnakeDTO
    {
        public IEnumerable<PointDTO> Cells { get; set; }
        public string Id { get; set; }
        public PointDTO Head { get; set; }
        public bool IsAlive { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public SnakeDirection Direction { get; set; }
        public int Score { get; set; }
        public int Weight { get; set; }
        public int MaxWeight { get; set; }
    }
}