using System.Collections.Generic;
using AutoMapper;
using SnakeWars.Contracts;

namespace SnakeWars.ContestRunner
{
    public static class Mapping
    {
        static Mapping()
        {
            Mapper.CreateMap<RemotePlayer, PlayerPublicInfoDTO>();
            Mapper.CreateMap<GameState, GameStateDTO>();
            Mapper.CreateMap<Snake, SnakeDTO>().ForMember(s => s.Direction, o => o.MapFrom(s => s.Status));
            Mapper.CreateMap<Point, PointDTO>();
            Mapper.CreateMap<Size, SizeDTO>();

            Mapper.CreateMap<SnakeStatus, SnakeDirection>().ConvertUsing(s =>
            {
                switch (s)
                {
                    case SnakeStatus.MovingLeft:
                        return SnakeDirection.Left;
                    case SnakeStatus.MovingRight:
                        return SnakeDirection.Right;
                    case SnakeStatus.MovingDown:
                        return SnakeDirection.Down;
                    case SnakeStatus.MovingUp:
                        return SnakeDirection.Up;
                }
                return SnakeDirection.None;
            });

            Mapper.AssertConfigurationIsValid();
        }

        public static TournamentStateDTO CreateTournamentStateDTO(int gameNumber, GameState gameState,
            IEnumerable<IPlayer> players)
        {
            return new TournamentStateDTO
            {
                GameState = Mapper.Map<GameStateDTO>(gameState),
                GameNumber = gameNumber,
                Players = Mapper.Map<IEnumerable<PlayerPublicInfoDTO>>(players)
            };
        }

        public static GameStateDTO CreateGameStateDTO(GameState gameState)
        {
            return Mapper.Map<GameStateDTO>(gameState);
        }
    }
}