using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Enums;
using SimpleRPG.Events;
using SimpleRPG.Regions;
using SimpleRPG.Screens;
using SimpleRPG.Screens.Menus;
using SimpleRPG.Screens.Menus.MenuActions;
using SimpleRPG.Screens.Menus.MenuSelections;

namespace SimpleRPG.Helpers
{

    public class DecisionManager : IDecisionManager
    {
        #region private helper classes

        private abstract class DecisionBonus
        {
            public abstract void ApplyBonus(HumanFighter fighter, GodRelationshipManager relationshipManager, bool isSecretBonus = false);
        }

        private class PersonalityDecisionBonus : DecisionBonus
        {
            private readonly PersonalityFlag _personalityFlag;

            public PersonalityDecisionBonus(PersonalityFlag flag)
            {
                _personalityFlag = flag;
            }

            public override void ApplyBonus(HumanFighter fighter, GodRelationshipManager relationshipManager, bool isSecretBonus = false)
            {
                fighter.AddPersonalityFlag(_personalityFlag);
            }
        }

        private abstract class IntDecisionBonus : DecisionBonus
        {
            protected readonly int BonusAmount;

            protected IntDecisionBonus(int bonusAmount)
            {
                BonusAmount = bonusAmount;
            }
        }

        private class StatDecisionBonus : IntDecisionBonus
        {
            private readonly StatType _statType;

            public StatDecisionBonus(int bonusAmount, StatType statType) : base(bonusAmount)
            {
                _statType = statType;
            }

            public override void ApplyBonus(HumanFighter fighter, GodRelationshipManager relationshipManager, bool isSecretBonus = false)
            {
                fighter.AddStatBonus(_statType, BonusAmount, isSecretBonus);
            }
        }

        private class MagicDecisionBonus : IntDecisionBonus
        {
            private readonly MagicType _magicType;

            private readonly MagicStatType _magicStatType;

            public MagicDecisionBonus(int bonusAmount, MagicStatType magicStatType, MagicType magicType) : base(bonusAmount)
            {
                _magicType = magicType;
                _magicStatType = magicStatType;
            }

            public override void ApplyBonus(HumanFighter fighter, GodRelationshipManager relationshipManager, bool isSecretBonus = false)
            {
                fighter.AddMagicBonus(_magicStatType, _magicType, BonusAmount, isSecretBonus);
            }
        }

        private class GodDecisionBonus : IntDecisionBonus
        {
            private readonly GodEnum _god;

            public GodDecisionBonus(int bonusAmount, GodEnum god) : base(bonusAmount)
            {
                _god = god;
            }

            public override void ApplyBonus(HumanFighter fighter, GodRelationshipManager relationshipManager, bool isSecretBonus = false)
            {
                relationshipManager.UpdateRelationship(fighter, _god, BonusAmount);
            }
        }

        private class NameBonuses
        {
            public string Name;

            public List<DecisionBonus> Bonuses;

            public void ApplyBonuses(HumanFighter fighter, GodRelationshipManager relationshipManager)
            {
                foreach (DecisionBonus bonus in Bonuses)
                {
                    bonus.ApplyBonus(fighter, relationshipManager);
                }
            }
        }

        #endregion

        private static readonly List<Tuple<NameBonuses, NameBonuses>> NamePairBonuses = new List<Tuple<NameBonuses, NameBonuses>>
        {
            new Tuple<NameBonuses, NameBonuses>(
                new NameBonuses
                {
                    Name = "Dante",
                    Bonuses = new List<DecisionBonus>
                    {
                        new StatDecisionBonus(1, StatType.Speed),
                        new MagicDecisionBonus(1, MagicStatType.Power, MagicType.Fire),
                        new MagicDecisionBonus(1, MagicStatType.Resistance, MagicType.Fire)
                    }
                }, 
                new NameBonuses
                {
                    Name = "Arrokoh",
                    Bonuses = new List<DecisionBonus>
                    {
                        new StatDecisionBonus(1, StatType.Strength),
                        new MagicDecisionBonus(1, MagicStatType.Power, MagicType.Lightning),
                        new MagicDecisionBonus(1, MagicStatType.Resistance, MagicType.Lightning)
                    }
                }),
            new Tuple<NameBonuses, NameBonuses>(
                new NameBonuses
                {
                    Name = "Chesterton", //The Bucky of the pair
                    Bonuses = new List<DecisionBonus>
                    {
                        new StatDecisionBonus(1, StatType.Strength),
                        new GodDecisionBonus(1, GodEnum.MalevolentGod),
                        new PersonalityDecisionBonus(PersonalityFlag.MorallyFlexible)
                    }
                },
                new NameBonuses
                {
                    Name = "Poopy Carrots", //the Captain America of the pair
                    Bonuses = new List<DecisionBonus>
                    {
                        new StatDecisionBonus(1, StatType.Speed),
                        new GodDecisionBonus(1, GodEnum.MercyGod),
                        new PersonalityDecisionBonus(PersonalityFlag.Heroic)
                    }
                })
        };

