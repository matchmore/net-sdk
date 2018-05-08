using System;
using System.Collections.Generic;
using Matchmore.SDK;

public class MatchReceivedEventArgs : EventArgs
{
    private Device _device;
    private MatchChannel _channel;
    private List<Match> _matches;

    public MatchReceivedEventArgs(Device device, MatchChannel channel, List<Match> matches)
    {
        _device = device;
        _channel = channel;
        _matches = matches;
    }
    public Device Device
    {
        get
        {
            return _device;
        }

        private set
        {
            _device = value;
        }
    }

    public MatchChannel Channel
    {
        get
        {
            return _channel;
        }

        private set
        {
            _channel = value;
        }
    }

    public List<Match> Matches
    {
        get
        {
            return _matches;
        }

        private set
        {
            _matches = value;
        }
    }
}