using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SnakeWars.ContestRunner
{
    internal class PlayersConnector
    {
        private readonly int _listenerPort;
        private readonly IDictionary<string, RemotePlayer> _players;
        private readonly ManualResetEventSlim _stopSignal;

        public PlayersConnector(ManualResetEventSlim stopSignal, int listenerPort, IEnumerable<RemotePlayer> players)
        {
            _stopSignal = stopSignal;
            _listenerPort = listenerPort;
            _players = players.ToDictionary(p => p.LoginId);
        }

        public Task Start()
        {
            Console.WriteLine($"Starting player connector on port {_listenerPort}.");
            var listener = new TcpListener(IPAddress.Any, _listenerPort);
            listener.Start();
            return Task.Factory.StartNew(() =>
            {
                var clients = new List<Task>();
                while (!_stopSignal.IsSet)
                {
                    var acceptTask = listener.AcceptTcpClientAsync();
                    WaitHandle.WaitAny(new[] {_stopSignal.WaitHandle, ((IAsyncResult) acceptTask).AsyncWaitHandle});
                    if (acceptTask.IsCompleted)
                    {
                        var tcpClient = acceptTask.Result;
                        clients = clients.Where(task => !task.IsCompleted).ToList();
                        clients.Add(Task.Factory.StartNew(() => HandleSinglePlayer(tcpClient)));
                    }
                }
                // FIX IT: do not wait for clients, as they are written in synchronous way and can block
                //Task.WaitAll(clients.ToArray());
            });
        }

        private void HandleSinglePlayer(TcpClient tcpClient)
        {
            try
            {
                Console.WriteLine($"New player connected: {tcpClient.Client.RemoteEndPoint}");
                tcpClient.NoDelay = true;
                using (var writer = new StreamWriter(tcpClient.GetStream()))
                {
                    writer.AutoFlush = true;
                    using (var reader = new StreamReader(tcpClient.GetStream()))
                    {
                        writer.WriteLine("ID");
                        var loginId = reader.ReadLine();
                        RemotePlayer player;
                        if (!_players.TryGetValue(loginId.Trim(), out player))
                        {
                            writer.WriteLine("Invalid login id!");
                        }
                        else
                        {
                            writer.WriteLine(player.Id);
                            var errorDetected = new ManualResetEventSlim();
                            var statusUpdater = new Action<string>(state =>
                            {
                                try
                                {
                                    writer.WriteLine(state);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(
                                        $"Client {tcpClient.Client.RemoteEndPoint} disconnected with error: {ex.Message}.");
                                    errorDetected.Set();
                                }
                            });
                            try
                            {
                                player.GameStateUpdated += statusUpdater;

                                while (true)
                                {
                                    var readLineTask = reader.ReadLineAsync();
                                    WaitHandle.WaitAny(new[]
                                    {
                                        _stopSignal.WaitHandle, errorDetected.WaitHandle,
                                        ((IAsyncResult) readLineTask).AsyncWaitHandle
                                    });
                                    if (readLineTask.IsCompleted)
                                    {
                                        var line = readLineTask.Result.Trim().ToUpper();
                                        switch (line)
                                        {
                                            case "LEFT":
                                                player.SetNextMove(MoveDisposition.TurnLeft);
                                                break;
                                            case "RIGHT":
                                                player.SetNextMove(MoveDisposition.TurnRight);
                                                break;
                                            default:
                                                // ignore everything else
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(
                                    $"Client {tcpClient.Client.RemoteEndPoint} disconnected with error: {ex.Message}.");
                            }
                            finally
                            {
                                player.GameStateUpdated -= statusUpdater;
                            }
                        }
                    }
                }
            }
            finally
            {
                tcpClient.Dispose();
            }
        }
    }
}