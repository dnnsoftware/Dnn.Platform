// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Common.Utilities.Internal;
    using NUnit.Framework;

    [TestFixture]
    public class RetryableActionTests
    {
        private SleepMonitor _sleepMonitor;

        [SetUp]
        public void Setup()
        {
            this._sleepMonitor = new SleepMonitor();
            RetryableAction.SleepAction = this._sleepMonitor.GoToSleep;
        }

        [Test]
        public void ActionSucceedsFirstTime()
        {
            var monitor = new ActionMonitor();
            var retryable = CreateRetryable(monitor.Action);

            retryable.TryIt();

            Assert.AreEqual(1, monitor.TimesCalled);
        }

        [Test]
        public void ActionFailsTwice()
        {
            var monitor = new ActionMonitor(2);
            var retryable = CreateRetryable(monitor.Action);

            retryable.TryIt();

            Assert.AreEqual(3, monitor.TimesCalled);
        }

        [Test]
        public void DelaysIncreaseWithEachRetry()
        {
            var monitor = new ActionMonitor(3);
            var retryable = CreateRetryable(monitor.Action, 10);

            retryable.TryIt();

            var firstRetry = this._sleepMonitor.SleepPeriod[0];
            var secondRetry = this._sleepMonitor.SleepPeriod[1];
            var thirdRetry = this._sleepMonitor.SleepPeriod[2];

            Assert.AreEqual(5, firstRetry);
            Assert.AreEqual(50, secondRetry);
            Assert.AreEqual(500, thirdRetry);
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void ActionNeverSucceeds()
        {
            var monitor = new ActionMonitor(-1);
            var retryable = CreateRetryable(monitor.Action);

            retryable.TryIt();
        }

        private static RetryableAction CreateRetryable(Action action)
        {
            return CreateRetryable(action, 1);
        }

        private static RetryableAction CreateRetryable(Action action, int factor)
        {
            return new RetryableAction(action, "foo", 10, TimeSpan.FromMilliseconds(5), factor);
        }
    }

    internal class SleepMonitor
    {
        private readonly List<int> _periods = new List<int>();

        public IList<int> SleepPeriod
        {
            get
            {
                return this._periods.AsReadOnly();
            }
        }

        public void GoToSleep(int delay)
        {
            this._periods.Add(delay);
        }
    }

    internal class ActionMonitor
    {
        private readonly List<DateTime> _callTimes = new List<DateTime>();
        private int _failuresRemaining;

        public ActionMonitor()
            : this(0)
        {
        }

        public ActionMonitor(int failureCount)
        {
            this._failuresRemaining = failureCount;
        }

        public IList<DateTime> CallTime
        {
            get
            {
                return this._callTimes.AsReadOnly();
            }
        }

        public int TimesCalled { get; private set; }

        public void Action()
        {
            this._callTimes.Add(DateTime.Now);
            this.TimesCalled++;

            if (this._failuresRemaining != 0)
            {
                if (this._failuresRemaining > 0)
                {
                    this._failuresRemaining--;
                }

                throw new Exception("it failed");
            }
        }
    }
}
