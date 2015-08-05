using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using SnakeWars.ContestRunner.Properties;

namespace SnakeWars.ContestRunner
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var options = new Options();
                if (Parser.Default.ParseArgumentsStrict(args, options))
                {
                    var generators = new Dictionary<string, Func<Func<BoardDefinition>>>
                    {
                        {
                            "empty",
                            () => () =>
                                BoardFactory.EmptyBoard(options.BoardWidth, options.BoardHeight,
                                    options.TurnTimeMilliseconds)
                        },
                        {
                            "boxed",
                            () => () =>
                                BoardFactory.BoardWithOuterWalls(options.BoardWidth, options.BoardHeight,
                                    options.TurnTimeMilliseconds)
                        },
                        {
                            "complex",
                            () =>
                                BoardFactory.CreateComplexBoardGenerator(options.TurnTimeMilliseconds,
                                    options.RepetitionCount)
                        }
                    };

                    Func<Func<BoardDefinition>> boardGeneratorFactory;
                    if (generators.TryGetValue(options.Generator.ToLower().Trim(), out boardGeneratorFactory))
                    {
                        RunTournament(boardGeneratorFactory());
                    }
                    else
                    {
                        Console.WriteLine($"Invalid board generator name: {options.Generator}.");
                        Console.WriteLine($"Available generators: {string.Join(", ", generators.Keys)}.");
                    }
                }
                else
                {
                    // Display the default usage information
                    Console.WriteLine(HelpText.AutoBuild(options));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex}");
            }
        }

        private static void RunTournament(Func<BoardDefinition> boardGenerator)
        {
            try
            {
                var tournament = new Tournament(boardGenerator)
                {
                    GameBreakTime = TimeSpan.FromMilliseconds(Settings.Default.GameBreakTimeMilliseconds)
                };

                var connectorCts = new CancellationTokenSource();

                var viewersConnector = new ViewersConnector(Settings.Default.ViewersConnectorPort);
                tournament.StateUpdated += state => viewersConnector.UpdateState(state);
                var viewersConnectorTask = viewersConnector.Start(connectorCts.Token);

                var playersConnector = new PlayersConnector(Settings.Default.PlayersConnectorPort, tournament.Players);
                var playersConnectorTask = playersConnector.Start(connectorCts.Token);

                var stopTournamentSignal = new ManualResetEventSlim();
                var gameRunnerTask = Task.Factory.StartNew(() => { tournament.Run(() => stopTournamentSignal.IsSet); });

                Console.WriteLine("Press any key to stop tournament...");
                Task.WaitAny(gameRunnerTask, playersConnectorTask, viewersConnectorTask,
                    Task.Factory.StartNew(() => Console.ReadKey()));

                stopTournamentSignal.Set();
                Task.WaitAll(gameRunnerTask);
                connectorCts.Cancel();
                Task.WaitAll(playersConnectorTask, viewersConnectorTask);
            }
            catch (Exception ex)
                when (
                    (ex is TaskCanceledException) ||
                    (((ex as AggregateException)?.InnerExceptions.All(ie => (ie is TaskCanceledException))) ?? false))
            {
                // Ignore task cancellation exceptions
            }
        }

        private class Options
        {
            [Option('g', "generator", DefaultValue = "empty", HelpText = "Board generator for tournament.")]
            public string Generator { get; set; }

            [Option('t', "turn", DefaultValue = 200, HelpText = "Turn time in milliseconds.")]
            public int TurnTimeMilliseconds { get; set; }

            [Option('w', "width", DefaultValue = 30, HelpText = "Board width (if applicable).")]
            public int BoardWidth { get; set; }

            [Option('h', "height", DefaultValue = 20, HelpText = "Board height (if applicable).")]
            public int BoardHeight { get; set; }

            [Option('r', "repeat", DefaultValue = 10, HelpText = "Board repetition count (if applicable).")]
            public int RepetitionCount { get; set; }
        }
    }
}