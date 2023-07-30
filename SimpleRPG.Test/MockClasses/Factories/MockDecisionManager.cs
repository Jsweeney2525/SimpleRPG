using System;
using System.Linq;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Events;
using SimpleRPG.Helpers;
using SimpleRPG.Regions;

namespace SimpleRPG.Test.MockClasses.Factories
{
    public class MockDecisionManager : IDecisionManager
    {
        private readonly DecisionManager _realDecisionManager;

        public MockDecisionManager(GodRelationshipManager relationshipManager)
        {
            _realDecisionManager = new DecisionManager(relationshipManager);
        }

        public void AssignNameBonuses(HumanFighter fighter1, HumanFighter fighter2)
        {
            _realDecisionManager.AssignNameBonuses(fighter1, fighter2);
        }

        public void PersonalityQuiz(HumanFighter fighter1, HumanFighter fighter2,
            bool randomizeAnswers = false, IChanceService chanceService = null)
        {
            _realDecisionManager.PersonalityQuiz(fighter1, fighter2, randomizeAnswers, chanceService);
        }

        private int _groupingChoiceIndex = -1;

        public void SetGroupingChoice(int selectedIndex)
        {
            _groupingChoiceIndex = selectedIndex;
        }

        public TArea PickNextArea<TArea, TAreaId>(MapGrouping<TArea, TAreaId> grouping, Team advancingTeam) where TArea : Area<TAreaId>
        {
            TArea ret = _groupingChoiceIndex == -1
                ? _realDecisionManager.PickNextArea<TArea, TAreaId>(grouping, advancingTeam) 
                : grouping.GetAvaialableAreas().ToList()[_groupingChoiceIndex];

            return ret;
        }

        public Action<object, RegionCompletedEventArgs> ResetDecisionsAfterRegionClearedCallback { get; set; }

        public void ResetDecisionsAfterRegionCleared(object sender, RegionCompletedEventArgs e)
        {
            ResetDecisionsAfterRegionClearedCallback?.Invoke(sender, e);
        }
    }
}
