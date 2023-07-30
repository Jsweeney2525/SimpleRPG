using System;
using System.Collections.Generic;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Statuses;
using SimpleRPG.Events;
using SimpleRPG.Screens.Menus.MenuActions;

namespace SimpleRPG.Battle.Fighters
{
    public interface ITeam<T> where T:IFighter
    {
        List<T> Fighters { get; }

        #region events

        EventHandler<TeamDefeatedEventArgs> TeamDefeated { get; set; }

        void OnTeamDefeated(TeamDefeatedEventArgs e);

        EventHandler<RoundEndedEventArgs> RoundEnded { get; set; }

        void OnRoundEnded(RoundEndedEventArgs e);

        EventHandler<RanEventArgs> Ran { get; set; }

        void OnRun(RanEventArgs e);

        #endregion events

        bool Contains(IFighter fighter);

        bool IsTeamDefeated();

        void Add(T newFighter, bool setupDisplayNamesAfterAdd = true);

        void AddRange(IEnumerable<T> newFighters);

        void Remove(T fighter);

        void AddStatus(Status status);

        List<BattleMoveWithTarget> GetInputs(Team enemyTeam, List<MenuAction> specialMenuActions = null);
    }
}
