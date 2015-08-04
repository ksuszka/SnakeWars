using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SnakeWars.ContestRunner.Properties;

namespace SnakeWars.ContestRunner
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var tournament = new Tournament();

                var stopConnectorsSignal = new ManualResetEventSlim();
                var viewersConnector = new ViewersConnector(stopConnectorsSignal, Settings.Default.ViewersConnectorPort);
                tournament.StateUpdated += state => viewersConnector.UpdateState(state);
                var viewersConnectorTask = viewersConnector.Start();

                var playersConnector = new PlayersConnector(stopConnectorsSignal, Settings.Default.PlayersConnectorPort, tournament.Players);
                var playersConnectorTask = playersConnector.Start();

                var stopTournamentSignal = new ManualResetEventSlim();
                var gameRunnerTask = Task.Factory.StartNew(() =>
                {
                    tournament.Run(() => stopTournamentSignal.IsSet);
                });
                Console.WriteLine("Press any key to stop tournament...");
                Task.WaitAny(gameRunnerTask, viewersConnectorTask, playersConnectorTask, Task.Factory.StartNew(() => Console.ReadKey()));
                stopTournamentSignal.Set();
                Task.WaitAll(gameRunnerTask);
                stopConnectorsSignal.Set();
                Task.WaitAll(viewersConnectorTask, playersConnectorTask);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex}");
            }
        }
    }
}