        private readonly GodRelationshipManager _relationshipManager;

        private readonly IChanceService _chanceService;

        private readonly IMenuFactory _menuFactory;

        private readonly IInput _input;

        private readonly IOutput _output;

        public DecisionManager(GodRelationshipManager godRelationshipManager, 
            IChanceService chanceService = null, 
            IMenuFactory menuFactory = null,
            IInput input = null,
            IOutput output = null)
        {
            _relationshipManager = godRelationshipManager;
            _chanceService = chanceService ?? Globals.ChanceService;
            _menuFactory = menuFactory ?? Globals.MenuFactory;
            _input = input ?? Globals.Input;
            _output = output ?? Globals.Output;

            _groupingChoicesDictionary = new Dictionary<int, WorldSubRegion>();
        }

        public void AssignNameBonuses(HumanFighter fighter1, HumanFighter fighter2)
        {
            List<HumanFighter> fighterList = new List<HumanFighter> { fighter1, fighter2 };

            foreach (Tuple<NameBonuses, NameBonuses> namePairBonus in NamePairBonuses)
            {
                string firstName  = namePairBonus.Item1.Name.Replace(" ", "");
                string secondName = namePairBonus.Item2.Name.Replace(" ", "");

                HumanFighter firstFighter = fighterList.FirstOrDefault(f => string.Equals(f.BaseName, firstName, StringComparison.InvariantCultureIgnoreCase));
                HumanFighter secondFighter = fighterList.FirstOrDefault(f => string.Equals(f.BaseName, secondName, StringComparison.InvariantCultureIgnoreCase));

                if (firstFighter != null && secondFighter != null)
                {
                    _output.WriteLine("Congratulations! The given names were recognized, and shall be given bonuses!");
                    _input.WaitAndClear(_output);
                    namePairBonus.Item1.ApplyBonuses(firstFighter, _relationshipManager);
                    _input.WaitAndClear(_output);
                    namePairBonus.Item2.ApplyBonuses(secondFighter, _relationshipManager);
                    _input.WaitAndClear(_output);
                }
            }
        }

        private class PersonalityQuizQuestionAndBonus
        {
            public string Question { get; }

            /// <summary>
            /// The text displayed to user to explain which answers were selected when randomizing the questionaire results
            /// </summary>
            public string PersonalityStatement { get; }

            public DecisionBonus[] SelectedPlayerBonuses { get; }

            public DecisionBonus[] NotSelectedPlayerBonuses { get; }

            /// <summary>
            /// Currently only used to represent one case: each fighter is an option, as well as a "both" and a "neither."
            /// If that logic changes to allow more complex logic, a refactor is in order
            /// </summary>
            public bool AllowBonusOptions { get; }

            public PersonalityQuizQuestionAndBonus(string question, string personalityStatement, DecisionBonus[] selectedPlayerBonuses, DecisionBonus[] notSelectedPlayerBonuses, bool allowBonusOptions = false)
            {
                Question = question;
                PersonalityStatement = personalityStatement;
                SelectedPlayerBonuses = selectedPlayerBonuses ?? new DecisionBonus[0];
                NotSelectedPlayerBonuses = notSelectedPlayerBonuses ?? new DecisionBonus[0];
                AllowBonusOptions = allowBonusOptions;
            }
        }
        
