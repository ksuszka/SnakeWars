namespace SnakeWars.ContestRunner
{
    public interface IPlayer
    {
        string Id { get; }
        void NewTurn(GameState gameState);
        MoveDisposition GetNextMove();
    }
}