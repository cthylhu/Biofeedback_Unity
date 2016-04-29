using System;
using System.Collections.Generic;

namespace PhySigTK
{
	[Serializable]
	public struct TimeStampedValue<T>
	{
		public TimeStampedValue(long timeStamp, T value)
		{
			TimeStamp = timeStamp;
			Value = value;
		}

		public readonly long TimeStamp;

		public readonly T Value;
	}

	[Serializable]
	public class TimeStampedFloatList : List<TimeStampedValue<float>> { };
}
