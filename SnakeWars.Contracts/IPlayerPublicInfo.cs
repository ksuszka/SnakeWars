namespace SnakeWars.Contracts
{
    public interface IPlayerPublicInfo
    {
        string Id { get; set; }
        string Name { get; set; }
        int TotalScore { get; set; }
    }
}