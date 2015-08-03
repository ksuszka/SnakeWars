using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SnakeWars.Contracts;
using SnakeWars.WinFormsViewer.Properties;

namespace SnakeWars.WinFormsViewer
{
    public partial class MainForm : Form, IContestStateListener
    {
        private readonly Visualiser _visualiser;

        public MainForm()
        {
            InitializeComponent();
            rightSplitContainer.SplitterWidth = 12;
            var fc = new FontConverter();
            tbInfo.Font = fc.ConvertFromInvariantString(Settings.Default.InfoFont) as Font;
            _visualiser = new Visualiser(Settings.Default.VisualisationCellSize);
        }

        public void ExceptionDetected(Exception exception)
        {
            BeginInvoke(new Action(() => { tbStatus.Text = exception.Message; }));
        }

        public void UpdateState(TournamentStateDTO state)
        {
            BeginInvoke(new Action(() =>
            {
                tbStatus.Text = $"Game {state.GameNumber}, Turn {state.GameState?.Turn}";
                tbInfo.Text = GenerateInfo(state);
                DrawBoard(state.GameState);
            }));
        }

        private void DrawBoard(IGameState gs)
        {
            pbGameView.Image?.Dispose();
            if (gs == null)
            {
                pbGameView.Image = null;
            }
            else
            {
                pbGameView.Image = _visualiser.CreateBoardImage(gs);
            }
        }

        private static string GenerateInfo(TournamentStateDTO state)
        {
            var info = new StringBuilder();
            info.AppendLine("Overall classification:");
            info.AppendLine();
            state.Players.ToList()
                .OrderByDescending(p => p.TotalScore)
                .ToList()
                .ForEach(player => { info.AppendLine($"{player.Name} - {player.TotalScore}"); });

            info.AppendLine();
            info.AppendLine("Current game:");
            info.AppendLine($"Game {state.GameNumber}, Turn {state.GameState?.Turn}");

            info.AppendLine();
            if (state.GameState != null)
            {
                var maxPlayerNameLength = Math.Max(7, state?.Players.Max(p => p.Name.Length) ?? 0);
                var maxIdLength = Math.Max(2, state.GameState.Snakes.Max(p => p.Id.Length));
                info.AppendLine(
                    $"{"ID".PadRight(maxIdLength)} {"Name".PadRight(maxPlayerNameLength)}Status Length Weight Score");
                var playerMap = state.Players.ToDictionary(k => k.Id, v => v.Name);
                var snakes = state.GameState.Snakes.ToList();
                for (var i = 0; i < snakes.Count; i++)
                {
                    var snake = snakes[i];
                    var playerName = "Unknown";
                    playerMap.TryGetValue(snake.Id, out playerName);

                    info.AppendLine(
                        $"{snake.Id.Trim().PadRight(maxIdLength)} {playerName.Trim().PadRight(maxPlayerNameLength)}{(snake.IsAlive ? "ALIVE" : " DEAD"),6}{snake.Cells.Count(),7}{snake.Weight,7}{snake.Score,6}");
                }
            }
            return info.ToString();
        }
    }
}