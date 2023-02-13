using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Devices
{
    /// <summary>
    /// Umgang mit StopWatch
    /// </summary>
    public static class TicksUtils
    {
        public static void TicksReset(Stopwatch sw)
        {
            sw.Restart();
        }

        public static long TicksDelayed(Stopwatch sw)
        {
            return sw.ElapsedMilliseconds;
        }

        public static long TicksRestTime(Stopwatch sw, long interval)
        {
            return Math.Max(0, interval - TicksDelayed(sw));
        }

        public static bool TicksCheck(Stopwatch sw, long interval)
        {
            bool result = TicksDelayed(sw) > interval;
            if (result)
                TicksReset(sw);
            return result;
        }
    }


    /// <summary>
    /// NonSystemTimer in eigenem Thread:
    /// A Timer that execute an async callback after a due time and repeating it after a period (interval).
    /// Callback is a non reentrant timer (ie. callbacks never overlaps). Dispose method wait for the current callback to finish.
    /// Timer can be cancelled using a CancellationToken or by calling StopAsync or by calling Dispose.
    /// Exception inside callbacks are ignored and just traced.
    /// Callback is invoked at each interval (period) after the end of the previous invocation.
    /// </summary>
    public sealed class TimerAsync : IDisposable
    {
        private readonly Func<CancellationToken, Task> _callback;
        private readonly object _changeLock = new();
        private TimeSpan _dueTime;
        private TimeSpan _period;
        private CancellationTokenSource _cancellationSource;
        private Task? _timerTask;

        public TimerAsync(Func<CancellationToken, Task> callback, TimeSpan dueTime, TimeSpan period)
            : this(callback, dueTime, period, CancellationToken.None)
        {
        }

        public TimerAsync(Func<CancellationToken, Task> callback, TimeSpan dueTime, TimeSpan period, CancellationToken token)
        {
            _callback = callback;
            _dueTime = dueTime;
            _period = period;
            _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(token);

            Start();
        }

        public Task ChangeAsync(TimeSpan dueTime, TimeSpan period)
        {
            return ChangeAsync(dueTime, period, CancellationToken.None);
        }
        public async Task ChangeAsync(TimeSpan dueTime, TimeSpan period, CancellationToken token)
        {
            await StopAsync()
                .ConfigureAwait(false);

            lock (_changeLock)
            {
                _dueTime = dueTime;
                _period = period;
                _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(token);

                Start();
            }
        }

        public async Task StopAsync()
        {
            _cancellationSource.Cancel();

            try
            {
                if (_timerTask == null)
                    throw new ArgumentNullException("_timerTask", "StopAsync");
                await _timerTask
                        .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Cancelled requested, just ignore it...
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError(e.ToString());
            }
        }

        public void Dispose()
        {
            StopAsync()
                .Wait();
        }

        private void Start()
        {
            _timerTask = IsRecurring
                ? StartRecurringAsync(_cancellationSource.Token)
                : StartOneTimeAsync(_cancellationSource.Token);
        }

        private Task StartRecurringAsync(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                await Task.Delay(_dueTime, cancellationToken)
                    .ConfigureAwait(false);

                while (!cancellationToken.IsCancellationRequested)
                {
                    await RunCallback(cancellationToken)
                        .ConfigureAwait(false);

                    await Task.Delay(_period, cancellationToken)
                        .ConfigureAwait(false);
                }
            }, cancellationToken);
        }

        private Task StartOneTimeAsync(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                await Task.Delay(_dueTime, cancellationToken)
                        .ConfigureAwait(false);

                await RunCallback(cancellationToken)
                    .ConfigureAwait(false);

            }, cancellationToken);
        }

        private async Task RunCallback(CancellationToken cancellationToken)
        {
            try
            {
                await _callback(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Cancelled requested, just ignore it...
                throw;
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError(e.ToString());
            }
        }

        private bool IsRecurring => _period.TotalMilliseconds > 0;
    }
}
