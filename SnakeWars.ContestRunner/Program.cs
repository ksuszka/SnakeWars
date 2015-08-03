using System;
using System.Threading;
using System.Threading.Tasks;

namespace SnakeWars.ContestRunner
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var stopTournamentSignal = new ManualResetEventSlim();
                var gameRunnerTask = Task.Factory.StartNew(() =>
                {
                    var tournament = new Tournament();

                    var stopConnectorsSignal = new ManualResetEventSlim();
                    var viewersConnector = new ViewersConnector(stopConnectorsSignal, 9933);
                    tournament.StateUpdated += state => viewersConnector.UpdateState(state);
                    var viewersConnectorTask = viewersConnector.Start();

                    var playersConnector = new PlayersConnector(stopConnectorsSignal, 9977, tournament.Players);
                    var playersConnectorTask = playersConnector.Start();


                    tournament.Run(() => stopTournamentSignal.IsSet);

                    stopConnectorsSignal.Set();
                    Task.WaitAll(viewersConnectorTask, playersConnectorTask);
                });
                Console.WriteLine("Press any key to stop tournament...");
                Task.WaitAny(gameRunnerTask, Task.Factory.StartNew(() => Console.ReadKey()));
                stopTournamentSignal.Set();
                gameRunnerTask.Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex}");
            }
        }
    }
}