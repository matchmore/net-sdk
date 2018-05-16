using System;
using System.Collections.Generic;
using Matchmore.SDK;

public class MatchReceivedEventArgs : EventArgs
{
    public MatchReceivedEventArgs(Device device, MatchChannel channel, List<Match> matches)
    {
		Device = device;
		Channel = channel;
		Matches = matches;
    }
    public Device Device { get; private set; }

    public MatchChannel Channel { get; private set; }

    public List<Match> Matches { get; private set; }
}