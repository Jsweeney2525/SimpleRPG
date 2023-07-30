using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Enums;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Screens;
using SimpleRPG.Battle.BattleManager;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Helpers;
using SimpleRPG.Regions;
using SimpleRPG.Screens.Menus;
using SimpleRPG.Screens.Menus.MenuActions;
using SimpleRPG.Screens.Menus.MenuSelections;

namespace SimpleRPG
{
    class Program
    {
        private class FakeBattleManager : BattleManager
        {
            public FakeBattleManager(IChanceService chanceService, IInput input, IOutput output) 
                : base(chanceService, input, output)
            {
            }

            public void ClearDanceEffects()
            {
                Config.EnemyDanceEffects.Clear();
                Config.HumanDanceEffects.Clear();
            }

            public void HealEveryone()
            {
                foreach(var fighter in _enemyTeam.Fighters)
                {
                    fighter.Revive(fighter.MaxHealth);
                    fighter.Heal(fighter.MaxHealth);
                }

                foreach (var fighter in _humanTeam.Fighters)
                {
                    fighter.Revive(fighter.MaxHealth);
                    fighter.Heal(fighter.MaxHealth);
                }
            }

            public new void ExecuteMove(BattleMoveWithTarget move)
            {
                base.ExecuteMove(move);
            }
        }

        private static bool Foo()
        {
            SelectEnemyTeamMenuManager enemyTeamGenerator = new SelectEnemyTeamMenuManager(Globals.Input, Globals.Output);

            BattleConfigurationSpecialFlag battleFlag;

            Team enemyTeam = enemyTeamGenerator.GetTeam(new MenuManager(Globals.Input, Globals.Output, Globals.MenuFactory), Globals.Input, Globals.Output, out battleFlag);

            NumberInputMenu levelInputMenu = new NumberInputMenu("What levels will your fighters be (min 1. Max 5)?", Globals.Input, Globals.Output, 1, 5);
            NumberInputMenuSelection levelInput = levelInputMenu.GetInput() as NumberInputMenuSelection;
            if (levelInput == null)
            {
                throw new Exception("something went terribly wrong, a NumberInputMenu did not return a NumberInputMenuSelection");
            }
            int level = levelInput.Number;

            var playerOne = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, level, "Dante");
            var playerTwo = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, level, "Arrokoh");
            var playerTeam = new Team(new MenuManager(Globals.Input, Globals.Output, Globals.MenuFactory), playerOne, playerTwo);

            BattleMove doNothing = MoveFactory.Get(BattleMoveType.DoNothing);
            BattleMove feintAttack = MoveFactory.Get(BattleMoveType.Attack, "feint");
            BattleMove shieldBuster = MoveFactory.Get(BattleMoveType.ShieldBuster);
            
            foreach (HumanFighter fighter in playerTeam.Fighters)
            {
                fighter.AddMove(doNothing);
                fighter.AddMove(feintAttack);
                fighter.AddMove(shieldBuster);
            }

            FakeBattleManager manager = new FakeBattleManager(Globals.ChanceService, Globals.Input, Globals.Output);

            manager.Battle(playerTeam, enemyTeam, config: new BattleManagerBattleConfiguration { SpecialBattleFlag = battleFlag });

