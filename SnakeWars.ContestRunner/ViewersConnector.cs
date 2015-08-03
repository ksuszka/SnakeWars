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
    internal class ViewersConnector
    {
        private readonly int _listenerPort;
        private readonly ManualResetEventSlim _stopSignal;
        private readonly ViewersPool<TcpClient> _viewersPool;

        public ViewersConnector(ManualResetEventSlim stopSignal, int listenerPort)
        {
            _stopSignal = stopSignal;
            _listenerPort = listenerPort;
            _viewersPool = new ViewersPool<TcpClient>();
        }

        public Task Start()
        {
            return Task.Factory.StartNew(() =>
            {
                var listener = new TcpListener(IPAddress.Any, _listenerPort);
                listener.Start();
                var clients = new List<Task>();
                while (!_stopSignal.IsSet)
                {
                    var acceptTask = listener.AcceptTcpClientAsync();
                    WaitHandle.WaitAny(new[] {_stopSignal.WaitHandle, ((IAsyncResult) acceptTask).AsyncWaitHandle});
                    if (acceptTask.IsCompleted)
                    {
                        var tcpClient = acceptTask.Result;
                        clients = clients.Where(task => !task.IsCompleted).ToList();
                        clients.Add(Task.Factory.StartNew(() => HandleSingleViewer(tcpClient)));
                    }
                }
                Task.WaitAll(clients.ToArray());
            });
        }

        private void HandleSingleViewer(TcpClient tcpClient)
        {
            try
            {
                Console.WriteLine($"New viewer connected: {tcpClient.Client.RemoteEndPoint}");
                tcpClient.NoDelay = true;
                using (var stream = new StreamWriter(tcpClient.GetStream()))
                {
                    stream.AutoFlush = true;
                    var errorDetected = new ManualResetEventSlim();
                    _viewersPool.Add(tcpClient, line =>
                    {
                        try
                        {
                            stream.WriteLine(line);
                        }
                        catch (Exception ex)
                        {
                            _viewersPool.Remove(tcpClient);
                            Console.WriteLine(
                                $"Client {tcpClient.Client.RemoteEndPoint} disconnected with error: {ex.Message}.");
                            errorDetected.Set();
                        }
                    });
                    WaitHandle.WaitAny(new[] {_stopSignal.WaitHandle, errorDetected.WaitHandle});
                }
            }
            finally
            {
                tcpClient.Dispose();
            }
        }

        public void UpdateState(string state)
        {
            _viewersPool.CurrentViewers.ToList().ForEach(v => v(state));
        }
    }
}