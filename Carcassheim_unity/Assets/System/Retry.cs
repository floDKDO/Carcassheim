using System;
using System.Collections.Generic;
using System.Threading;

/// <summary>
///     Class involving methods used when attempting to use a method multiple times.
/// </summary>
public static class Retry
{
    /// <summary>
    ///     Attempt to execute a specific action, for a specific number of times with a specific interval
    ///     of time.
    /// </summary>
    /// <param name="action">Action to attempt.</param>
    /// <param name="retryInterval">Interval of time <see cref="TimeSpan" /> between each try.</param>
    /// <param name="maxAttemptCount">Number of attempts.</param>
    public static void Do(
        Action action,
        TimeSpan retryInterval,
        int maxAttemptCount = 3)
        => Do(() =>
        {
            action();
            return new object();
        }, retryInterval, maxAttemptCount);

    /// <summary>
    ///     Attempt to execute a specific action, for a specific number of times with a specific interval
    ///     of time.
    /// </summary>
    /// <param name="action">Action to attempt.</param>
    /// <param name="retryInterval">Interval of time <see cref="TimeSpan" /> between each try.</param>
    /// <param name="maxAttemptCount">Number of attempts.</param>
    /// <typeparam name="T">Type of the value returned by the attempted action.</typeparam>
    /// <returns>The return value of the attempted action.</returns>
    /// <exception cref="AggregateException"></exception>
    public static T Do<T>(Func<T> action, TimeSpan retryInterval, int maxAttemptCount = 3)
    {
        var exceptions = new List<Exception>();

        for (var attempted = 0; attempted < maxAttemptCount; attempted++)
        {
            try
            {
                if (attempted > 0)
                {
                    Thread.Sleep(retryInterval);
                }

                return action();
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }

        throw new AggregateException(exceptions);
    }
}
