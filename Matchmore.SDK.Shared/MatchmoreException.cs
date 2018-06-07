using System;
using System.Runtime.Serialization;

namespace Matchmore.SDK
{
    [Serializable]
    public class MatchmoreException : Exception
    {
        public MatchmoreException(string message) : base(message)
        {
        }

        public MatchmoreException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MatchmoreException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}