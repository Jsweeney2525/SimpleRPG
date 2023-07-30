using System;

namespace SimpleRPG.Events
{
    public class FighterTransformedEventArgs : EventArgs
    {
        public string PreTransformDisplayName { get; }

        public string PostTransformDisplayName { get; }

        public FighterTransformedEventArgs(string preTransformDisplayName, string postTransformDisplayName)
        {
            PreTransformDisplayName = preTransformDisplayName;
            PostTransformDisplayName = postTransformDisplayName;
        }
    }
}
