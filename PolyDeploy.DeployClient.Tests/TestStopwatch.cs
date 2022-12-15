namespace PolyDeploy.DeployClient.Tests;
using System;
using System.Collections.Generic;
using Shouldly;

public class TestStopwatch : IStopwatch
{
    private readonly IReadOnlyList<TimeSpan> timeSpans;
    
    public TestStopwatch(params TimeSpan[] timeSpans)
    {
        this.timeSpans = timeSpans;
    }

    private int elapsedCalled;

    public TimeSpan Elapsed
    {
        get
        {
            this.IsStartNewCalled.ShouldBeTrue();

            if (this.timeSpans.Count == 0)
            {
                return TimeSpan.Zero;
            }

            return elapsedCalled >= this.timeSpans.Count 
                ? this.timeSpans[^0] 
                : timeSpans[elapsedCalled++];
        }
    }

    public bool IsStartNewCalled { get; private set; }

    public void StartNew()
    {
        this.IsStartNewCalled = true;
    }
}
