// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Utilities.Internal
{
    using System;
    using System.Threading;

    using DotNetNuke.Instrumentation;

    /// <summary>
    /// Allows an action to be run and retried after a delay when an exception is thrown.
    /// <remarks>If the action never succeeds the final exception will be re-thrown for the caller to catch.</remarks>
    /// </summary>
    public class RetryableAction
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RetryableAction));

        public RetryableAction(Action action, string description, int maxRetries, TimeSpan delay)
            : this(action, description, maxRetries, delay, 1)
        {
        }

        public RetryableAction(Action action, string description, int maxRetries, TimeSpan delay, float delayMultiplier)
        {
            if (delay.TotalMilliseconds > int.MaxValue)
            {
                throw new ArgumentException(string.Format("delay must be less than {0} milliseconds", int.MaxValue));
            }

            this.Action = action;
            this.Description = description;
            this.MaxRetries = maxRetries;
            this.Delay = delay;
            this.DelayMultiplier = delayMultiplier;
        }

        static RetryableAction()
        {
            SleepAction = GoToSleep;
        }

        /// <summary>
        /// Gets or sets method that allows thread to sleep until next retry meant for unit testing purposes.
        /// </summary>
        public static Action<int> SleepAction { get; set; }

        /// <summary>
        /// Gets or sets the Action to execute.
        /// </summary>
        public Action Action { get; set; }

        /// <summary>
        /// Gets or sets a message describing the action.  The primary purpose is to make the action identifiable in the log output.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of retries to attempt.
        /// </summary>
        public int MaxRetries { get; set; }

        /// <summary>
        /// Gets or sets the number of milliseconds to wait between retries.
        /// <remarks>The delay period is approximate and will be affected by the demands of other threads on the system.</remarks>
        /// </summary>
        public TimeSpan Delay { get; set; }

        /// <summary>
        /// Gets or sets a factor by which the delay is adjusted after each retry.  Default is 1.
        /// <remarks>To double the delay with every retry use a factor of 2, retrys will be 1s, 2s, 4s, 8s...</remarks>
        /// <remarks>To quarter the delay with every retry use a factor of 0.25, retrys will be 1s, 0.25, 0.0625, 0.015625s...</remarks>
        /// </summary>
        public float DelayMultiplier { get; set; }

        public static void RetryEverySecondFor30Seconds(Action action, string description)
        {
            new RetryableAction(action, description, 30, TimeSpan.FromSeconds(1)).TryIt();
        }

        public static void Retry5TimesWith2SecondsDelay(Action action, string description)
        {
            new RetryableAction(action, description, 5, TimeSpan.FromSeconds(2)).TryIt();
        }

        public void TryIt()
        {
            var currentDelay = (int)this.Delay.TotalMilliseconds;
            int retrysRemaining = this.MaxRetries;

            do
            {
                try
                {
                    this.Action();
                    if (Logger.IsTraceEnabled)
                    {
                        Logger.TraceFormat("Action succeeded - {0}", this.Description);
                    }

                    return;
                }
                catch (Exception)
                {
                    if (retrysRemaining <= 0)
                    {
                        Logger.WarnFormat("All retries of action failed - {0}", this.Description);
                        throw;
                    }

                    if (Logger.IsTraceEnabled)
                    {
                        Logger.TraceFormat("Retrying action {0} - {1}", retrysRemaining, this.Description);
                    }

                    SleepAction.Invoke(currentDelay);

                    const double epsilon = 0.0001;
                    if (Math.Abs(this.DelayMultiplier - 1) > epsilon)
                    {
                        currentDelay = (int)(currentDelay * this.DelayMultiplier);
                    }
                }

                retrysRemaining--;
            }
            while (true);
        }

        private static void GoToSleep(int delay)
        {
            Thread.Sleep(delay);
        }
    }
}
