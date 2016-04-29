using System;
using UnityEngine.Events;

namespace PhySigTK
{
	[Serializable]
	public class TimeStampedDataEvent : UnityEvent<TimeStampedFloatList> { }
	[Serializable]
	public class StatusChangeEvent : UnityEvent<string> { }
}
