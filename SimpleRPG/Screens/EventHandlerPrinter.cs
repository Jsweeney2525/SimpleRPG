using SimpleRPG.Battle.Fighters;
using SimpleRPG.Events;

namespace SimpleRPG.Screens
{
    public class EventHandlerPrinter
    {
        private IOutput _output;

        public EventHandlerPrinter(IOutput output)
        {
            _output = output;
        }

        public void Subscribe(params HumanFighter[] fighters)
        {
            foreach (HumanFighter fighter in fighters)
            {
                fighter.StatBonusApplied += PrintStatBoostMessage;
                fighter.MagicBonusApplied += PrintMagicBoostMessage;
            }
        }

        private void PrintStatBoostMessage(object sender, StatBonusAppliedEventArgs e)
        {
            if (!e.IsSecretStatBonus)
            {
                IFighter senderAsFighter = sender as IFighter;

                if (senderAsFighter == null)
                {
                    return;
                }

                _output.WriteLine($"{senderAsFighter.DisplayName} gained +{e.BonusAmount} {e.Stat.ToString().ToLower()}");
            }
        }

        private void PrintMagicBoostMessage(object sender, MagicBonusAppliedEventArgs e)
        {
            if (!e.IsSecretStatBonus)
            {
                IFighter senderAsFighter = sender as IFighter;

                if (senderAsFighter == null)
                {
                    return;
                }

                _output.WriteLine($"{senderAsFighter.DisplayName} gained +{e.BonusAmount} {e.MagicType.ToString().ToLower()} magic {e.MagicStatType.ToString().ToLower()}");
            }
        }
    }
}
