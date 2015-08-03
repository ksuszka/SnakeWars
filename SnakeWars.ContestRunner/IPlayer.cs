using SnakeWars.Contracts;

namespace SnakeWars.ContestRunner
{
    internal interface IPlayer : IPlayerPublicInfo
    {
        void NewTurn(GameState gameState);
        MoveDisposition GetNextMove();
    }
}