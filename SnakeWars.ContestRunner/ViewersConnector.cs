using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SnakeWars.ContestRunner
{
    internal class ViewersConnector
    {
        private readonly int _listenerPort;

        public ViewersConnector(int listenerPort)
        {
            _listenerPort = listenerPort;
        }

        public Task Start(CancellationToken cancellationToken)
        {
            Console.WriteLine($"Starting viewer connector on port {_listenerPort}.");
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

        private async Task HandleSinglePlayer(TcpClient tcpClient, CancellationToken cancellationToken)
        {
            var remoteEndpoint = "Unknown";
            try
            {
                remoteEndpoint = tcpClient.Client.RemoteEndPoint.ToString();
                Console.WriteLine($"New viewer connected: {remoteEndpoint}");
                tcpClient.NoDelay = true;
                using (var writer = new StreamWriter(tcpClient.GetStream()))
                {
                    writer.AutoFlush = true;
                    await HandleOutgoingData(writer, cancellationToken);
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

        private async Task HandleOutgoingData(StreamWriter writer,
            CancellationToken cancellationToken)
        {
            var dataToSend = new ConcurrentQueue<string>();
            var newData = new AutoResetEvent(false);
            var statusUpdater = new Action<string>(state =>
            {
                dataToSend.Enqueue(state);
                newData.Set();
            });
            try
            {
                StateUpdated += statusUpdater;

                while (!cancellationToken.IsCancellationRequested)
                {
                    string data;
                    while (dataToSend.TryDequeue(out data))
                    {
                        await writer.WriteLineAsync(data, cancellationToken);
                    }
                    WaitHandle.WaitAny(new[] {newData, cancellationToken.WaitHandle});
                }
            }
            finally
            {
                StateUpdated -= statusUpdater;
            }
        }

        public event Action<string> StateUpdated;
        public void UpdateState(string state) => StateUpdated?.Invoke(state);
    }
}