        private static readonly PersonalityQuizQuestionAndBonus[] PersonalityQuizQuestionsAndBonuses = {
            new PersonalityQuizQuestionAndBonus("Who is more enigmatic?", "is more enigmatic", new DecisionBonus[] { new PersonalityDecisionBonus(PersonalityFlag.Enigmatic) }, null),
            new PersonalityQuizQuestionAndBonus("Who always wins at cards?", "always wins at cards", new DecisionBonus[] { new StatDecisionBonus(10, StatType.Luck) }, null),
            new PersonalityQuizQuestionAndBonus("Who is more likely to seek treasure?", "is more likely to seek treasure", new DecisionBonus[] { new PersonalityDecisionBonus(PersonalityFlag.Adventurous) }, null),
            new PersonalityQuizQuestionAndBonus("Who sometimes watches the stars at night?", "sometimes watches the stars at night", new DecisionBonus[] { new PersonalityDecisionBonus(PersonalityFlag.Dreamer) }, null),
            new PersonalityQuizQuestionAndBonus("Who is better at solving maze puzzles- you know, getting through a maze on a piece of paper?", "is better at solving maze puzzles", 
                new DecisionBonus[] { new PersonalityDecisionBonus(PersonalityFlag.MazeSolver), new GodDecisionBonus(1, GodEnum.IntellectGod)  }, null),
            new PersonalityQuizQuestionAndBonus("Who would succumb to the dark powers of an ageless gem containing the soul of an evil wizard-\n(I swear I'm only asking out of a sense of curiosity)", 
                "would succumb to the dark powers of an ageless gem containing the soul of an evil wizard",
                new DecisionBonus[] { new GodDecisionBonus(1, GodEnum.MalevolentGod) }, new DecisionBonus[] { new GodDecisionBonus(1, GodEnum.MercyGod) }, true),
            new PersonalityQuizQuestionAndBonus("Who believes in ghosts?", "believes in ghosts", null, new DecisionBonus[] { new GodDecisionBonus(1, GodEnum.MachineGod) }),
            new PersonalityQuizQuestionAndBonus("Who eats the last donut without asking?", "eats the last donut without asking", new DecisionBonus[] { new PersonalityDecisionBonus(PersonalityFlag.SelfishDonutEater) }, null)
        };

        private static string _neitherMenuActionText = "neither!";
        private static string _bothMenuActionText = "both, obviously";

