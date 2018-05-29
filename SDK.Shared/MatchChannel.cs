using System;

namespace Matchmore.SDK
{
	[Flags]
    public enum MatchChannel
    {
		Polling = 1, 
		Websocket = 2,
        ThirdParty = 4
    }
}