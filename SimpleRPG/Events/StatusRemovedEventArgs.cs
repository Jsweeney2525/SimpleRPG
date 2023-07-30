using System;
using SimpleRPG.Battle.Statuses;

namespace SimpleRPG.Events
{
    public class StatusRemovedEventArgs : EventArgs
    {
        public Status Status { get; private set; }

        public StatusRemovedEventArgs(Status status)
        {
            Status = status;
        }
    }
}