        public void PersonalityQuiz(HumanFighter fighter1, HumanFighter fighter2, 
            bool randomizeAnswers = false, IChanceService chanceService = null)
        {
            if (randomizeAnswers)
            {
                _output.WriteLine("Alright- in case you were wondering, here's what was generated:\n");
            }

            foreach (PersonalityQuizQuestionAndBonus quizQuestionAndBonus in PersonalityQuizQuestionsAndBonuses)
            {
                List<HumanFighter> selectedFighters = new List<HumanFighter>();

                if (!randomizeAnswers)
                {
                    List<MenuAction> additionalMenuActions = new List<MenuAction>();

                    if (quizQuestionAndBonus.AllowBonusOptions)
                    {
                        additionalMenuActions.Add(new MenuAction(_bothMenuActionText, "both"));
                        additionalMenuActions.Add(new MenuAction(_neitherMenuActionText, "neither"));
                    }

                    FighterSelectionMenu menu = new FighterSelectionMenu(quizQuestionAndBonus.Question, _input, _output, additionalMenuActions.ToArray(),
                        fighter1, fighter2);
                    MenuSelection selection = menu.GetInput();

                    if (quizQuestionAndBonus.AllowBonusOptions && selection.Target == null)
                    {
                        if (selection.Description == _bothMenuActionText)
                        {
                            selectedFighters.Add(fighter1);
                            selectedFighters.Add(fighter2);
                        }
                    }
                    else
                    {
                        HumanFighter selectedFighter = selection.Target as HumanFighter;
                        selectedFighters.Add(selectedFighter);

                        if (selectedFighter == null)
                        {
                            throw new InvalidCastException(
                                "DecisionManager.PersonalityQuiz() was somehow provided with a fighter that wasn't a HumanFighter!");
                        }
                    }
                }
                else
                {
                    if (chanceService == null)
                    {
                        throw new InvalidOperationException("PersonalityQuiz must be supplied a chanceService if randomizeAnswers is set to true");
                    }

                    int numberOptions = quizQuestionAndBonus.AllowBonusOptions ? 4 : 2;

                    int selectedFighterIndex = chanceService.WhichEventOccurs(numberOptions);

                    if (quizQuestionAndBonus.AllowBonusOptions && selectedFighterIndex > 1)
                    {
                        if (selectedFighterIndex == 2)
                        {
                            selectedFighters.Add(fighter1);
                            selectedFighters.Add(fighter2);

                            _output.WriteLine($"both fighters {quizQuestionAndBonus.PersonalityStatement}");
                        }
                        else
                        {
                            _output.WriteLine($"neither fighter {quizQuestionAndBonus.PersonalityStatement}");
                        }
                    }
                    else
                    {
                        HumanFighter selectedFighter = selectedFighterIndex == 0 ? fighter1 : fighter2;
                        selectedFighters.Add(selectedFighter);

                        _output.WriteLine($"{selectedFighter.DisplayName} {quizQuestionAndBonus.PersonalityStatement}");
                    }
                }

                DecisionBonus[] selectedFighterBonus = quizQuestionAndBonus.SelectedPlayerBonuses;

                foreach (DecisionBonus bonus in selectedFighterBonus)
                {
                    selectedFighters.ForEach(f => bonus.ApplyBonus(f, _relationshipManager, true));
                }

                DecisionBonus[] notSelectedFighterBonus = quizQuestionAndBonus.NotSelectedPlayerBonuses;
                List<HumanFighter> notSelectedFighters = new List<HumanFighter>();

                if (!selectedFighters.Contains(fighter1))
                {
                    notSelectedFighters.Add(fighter1);
                }
                if (!selectedFighters.Contains(fighter2))
                {
                    notSelectedFighters.Add(fighter2);
                }

                foreach (DecisionBonus bonus in notSelectedFighterBonus)
                {
                    notSelectedFighters.ForEach(f => bonus.ApplyBonus(f, _relationshipManager, true));
                }
            }

            if (randomizeAnswers)
            {
                _input.WaitAndClear(_output);
            }
        }

        /// <summary>
        /// The keys represent the various Ids in the <see cref="GroupingKeys"/> class,
        /// and the values are which SubRegion was chosen from that group. Can be reset via the 
        /// "Reset grouping choice" method
        /// </summary>
        private readonly Dictionary<int, WorldSubRegion> _groupingChoicesDictionary;

        public T PickNextArea<T, TAreaId>(MapGrouping<T, TAreaId> grouping, Team advancingTeam) where T : Area<TAreaId>
        {
            T ret;

            if (grouping.GroupingId == Globals.GroupingKeys.FirstDesertGroupingId)
            {
                ret = ChooseDesertSubRegions(advancingTeam, grouping as MapGrouping<SubRegion, WorldSubRegion>) as T;
            }
            else if (grouping.GroupingId == Globals.GroupingKeys.MainRegionalMapGroupingId)
            {
                ret = ChooseNextRegion(grouping as MapGrouping<Region, WorldRegion>) as T;
            }
            else
            {
                ret = _chanceService.WhichEventOccurs(grouping.GetAvaialableAreas());
            }

            return ret;
        }

        private Region ChooseNextRegion(MapGrouping<Region, WorldRegion> grouping)
        {
            IEnumerable<MenuAction> menuActions =
                grouping.GetAvaialableAreas().Select(r => new TypedMenuAction<WorldRegion>(r.AreaId, r.AreaId.ToString(), isHidden: r.AreaId == WorldRegion.Casino || r.AreaId == WorldRegion.CrystalCaves)).ToList();

            IMenu menu = _menuFactory.GetMenu(MenuType.NonSpecificMenu, _input, _output, allowHelp: false, allowBack: false, allowStatus: false,
                prompt: "Which region would you like to visit next?", errorText: Globals.GenericErrorMessage,
                menuActions: menuActions.ToList());

            menu.Build(null, null, null, null);
            TypedMenuSelection<WorldRegion> menuSelection = menu.GetInput() as TypedMenuSelection<WorldRegion>;

            if (menuSelection == null)
            {
                throw new InvalidCastException("DecisionManager.ChooseNextRegion() should have generated menus that would return a TypedMenuSeleciton<WorldRegion> but it did not!");
            }

            return grouping.GetAvaialableAreas().First(sr => sr.AreaId == menuSelection.Item);
        }

