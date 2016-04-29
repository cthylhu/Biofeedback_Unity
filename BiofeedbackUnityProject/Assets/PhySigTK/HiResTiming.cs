using System;
using System.Diagnostics;

namespace PhySigTK
{
	/// <summary>
	/// Provide high-frequency timestamps and methods to relate it to real wall clock time.
	/// </summary>
	public static class HiResTiming
	{
		public static DateTime StartTime = DateTime.Now;
		private static Stopwatch _stopwatch = Stopwatch.StartNew();

		public static long CurrentTimeStamp {
			get
			{
				return _stopwatch.Elapsed.Ticks;
			}
		}

		public static DateTime getDateTimeForTimestamp(long timeStamp)
		{
			return StartTime.AddTicks(timeStamp);
		}
	}
}
