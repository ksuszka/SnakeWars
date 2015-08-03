using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SnakeWars.Contracts;

namespace SnakeWars.WinFormsViewer
{
    internal class ContestConnector
    {
        private readonly string _contestServerHost;
        private readonly int _contestServerPort;
        private readonly IContestStateListener _stateListener;
        private Task _acceptor;

        public ContestConnector(string contestServerHost, int contestServerPort, IContestStateListener stateListener)
        {
            _contestServerHost = contestServerHost;
            _contestServerPort = contestServerPort;
            _stateListener = stateListener;
            _acceptor = Task.Factory.StartNew(Listen);
        }

        private void Listen()
        {
            while (true)
            {
                try
                {
                    using (var client = new TcpClient())
                    {
                        client.NoDelay = true;
                        client.Connect(_contestServerHost, _contestServerPort);
                        using (var reader = new StreamReader(client.GetStream()))
                        {
                            while (true)
                            {
                                var line = reader.ReadLine();
                                var state = JsonConvert.DeserializeObject<TournamentStateDTO>(line);
                                _stateListener.UpdateState(state);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _stateListener.ExceptionDetected(ex);
                    // wait and try again
                    Thread.Sleep(2000);
                }
            }
        }
    }
}