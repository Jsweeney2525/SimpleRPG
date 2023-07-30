using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Enums;
using SimpleRPG.Events;

namespace SimpleRPG.Battle.Statuses
{
    public class StatusManager
    {
        public List<Status> Statuses { get; }

        public StatusManager()
        {
            Statuses = new List<Status>();
        }

        /// <summary>
        /// Will either add a new status that did not previously exist, or else reset the move counter of a status that already exists.
        /// </summary>
        /// <param name="status">The status to be added or refreshed</param>
        public void AddOrRefreshStatus(Status status)
        {
            Status statusCopy = status.Copy();

            Status existingStatus = Statuses.FirstOrDefault(status.AreEqual);

            if (existingStatus == null)
            {
                Statuses.Add(statusCopy);
            }
            else if(existingStatus.TurnCounter < status.TurnCounter)
            {
                int existingStatusIndex = Statuses.IndexOf(existingStatus);

                Statuses[existingStatusIndex] = statusCopy;
            }
        }

        public IEnumerable<Status> RemoveStatuses(Func<Status, bool> removePredicate)
        {
            IEnumerable<Status> removedStatuses = Statuses.Where(removePredicate).ToList();

            foreach (Status removeStatus in removedStatuses)
            {
                Statuses.Remove(removeStatus);
            }

            return removedStatuses;
        }

        public void DecrementTurnCounters()
        {
            Statuses.ForEach(s => s.DecrementTurnCounter());
        }

        public void TurnEnded(object sender, TurnEndedEventArgs e)
        {
            IStatusable senderAsStasusable = sender as IStatusable;

            if (senderAsStasusable == null)
            {
                throw new ArgumentException("StatusManager.TurnEnded() should only subscribe to classes that implement IStatusable!");
            }

            List<Status> turnEndStatuses = Statuses.Where(s => s.IsTurnEndStatus).ToList();

            turnEndStatuses.ForEach(s => ExecuteTurnEndStatus(s, sender));

            DecrementTurnCounters();
        }

        public void RoundEnded(object sender, RoundEndedEventArgs e)
        {
            IStatusable senderAsStasusable = sender as IStatusable;

            if (senderAsStasusable == null)
            {
                throw new ArgumentException("StatusManager.RoundEnded() should only subscribe to classes that implement IStatusable!");
            }

            List<Status> statusesToRemove = Statuses.Where(s => s.TurnCounter == 0).ToList();
            Statuses.RemoveAll(s => statusesToRemove.Contains(s));

            statusesToRemove.ForEach(s =>
            {
                StatusRemovedEventArgs statusRemovedEventArgs = new StatusRemovedEventArgs(s);
                senderAsStasusable.OnStatusRemoved(statusRemovedEventArgs);
            });
        }

        private void ExecuteTurnEndStatus(Status status, object sender)
        {
            RestorePercentageStatus restoreStatus = status as RestorePercentageStatus;
            

            if (restoreStatus != null)
            {
                Team senderAsTeam = sender as Team;

                IFighter senderAsFighter = sender as IFighter;

                if (senderAsTeam != null)
                {
                    ExecuteRestorePrecentageStatus(restoreStatus, senderAsTeam);
                }
                else if (senderAsFighter != null)
                {
                    ExecuteRestorePrecentageStatus(restoreStatus, senderAsFighter);
                }
            }
        }

        private void ExecuteRestorePrecentageStatus(RestorePercentageStatus status, Team team)
        {
            foreach (IFighter fighter in team.Fighters)
            {
                ExecuteRestorePrecentageStatus(status, fighter);
            }
        }

        private void ExecuteRestorePrecentageStatus(RestorePercentageStatus status, IFighter fighter)
        {
            if (fighter == null)
            {
                throw new ArgumentException("RestorePercentageStatus somehow given to something that is not an IFighter!");
            }

            int restoreAmount;
            switch (status.RestorationType)
            {
                case RestorationType.Health:
                    restoreAmount = (int)(fighter.MaxHealth * status.Percentage);
                    fighter.Heal(restoreAmount);
                    break;
                case RestorationType.Mana:
                    restoreAmount = (int)(fighter.MaxMana * status.Percentage);
                    fighter.RestoreMana(restoreAmount);
                    break;
            }
        }
    }
}
