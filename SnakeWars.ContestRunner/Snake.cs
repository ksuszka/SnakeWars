using System;
using System.Collections.Generic;
using System.Linq;

namespace SnakeWars.ContestRunner
{
    public class Snake
    {
        public static readonly IList<SnakeStatus> Directions = new[]
        {
            SnakeStatus.MovingUp,
            SnakeStatus.MovingRight,
            SnakeStatus.MovingDown,
            SnakeStatus.MovingLeft
        }.ToList();

        private static readonly IDictionary<SnakeStatus, Tuple<int, int>> Offsets = new Dictionary
            <SnakeStatus, Tuple<int, int>>
        {
            {SnakeStatus.MovingUp, Tuple.Create(0, 1)},
            {SnakeStatus.MovingDown, Tuple.Create(0, -1)},
            {SnakeStatus.MovingLeft, Tuple.Create(-1, 0)},
            {SnakeStatus.MovingRight, Tuple.Create(1, 0)}
        };

        private readonly Size _boardSize;
        private readonly List<Point> _cells = new List<Point>();
        private int _desiredLength;
        private int _moveCount;
        private SnakeStatus _status;

        public Snake(string id, int initialLength, Point initialPosition, SnakeStatus initialDirection, int maxWeight,
            Size boardSize)
        {
            Id = id;
            _desiredLength = Math.Max(1, initialLength);
            _status = initialDirection;
            _cells.Add(initialPosition);
            _boardSize = boardSize;
            MaxWeight = maxWeight;
            ResetWeight();
        }

        public IEnumerable<Point> Cells => _cells;
        public string Id { get; }
        public Point Head => _cells.Last();
        public bool IsAlive => _status != SnakeStatus.Dead;
        public int Score => _moveCount + _desiredLength;
        public int Weight { get; set; }
        public int MaxWeight { get; }
        private void ResetWeight() => Weight = MaxWeight;

        public void UpdateDirection(MoveDisposition move)
        {
            switch (move)
            {
                case MoveDisposition.TurnLeft:
                {
                    var index = Directions.IndexOf(_status);
                    if (index >= 0)
                    {
                        _status = Directions[(index - 1 + Directions.Count)%Directions.Count];
                    }
                }
                    break;
                case MoveDisposition.TurnRight:
                {
                    var index = Directions.IndexOf(_status);
                    if (index >= 0)
                    {
                        _status = Directions[(index + 1 + Directions.Count)%Directions.Count];
                    }
                }
                    break;
            }
        }

        public void Move()
        {
            if (Offsets.ContainsKey(_status))
            {
                _moveCount++;
                var offset = Offsets[_status];
                var head = Head;
                var newHead = new Point((head.X + offset.Item1 + _boardSize.Width)%_boardSize.Width,
                    (head.Y + offset.Item2 + _boardSize.Height)%_boardSize.Height);
                _cells.Add(newHead);

                Weight--;
                if (Weight <= 0)
                {
                    Kill();
                }
            }

            if (_cells.Count > _desiredLength)
            {
                _cells.RemoveRange(0, _cells.Count - _desiredLength);
            }
        }

        public void Kill()
        {
            _status = SnakeStatus.Dead;
        }

        public void Grow()
        {
            if (IsAlive)
            {
                _desiredLength++;
                ResetWeight();
            }
        }
    }
}