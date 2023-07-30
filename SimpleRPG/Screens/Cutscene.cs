using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle.Fighters;

namespace SimpleRPG.Screens
{
    public abstract class CutsceneComponent
    {
    }

    public class SingleScene : CutsceneComponent
    {
        protected List<ColorString> Lines { get; set; }

        public SingleScene(params ColorString[] lines)
        {
            Lines = new List<ColorString>();
            Lines.AddRange(lines.ToList());
        }

        public void Print(IInput input, IOutput output, HumanFighter fighter1, HumanFighter fighter2)
        {
            foreach (ColorString line in Lines)
            {
                output.WriteLine(line);
            }

            input.WaitAndClear(output);
        }
    }
    
    //TODO: rename, move into its own file
    public class FooEventArgs : EventArgs
    {
        public int GroupingId { get; }

        public FooEventArgs(int groupingId)
        {
            GroupingId = groupingId;
        }
    }

    public class DecisionScene : CutsceneComponent
    {
        public EventHandler<FooEventArgs> Fooed { get; set; }

        public void OnFoo(FooEventArgs e)
        {
            Fooed?.Invoke(this, e);
        }

        public int GroupingId { get; }

        public DecisionScene(int groupingId)
        {
            GroupingId = groupingId;
        }

        public void Decide()
        {
            OnFoo(new FooEventArgs(GroupingId));
        }
    }

    public class Cutscene
    {
        protected List<CutsceneComponent> AllScenes { get; set; }

        public EventHandler<FooEventArgs> Fooed { get; set; }

        public Cutscene(params CutsceneComponent[] sceneComponents)
        {
            AllScenes = new List<CutsceneComponent>();

            AllScenes.AddRange(sceneComponents.ToList());
        }

        public void ExecuteCutscene(IInput input, IOutput output, HumanFighter fighter1, HumanFighter fighter2)
        {
            foreach (CutsceneComponent component in AllScenes)
            {
                (component as SingleScene)?.Print(input, output, fighter1, fighter2);

                DecisionScene decisionScene = component as DecisionScene;

                if (decisionScene != null)
                {
                    decisionScene.Fooed += PropagateFooEvent;
                    decisionScene.Decide();
                }
            }
        }

        private void PropagateFooEvent(object sender, FooEventArgs e)
        {
            Fooed?.Invoke(sender, e);
        }
    }
}
