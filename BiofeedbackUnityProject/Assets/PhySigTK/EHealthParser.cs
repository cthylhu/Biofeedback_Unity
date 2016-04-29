using System.Collections.Generic;

namespace PhySigTK
{
	/// <summary>
	/// Parses strings read from an ehealth arduino. A subset of the values read can be used.
	/// The expected format is: ECG0.1|SCL0.5|EMG100
	/// as provided by the arduino script eHealthArduino4PhySigTK.ino
	/// </summary>
	public class EHealthParser
	{
		private List<TimeStampedValue<float>> NewEMGValues = new List<TimeStampedValue<float>>();
		private List<TimeStampedValue<float>> NewSCLValues = new List<TimeStampedValue<float>>();
		private List<TimeStampedValue<float>> NewECGValues = new List<TimeStampedValue<float>>();

		// previous format for cathy: B,1.76,0.01,-1.00,0
		// B,ecg,air,gsr,hr
		// previous format for Jal: bpm spo2 gsr emg (all space-separated)
		// format now: ECG0.1|SCL0.5|EMG100
		public void ParseValues(TimeStampedValue<string>[] values)
		{
			foreach (TimeStampedValue<string> val in values) {
				long timeStamp = val.TimeStamp;
				foreach (string datapoint in val.Value.Split('|')) {
					if (datapoint.StartsWith("EMG")) {
						int emgInt = int.Parse(datapoint.Substring(3));
						// should be an integer smaller than 1024, map to 1-5V float
						float emgFloat = 5.0f * (emgInt / 1024.0f);
						NewEMGValues.Add(new TimeStampedValue<float>(timeStamp, emgFloat));
					} else if (datapoint.StartsWith("ECG")) {
						float ecgFloat = float.Parse(datapoint.Substring(3));
						NewECGValues.Add(new TimeStampedValue<float>(timeStamp, ecgFloat));
					} else if (datapoint.StartsWith("SCL")) {
						float sclFloat = float.Parse(datapoint.Substring(3));
						NewSCLValues.Add(new TimeStampedValue<float>(timeStamp, sclFloat));
					} else {
						throw new PhySigTKException("EHealthParser: unknown datapoint: " + datapoint);
					}
				}
				// TODO: ecg values need to be adapted, scr detection, smoothing...
			}
		}

		public TimeStampedValue<float>[] RetrieveNewEMGValues()
		{
			TimeStampedValue<float>[] values = NewEMGValues.ToArray();
			NewEMGValues.Clear();
			return values;
		}

		public TimeStampedValue<float>[] RetrieveNewECGValues()
		{
			TimeStampedValue<float>[] values = NewECGValues.ToArray();
			NewECGValues.Clear();
			return values;
		}

		public TimeStampedValue<float>[] RetrieveNewSCLValues()
		{
			TimeStampedValue<float>[] values = NewSCLValues.ToArray();
			NewSCLValues.Clear();
			return values;
		}
	}
}
