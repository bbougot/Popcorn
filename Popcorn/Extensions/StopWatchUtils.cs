using System;
using System.Diagnostics;

namespace Popcorn.Extensions
{
    public static class StopWatchUtils
    {
        /// <summary>
        /// Gets estimated time on compleation. 
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="counter"></param>
        /// <param name="counterGoal"></param>
        /// <returns></returns>
        public static TimeSpan GetEta(this Stopwatch sw, long counter, long counterGoal)
        {
            /* this is based off of:
             * (TimeTaken / linesProcessed) * linesLeft=timeLeft
             * so we have
             * (10/100) * 200 = 20 Seconds now 10 seconds go past
             * (20/100) * 200 = 40 Seconds left now 10 more seconds and we process 100 more lines
             * (30/200) * 100 = 15 Seconds and now we all see why the copy file dialog jumps from 3 hours to 30 minutes :-)
             * 
             * pulled from http://stackoverflow.com/questions/473355/calculate-time-remaining/473369#473369
             */
            if (counter == 0) return TimeSpan.Zero;
            float elapsedMin = ((float)sw.ElapsedMilliseconds / 1000) / 60;
            float minLeft = (elapsedMin / counter) * (counterGoal - counter); //see comment a
            TimeSpan ret = TimeSpan.FromMinutes(minLeft);
            return ret;
        }
    }
}