        private SubRegion ChooseDesertSubRegions(Team advancingTeam, MapGrouping<SubRegion, WorldSubRegion> grouping)
        {
            SubRegion ret;

            if (_groupingChoicesDictionary.ContainsKey(Globals.GroupingKeys.FirstDesertGroupingId))
            {
                WorldSubRegion selectedRegion = _groupingChoicesDictionary[Globals.GroupingKeys.FirstDesertGroupingId];
                ret = grouping.GetAvaialableAreas().First(sr => sr.AreaId == selectedRegion);
            }
            else
            {
                HumanFighter mazeSolver =
                    advancingTeam.Fighters.OfType<HumanFighter>()
                        .First(f => f.PersonalityFlags.Contains(PersonalityFlag.MazeSolver));

                HumanFighter notMazeSolver =
                    advancingTeam.Fighters.OfType<HumanFighter>()
                        .First(f => !f.PersonalityFlags.Contains(PersonalityFlag.MazeSolver));

                IEnumerable<WorldSubRegion> firstGroupingSubRegions =
                    WorldSubRegions.GetSubRegionsByGroupingId(Globals.GroupingKeys.FirstDesertGroupingId);

                IEnumerable<MenuAction> firstGroupingMenuActions =
                    firstGroupingSubRegions.Select(
                        sr => new TypedMenuAction<WorldSubRegion>(sr, GetDisplayTextForMenu(sr), GetCommandTextForMenu(sr), isHidden: sr == WorldSubRegion.Oasis));

                IMenu firstGroupingMenu = _menuFactory.GetMenu(MenuType.NonSpecificMenu, _input, _output,
                    allowHelp: false, allowBack: false, allowStatus: false,
                    prompt:
                    $"{Globals.OwnerReplaceText}: We should focus on forming a strategy for our next battle, we should...",
                    errorText: Globals.GenericErrorMessage,
                    menuActions: firstGroupingMenuActions.ToList(), chanceService: _chanceService, shuffleOptions: true);

                firstGroupingMenu.Build(mazeSolver, null, null, null);
                TypedMenuSelection<WorldSubRegion> firstMenuSelection =
                    firstGroupingMenu.GetInput() as TypedMenuSelection<WorldSubRegion>;

                if (firstMenuSelection == null)
                {
                    throw new InvalidCastException(
                        "DecisionManager.ChooseDesertSubRegions() should have generated menus that would return a TypedMenuSeleciton<WorldSubRegion> but it did not!");
                }

                WorldSubRegion firstSelectedSubRegionEnum = firstMenuSelection.Item;
                grouping.Lock(sr => sr.AreaId != firstSelectedSubRegionEnum);
                _relationshipManager.UpdateRelationship(mazeSolver,
                    WorldSubRegions.GetGodEnumBySubRegion(firstSelectedSubRegionEnum), 1);

                _groupingChoicesDictionary.Add(Globals.GroupingKeys.FirstDesertGroupingId, firstSelectedSubRegionEnum);

                IEnumerable<WorldSubRegion> secondGroupingSubRegions =
                    WorldSubRegions.GetSubRegionsByGroupingId(Globals.GroupingKeys.SecondDesertGroupingId);

                IEnumerable<MenuAction> secondGroupingMenuActions =
                    secondGroupingSubRegions.Select(
                        sr => new TypedMenuAction<WorldSubRegion>(sr, GetDisplayTextForMenu(sr), GetCommandTextForMenu(sr), isHidden: sr == WorldSubRegion.BeastTemple));

                IMenu secondGroupingMenu = _menuFactory.GetMenu(MenuType.NonSpecificMenu, _input, _output,
                    allowHelp: false, allowBack: false, allowStatus: false,
                    prompt: $"{Globals.OwnerReplaceText}: I was thinking about how we could improve, what if we...",
                    errorText: Globals.GenericErrorMessage, menuActions: secondGroupingMenuActions.ToList(),
                    chanceService: _chanceService, shuffleOptions: true);

                secondGroupingMenu.Build(notMazeSolver, null, null, null);
                TypedMenuSelection<WorldSubRegion> secondMenuSelection =
                    secondGroupingMenu.GetInput() as TypedMenuSelection<WorldSubRegion>;
                if (secondMenuSelection == null)
                {
                    throw new InvalidCastException(
                        "DecisionManager.ChooseDesertSubRegions() should have generated menus that would return a TypedMenuSeleciton<WorldSubRegion> but it did not!");
                }

                WorldSubRegion secondSelectedSubRegionEnum = secondMenuSelection.Item;
                grouping.Parent.MapPaths.First(path => path.To.GroupingId == Globals.GroupingKeys.SecondDesertGroupingId)
                    .To.Lock(sr => sr.AreaId != secondSelectedSubRegionEnum);
                _relationshipManager.UpdateRelationship(notMazeSolver,
                    WorldSubRegions.GetGodEnumBySubRegion(secondSelectedSubRegionEnum), 1);

                _groupingChoicesDictionary.Add(Globals.GroupingKeys.SecondDesertGroupingId, secondSelectedSubRegionEnum);

                ret = grouping.GetAvaialableAreas().First(sr => sr.AreaId == firstSelectedSubRegionEnum);
            }

            return ret;
        }

