using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using SnakeWars.Contracts;
using Point = System.Drawing.Point;

namespace SnakeWars.WinFormsViewer
{
    internal static class PointExtensions
    {
        public static Point Move(this Point p, int dx, int dy)
        {
            return new Point(p.X + dx, p.Y + dy);
        }
    }

    internal class Visualiser
    {
        private static readonly Color[] _snakeColors =
        {
            Color.Brown, Color.BlueViolet, Color.DarkCyan, Color.ForestGreen,
            Color.Chocolate, Color.Olive, Color.DodgerBlue, Color.DeepPink,
            Color.Gold, Color.DeepSkyBlue, Color.DarkOliveGreen, Color.Fuchsia
        };

        private static readonly Brush _wallBrush = Brushes.Black;
        private static readonly Pen _gridPen = Pens.LightSlateGray;
        private static readonly Brush HeadFontBrush = Brushes.White;
        private static readonly Brush BackgroundBrush = Brushes.White;
        private readonly int _cellOuterWidth;
        private readonly int _cellWidth;
        private readonly Pen _deadSnakeOutlinePen;
        private readonly Pen _foodPen;
        private readonly Font _snakeHeadFont;
        private readonly Pen _snakeHeadOverlayPen;
        private readonly Pen[][] _snakeTailPens;
        private static readonly StringFormat _headLabelStringFormat = new StringFormat(StringFormatFlags.NoWrap | StringFormatFlags.NoClip | StringFormatFlags.FitBlackBox) {Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.None};

        public Visualiser(int cellWidth)
        {
            _cellWidth = cellWidth;
            _cellOuterWidth = _cellWidth + 1;
            _snakeHeadFont = new Font(FontFamily.GenericSansSerif, _cellWidth*2/3, FontStyle.Regular, GraphicsUnit.Pixel);
            _snakeHeadOverlayPen = new Pen(Color.FromArgb(128, Color.Black), _cellWidth - 2)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };
            _snakeTailPens = _snakeColors.Select(
                color =>
                    Enumerable.Range(1, (_cellWidth - 5)/2)
                        .Select(w => w*2 + 1)
                        .Select(width => new Pen(color, width) {StartCap = LineCap.Round, EndCap = LineCap.Round})
                        .ToArray()).ToArray();
            _deadSnakeOutlinePen = new Pen(Color.Red, _cellWidth) {StartCap = LineCap.Round, EndCap = LineCap.Round};
            _foodPen = new Pen(Color.GreenYellow, _cellWidth - 4) {StartCap = LineCap.Round, EndCap = LineCap.Round};
        }


        public Bitmap CreateBoardImage(GameStateDTO gs)
        {
            var adjustPoint = new Func<Contracts.PointDTO, Point>(p => new Point(p.X, gs.BoardSize.Height - 1 - p.Y));
            var bitmap = new Bitmap(gs.BoardSize.Width*_cellOuterWidth + 1, gs.BoardSize.Height*_cellOuterWidth + 1);
            using (var g = Graphics.FromImage(bitmap))
            {
                // background
                g.FillRectangle(BackgroundBrush, 0, 0, bitmap.Width, bitmap.Height);
                Enumerable.Range(0, gs.BoardSize.Width + 1)
                    .ToList()
                    .ForEach(x => g.DrawLine(_gridPen, x*_cellOuterWidth, 0, x*_cellOuterWidth, bitmap.Height));
                Enumerable.Range(0, gs.BoardSize.Height + 1)
                    .ToList()
                    .ForEach(y => g.DrawLine(_gridPen, 0, y*_cellOuterWidth, bitmap.Width, y*_cellOuterWidth));

                gs.Food.Select(adjustPoint).ToList().ForEach(location =>
                {
                    var p = PointToGridCellCenter(location);
                    g.DrawLine(_foodPen, new PointF(p.X - 0.1f, p.Y), new PointF(p.X + 0.1f, p.Y));
                });

                gs.Walls.Select(adjustPoint).ToList()
                    .ForEach(
                        m =>
                        {
                            g.FillRectangle(_wallBrush, m.X*_cellOuterWidth + 1, m.Y*_cellOuterWidth + 1, _cellWidth,
                                _cellWidth);
                        });

                var snakes = gs.Snakes.ToList();
                for (var i = 0; i < snakes.Count; i++)
                {
                    var snake = snakes[i];
                    var cells = snake.Cells.Select(adjustPoint).ToList();
                    var head = adjustPoint(snake.Head);

                    if (!snake.IsAlive)
                    {
                        DrawSnakeLine(cells, g, _deadSnakeOutlinePen);
                    }
                    var tailPens = _snakeTailPens[i%_snakeTailPens.Length];
                    // If snake weigth is less than half of it maximum weight it is drown with thinner body
                    var tailPen =
                        tailPens[Math.Min(tailPens.Length - 1, 2*tailPens.Length*snake.Weight/snake.MaxWeight)];
                    DrawSnakeLine(cells, g, tailPen);

                    // head
                    var p = PointToGridCellCenter(cells.Last());
                    g.DrawLine(tailPens.Last(), new PointF(p.X - 0.1f, p.Y), new PointF(p.X + 0.1f, p.Y));
                    g.DrawLine(_snakeHeadOverlayPen, new PointF(p.X - 0.1f, p.Y), new PointF(p.X + 0.1f, p.Y));

                    g.DrawString(snake.Id, _snakeHeadFont, HeadFontBrush,
                        new RectangleF(head.X*_cellOuterWidth + 1, head.Y*_cellOuterWidth + 1, _cellWidth,
                            _cellWidth),_headLabelStringFormat);
                }
            }
            return bitmap;
        }

        private Point PointToGridCellCenter(Point p)
            => new Point(p.X*_cellOuterWidth + _cellOuterWidth/2, p.Y*_cellOuterWidth + _cellOuterWidth/2);

        private void DrawSnakeLine(List<Point> cells, Graphics g, Pen pen)
        {
            Func<Point, Point> converter = PointToGridCellCenter;
            if (cells.Count == 1)
            {
                var p = converter(cells.First());
                g.DrawLine(pen, new PointF(p.X - 0.1f, p.Y), new PointF(p.X + 0.1f, p.Y));
            }
            else if (cells.Count > 1)
            {
                for (var j = 0; j < cells.Count - 1; j++)
                {
                    var p1 = cells[j];
                    var p2 = cells[j + 1];
                    // if cells are not neighbours then board edge was crossed
                    if (p2.X - p1.X > 1)
                    {
                        g.DrawLine(pen, converter(p1), converter(p1.Move(-1, 0)));
                        g.DrawLine(pen, converter(p2), converter(p2.Move(1, 0)));
                    }
                    else if (p1.X - p2.X > 1)
                    {
                        g.DrawLine(pen, converter(p1), converter(p1.Move(1, 0)));
                        g.DrawLine(pen, converter(p2), converter(p2.Move(-1, 0)));
                    }
                    else if (p2.Y - p1.Y > 1)
                    {
                        g.DrawLine(pen, converter(p1), converter(p1.Move(0, -1)));
                        g.DrawLine(pen, converter(p2), converter(p2.Move(0, 1)));
                    }
                    else if (p1.Y - p2.Y > 1)
                    {
                        g.DrawLine(pen, converter(p1), converter(p1.Move(0, 1)));
                        g.DrawLine(pen, converter(p2), converter(p2.Move(0, -1)));
                    }
                    else
                    {
                        g.DrawLine(pen, converter(p1), converter(p2));
                    }
                }
            }
        }
    }
}