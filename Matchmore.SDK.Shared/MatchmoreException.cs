using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
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

    [Serializable]
    public class MatchmoreMultiException : Exception
    {
        public List<Exception> Exceptions { get; } = new List<Exception>();
        public MatchmoreMultiException()
        {

        }
        protected MatchmoreMultiException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}