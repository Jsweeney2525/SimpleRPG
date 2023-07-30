namespace SimpleRPG.Screens
{
    public interface IInput
    {
        string ReadInput();

        void WaitOnEnterKey();

        void WaitAndClear(IOutput output);
    }
}
