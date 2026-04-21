using System;
using System.Threading.Tasks;

namespace KolayCAR.Broker.Infrastructure.Extensions
{
    public static class TaskExtensions
    {
        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, int? millisecondsTimeout)
        {
            try
            {
                var typeName = typeof(TResult).FullName;
                if (millisecondsTimeout != null && millisecondsTimeout != -1)
                {
                    if (task == await Task.WhenAny(task, Task.Delay(millisecondsTimeout.ToIntNullSafe())))
                        return await task;
                    else
                        return default(TResult);
                }
                else if (millisecondsTimeout == -1)
                {
                    if (task == await Task.WhenAny(task, Task.Delay(20000)))
                        return await task;
                    else
                        return default(TResult);
                }

                return await task;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@TaskExtensionsEx}", ex.Message);
                return default(TResult);
            }
        }

        public static T TimeoutAfterNew<T>(Func<T> TaskAction, int TimeoutSeconds)
        {
            Task<T> backgroundTask;
            try
            {
                backgroundTask = Task.Factory.StartNew(TaskAction);
                backgroundTask.Wait(new TimeSpan(0, 0, TimeoutSeconds));
            }
            catch (AggregateException ex)
            {
                // task failed
                var failMessage = ex.Flatten().InnerException.Message;
                return default(T);
            }
            catch (Exception ex)
            {
                // task failed
                var failMessage = ex.Message;
                return default(T);
            }

            if (!backgroundTask.IsCompleted)
            {
                // task timed out
                return default(T);
            }

            // task succeeded
            return backgroundTask.Result;
        }
    }
}
