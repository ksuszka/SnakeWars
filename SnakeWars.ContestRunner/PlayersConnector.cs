using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SnakeWars.ContestRunner
{
    internal class PlayersConnector
    {
        private readonly int _listenerPort;
        private readonly IDictionary<string, RemotePlayer> _players;

        public PlayersConnector(int listenerPort, IEnumerable<RemotePlayer> players)
        {
            _listenerPort = listenerPort;
            _players = players.ToDictionary(p => p.LoginId);
        }

        public Task Start(CancellationToken cancellationToken)
        {
            Console.WriteLine($"Starting player connector on port {_listenerPort}.");
            var listener = new TcpListener(IPAddress.Any, _listenerPort);
            listener.Start();

            return Task.Run(async () =>
            {
                var clients = new List<Task>();
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var tcpClient =
                            await listener.AcceptTcpClientAsync().ContinueWith(t => t.Result, cancellationToken);
                        clients = clients.Where(task => !task.IsCompleted).ToList();
                        clients.Add(HandleSinglePlayer(tcpClient, cancellationToken));
                    }
                }
                finally
                {
                    await Task.WhenAll(clients.ToArray());
                }
            }, cancellationToken);
        }

        private async Task HandleSinglePlayer(TcpClient tcpClient, CancellationToken listenerCancellationToken)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(listenerCancellationToken);
            var remoteEndpoint = "Unknown";
            try
            {
                remoteEndpoint = tcpClient.Client.RemoteEndPoint.ToString();
                Console.WriteLine($"New player connected: {remoteEndpoint}");
                tcpClient.NoDelay = true;
                using (var writer = new StreamWriter(tcpClient.GetStream()))
                {
                    writer.AutoFlush = true;
                    using (var reader = new StreamReader(tcpClient.GetStream()))
                    {
                        await writer.WriteLineAsync("ID", cts.Token);
                        var loginId = await reader.ReadLineAsync(cts.Token);
                        RemotePlayer player;
                        if (!_players.TryGetValue(loginId.Trim(), out player))
                        {
                            await writer.WriteLineAsync("Invalid login id!", cts.Token);
                        }
                        else
                        {
                            await writer.WriteLineAsync(player.Id, cts.Token);
                            var tasks = new[]
                            {
                                HandleIncomingData(player, reader, cts.Token),
                                HandleOutgoingData(player, writer, cts.Token)
                            };
                            await Task.WhenAny(tasks);
                            // If reading or writing fails abort second process
                            cts.Cancel();
                            await Task.WhenAll(tasks);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Connection to {remoteEndpoint} aborted due to error: {ex.GetFlatMessage()}.");
            }
            finally
            {
                tcpClient.Dispose();
            }
        }

        private async Task HandleIncomingData(RemotePlayer player, StreamReader reader,
            CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var line = (await reader.ReadLineAsync(cancellationToken))?.Trim().ToUpper();
                switch (line)
                {
                    case "LEFT":
                        player.SetNextMove(MoveDisposition.TurnLeft);
                        break;
                    case "RIGHT":
                        player.SetNextMove(MoveDisposition.TurnRight);
                        break;
                    case "STRAIGHT":
                        player.SetNextMove(MoveDisposition.GoStraight);
                        break;
                    default:
                        // ignore everything else
                        break;
                }
            }
        }

        private async Task HandleOutgoingData(RemotePlayer player, StreamWriter writer,
            CancellationToken cancellationToken)
        {
            var dataToSend = new BufferBlock<string>();
            var statusUpdater = new Action<string>(state => dataToSend.Post(state));
            try
            {
                player.GameStateUpdated += statusUpdater;

                while (!cancellationToken.IsCancellationRequested)
                {
                    var data = await dataToSend.ReceiveAsync(cancellationToken);
                    await writer.WriteLineAsync(data, cancellationToken);
                }
            }
            finally
            {
                player.GameStateUpdated -= statusUpdater;
            }
        }
    }
}