        private static string GetDisplayTextForMenu(WorldSubRegion subRegion)
        {
            string ret = "";

            switch (subRegion)
            {
                case WorldSubRegion.DesertCrypt:
                    ret = "Weaken him, with poison, perhaps (\"weaken\")";
                    break;
                case WorldSubRegion.TavernOfHeroes:
                    ret = "Intimidate him with a show of strength (\"intimidate\")";
                    break;
                case WorldSubRegion.AncientLibrary:
                    ret = "Draw him out of his element, get him off balance (\"off-balance\")";
                    break;
                case WorldSubRegion.CliffsOfAThousandPushups:
                    ret = "Train, then hit him really hard (\"train\")";
                    break;
                case WorldSubRegion.TempleOfDarkness:
                    ret = "Come up with techniques to counter his (\"techniques\")";
                    break;
                case WorldSubRegion.VillageCenter:
                    ret = "Rally those wronged by his selfishness (\"rally\")";
                    break;
            }

            return ret;
        }

        private static string GetCommandTextForMenu(WorldSubRegion subRegion)
        {
            string ret = "";

            switch (subRegion)
            {
                case WorldSubRegion.DesertCrypt:
                    ret = "weaken";
                    break;
                case WorldSubRegion.TavernOfHeroes:
                    ret = "intimidate";
                    break;
                case WorldSubRegion.AncientLibrary:
                    ret = "off-balance";
                    break;
                case WorldSubRegion.CliffsOfAThousandPushups:
                    ret = "train";
                    break;
                case WorldSubRegion.TempleOfDarkness:
                    ret = "techniques";
                    break;
                case WorldSubRegion.VillageCenter:
                    ret = "rally";
                    break;
                case WorldSubRegion.Oasis:
                    ret = "hug";
                    break;
                case WorldSubRegion.BeastTemple:
                    ret = "transform";
                    break;
            }

            return ret;
        }

        public void ResetDecisionsAfterRegionCleared(object sender, RegionCompletedEventArgs e)
        {
            ResetGroupingChoice(e.CompletedRegion.AreaId);
        }

        private void ResetGroupingChoice(WorldRegion region)
        {
            IEnumerable<int> groupingIdsToClear = Globals.GroupingKeys.GetGroupingKeysForRegion(region);

            foreach (int groupingId in groupingIdsToClear)
            {
                _groupingChoicesDictionary.Remove(groupingId);
            }
        }
    }
}
