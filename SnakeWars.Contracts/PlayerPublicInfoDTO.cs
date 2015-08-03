namespace SnakeWars.Contracts
{
    public class PlayerPublicInfoDTO : IPlayerPublicInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int TotalScore { get; set; }
    }
}