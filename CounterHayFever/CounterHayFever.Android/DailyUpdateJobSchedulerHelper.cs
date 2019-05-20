using System;
using Android.App.Job;
using Android.Content;

namespace CounterHayFever.Droid
{
    public static class DailyUpdateJobSchedulerHelper
    {
        /// <summary>
        /// Creates the job builder using job identifier.
        /// </summary>
        /// <returns>The job builder using job identifier.</returns>
        /// <param name="context">Context.</param>
        /// <param name="jobId">Job identifier.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static JobInfo.Builder CreateJobBuilderUsingJobId<T>(this Context context, int jobId) where T : JobService
        {
            var javaClass = Java.Lang.Class.FromType(typeof(T));
            var componentName = new ComponentName(context, javaClass);
            return new JobInfo.Builder(jobId, componentName);
        }
    }
}
