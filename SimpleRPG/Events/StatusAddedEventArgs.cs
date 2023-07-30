using System;
using SimpleRPG.Battle.Statuses;

namespace SimpleRPG.Events
{
    public class StatusAddedEventArgs : EventArgs
    {
        public Status Status { get; private set; }

        public StatusAddedEventArgs(Status status)
        {
            Status = status;
        }
    }
}
