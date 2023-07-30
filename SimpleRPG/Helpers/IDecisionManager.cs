using SimpleRPG.Battle.Fighters;
using SimpleRPG.Events;
using SimpleRPG.Regions;

namespace SimpleRPG.Helpers
{

    public interface IDecisionManager
    {
       void  AssignNameBonuses(HumanFighter fighter1, HumanFighter fighter2);

        void PersonalityQuiz(HumanFighter fighter1, HumanFighter fighter2,
            bool randomizeAnswers = false, IChanceService chanceService = null);

        T PickNextArea<T, TAreaId>(MapGrouping<T, TAreaId> grouping, Team advancingTeam) where T : Area<TAreaId>;

        void ResetDecisionsAfterRegionCleared(object sender, RegionCompletedEventArgs e);
    }
}
