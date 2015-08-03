using System;
using SnakeWars.Contracts;

namespace SnakeWars.WinFormsViewer
{
    internal interface IContestStateListener
    {
        void ExceptionDetected(Exception exception);
        void UpdateState(TournamentStateDTO state);
    }
}