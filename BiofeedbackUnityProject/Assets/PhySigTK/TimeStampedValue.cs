namespace PhySigTK
{
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
}
