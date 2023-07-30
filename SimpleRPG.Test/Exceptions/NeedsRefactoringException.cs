using System;
using System.Runtime.Serialization;

namespace SimpleRPG.Test.Exceptions
{
    public class NeedsRefactoringException : NotImplementedException
    {
        public NeedsRefactoringException() : base()
        {
        }

        public NeedsRefactoringException(string message) : base(message)
        {
        }

        public NeedsRefactoringException(string message, Exception inner) : base(message, inner)
        {
        }

        public NeedsRefactoringException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