            return true;
        }

        static void Main()
        {
            FighterFactory.SetGodRelationshipManager(Globals.GodRelationshipManager);
            EventHandlerPrinter printer = new EventHandlerPrinter(Globals.Output);

            const string testBattleOptionText = "Test Battle ('test')";
            const string campaignOptionText = "Play the \"Campaign\" mode ('campaign')";
            //const string demoOptionText = "Play the \"Demo\" mode (fewer battles per region)";

            List<MenuAction> menuActions = new List<MenuAction>
                {
                    new MenuAction(testBattleOptionText, "test"),
                    new MenuAction(campaignOptionText, "campaign"),
                    new MenuAction("exit")
                };

            Menu selectGameplayModeMenu = new Menu(false, false, false, "What mode would you like to play?", null,
                null, menuActions, Globals.Input, Globals.Output);

            bool continuer = true;

            RegionManager regionManager;

            while (continuer)
            {
                Globals.Output.ClearScreen();
                regionManager = Globals.RegionManager;
                
                selectGameplayModeMenu.Build(null, null, null, null);
                MenuSelection gameplayModeSelection = selectGameplayModeMenu.GetInput();

                switch (gameplayModeSelection.Description)
                {
                    case testBattleOptionText:
                        Foo();
                        break;
                    case campaignOptionText:
                        TutorialScreens();

                        SetupPlayers(out var playerOne, out var playerTwo, printer);

                        MenuManager menuManager = new MenuManager(Globals.Input, Globals.Output, Globals.MenuFactory);
                        var playerTeam = new Team(menuManager, playerOne, playerTwo);
                        BattleManager manager = new BattleManager(Globals.ChanceService, Globals.Input, Globals.Output);

                        Globals.RegionManager.Battle(manager, playerTeam);

                        break;
                    case "exit":
                        continuer = false;
                        break;

                }
            }
        }

        private static void TutorialScreens()
        {
            var tutorialMenu = Globals.MenuFactory.GetMenu(MenuType.TutorialMenu);
            tutorialMenu.Build(null, null, null, null);
            var tutorial = tutorialMenu.GetInput();

            if (tutorial.Description == "yes")
            {
                List<ColorString[]> sceneLines = new List<ColorString[]>
                {
                    new[]
                    {
                        new ColorString("Right now, you're playing a very simple version of SimpleRPG."),
                        new ColorString("Your team is 2 players, with no means of customizing their stats or abilities."),
                        new ColorString("the basic run down of stats (none of which are original on any level):\n"),
                        new ColorString(new ColorString("Strength", ConsoleColor.Cyan), ": raises damage done"),
                        new ColorString(new ColorString("Defense", ConsoleColor.Cyan), ": lowers damage taken"),
                        new ColorString(new ColorString("Speed", ConsoleColor.Cyan), ": each round, the faster fighters move first-"),
                        new ColorString("(note, however, some moves may have a higher 'priority' which has higher precedence over speed)"),
                        new ColorString(new ColorString("HP", ConsoleColor.Cyan), ": your health points. If this hits 0 the fighter cannot act")
                    },
                    new[]
                    {
                        new ColorString("The battle menu will present you with 3 options"),
                        new ColorString("but... actually, 'item' currently does not work"),
                        new ColorString("And the only method of fighting available to you immediately is a basic attack"),
                        new ColorString("But other methods of combat will open up as you progress, promise!")
                    },
                    new[]
                    {
                        new ColorString("If you run from a battle, or both fighters are defeated, that's game over"),
                        new ColorString("If you emerge victorious, your characters will be fully healed for the next battle"),
                        new ColorString("Right now \"winning\" is defeating the second boss, and the story is linear."),
                        new ColorString("That will be updated in the future")
                    },
                    new[]
                    {
                        new ColorString("The gameplay is meant to focus on strategy,"),
                        new ColorString("rather than playing a game of \"highest numbers\" where better stats are the key to victory"),
                        new ColorString("Try and also keep in mind the \"personality\" of your two players-"),
                        new ColorString("That may or may not play a bigger part in the game in later iterations")
                    },
                    new[]
                    {
                        new ColorString("Also when selecting from a numbered list, you can use either the number or the name of the item."),
                        new ColorString("So if the first option was \"1. Goblin\", both \"1\" and \"goblin\" are valid-"),
                        new ColorString("and input is not case sensitive."),
                        new ColorString("And some menus allow for shortcuts (e.g. \"y\" in place of \"yes\"")
                    }
                };

                Cutscene tutorialCutscene = new Cutscene(sceneLines.Select(sl => new SingleScene(sl)).ToArray());
                tutorialCutscene.ExecuteCutscene(Globals.Input, Globals.Output, null, null);
            }
        }

        /// <summary>
        /// Handles player names and personality quiz stuff
        /// </summary>
        /// <param name="playerOne"></param>
        /// <param name="playerTwo"></param>
        /// <param name="printer"></param>
        private static void SetupPlayers(out HumanFighter playerOne, out HumanFighter playerTwo, EventHandlerPrinter printer)
        {
            string playerOneName = GetName(1);
            string playerTwoName = GetName(2, playerOneName);

            playerOne = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1, playerOneName);
            playerTwo = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1, playerTwoName);
            Globals.Output.ClearScreen();
            
            printer.Subscribe(playerOne, playerTwo);

            Globals.DecisionManager.AssignNameBonuses(playerOne, playerTwo);

            ConfirmationMenu takePersonalityQuizConfirmationMenu = new ConfirmationMenu(false, "To better get to know your characters, would you like to fill out a short personality questionaire?\n(if you decline, the answers will be asigned randomly)\n", Globals.Input, Globals.Output);
            MenuSelection takePersonalityQuizSelection = takePersonalityQuizConfirmationMenu.GetInput();

            Globals.DecisionManager.PersonalityQuiz(playerOne, playerTwo, takePersonalityQuizSelection.Description == "no", Globals.ChanceService);
        }

        /// <summary>
        /// Prompts the user to input a name for their character.
        /// </summary>
        /// <param name="playerNum">The 1-based index representing which player is being named</param>
        /// <param name="playerOneName"> supplied when prompting for the 2nd player name, so that way duplicates can properly be caught and disallowed</param>
        /// <returns>The name for the player</returns>
        private static string GetName(int playerNum, string playerOneName = null)
        {
            string prompt;
            string disallowedErrorMessage;

            switch (playerNum)
            {
                default:
                    prompt = "Please enter a name for your first fighter (max 20 characters)";
                    disallowedErrorMessage = null;
                    break;
                case 2:
                    prompt = "Please enter a name for your second fighter (max 20 characters)";
                    disallowedErrorMessage = "duplicate names are not allowed";
                    break;
            }

            NameInputMenu nameMenu = new NameInputMenu(Globals.Input, Globals.Output);

            string name = playerOneName == null? nameMenu.GetName(prompt, disallowedErrorMessage) : nameMenu.GetName(prompt, disallowedErrorMessage, playerOneName);

            return name;
        }
    }
}
