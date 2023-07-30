using System;
using SimpleRPG.Events;

namespace SimpleRPG.Battle
{
    public interface IStatusable
    {
        EventHandler<StatusAddedEventArgs> StatusAdded { get; set; }

        void OnStatusAdded(StatusAddedEventArgs e);

        EventHandler<StatusRemovedEventArgs> StatusRemoved { get; set; }

        void OnStatusRemoved(StatusRemovedEventArgs e);
    }
}
