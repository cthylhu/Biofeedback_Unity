using System;
using System.Runtime.InteropServices;
using System.Threading;

/// <summary>
/// IOM. Uses exposed dll functions and a reading thread to update values.
/// The thread continuously reads values and calculates heart beat as BPM (beats per minute).
/// See the following sources for the details of calculating this value:
/// Concepts: http://www.cvphysiology.com/Arrhythmias/A009.htm
/// Calculation: http://www.biomedical-engineering-online.com/content/3/1/28
/// and older http://www.engr.wisc.edu/bme/faculty/tompkins_willis/Pan.pdf
/// 
/// Exposed values that are updated while the thread is running:
/// SCL (float, raw skin conductance level)
/// HRV (float, raw heart rate variance)
/// QRS (bool, true if we are probably inside a qrs complex - the peak of the heart signals)
/// BPM (float, calculated heart rate in beats per minute)
/// </summary>
public class IOM
{
	struct hrv_scl
	{
		public float hrv;
		public float scl;
	};
	private hrv_scl hs;
	private float scl = -1f;
	private float hrv = -1f;
	private bool qrs = false;
	private float bpm = -1f;
	private int averagingCount = 5;
	private Thread readUpdateThread;
	private bool threadRunning = false;

	[DllImport ("IOMPlugin")]
	private static extern int iom_setup(); // returns number of iom devices found and sets up the first one.
	
	[DllImport ("IOMPlugin")]
	private static extern hrv_scl iom_get_hrvscl(); // returns both measurements. call iom_setup once first.
	
	[DllImport ("IOMPlugin")]
	private static extern int iom_close(); // deletes the previously set up iom device.

	public int setup() {
		return iom_setup();
	}

	public void setAvgNum(int averagingCount) {
		this.averagingCount = averagingCount;
	}

	public bool startReadingData() {
		if (readUpdateThread != null) { // should not happen, just return alive status
			return readUpdateThread.IsAlive;
		}
		readUpdateThread = new Thread(new ThreadStart(this.readAndUpdateData));
		//readUpdateThread.IsBackground = true; // not used as it potentially hides problems
		readUpdateThread.Start();
		return true;
	}

	public bool isReadingData() {
		if (readUpdateThread != null) {
			return readUpdateThread.IsAlive;
		}
		return false;
	}
	
	public void stopReadingData() {
		if (readUpdateThread != null) {
			this.threadRunning = false; // not using Abort as it's unreliable and messes with unity
			readUpdateThread.Join();
			readUpdateThread = null;
		}
	}

	public float getSCL() {
		return scl;
	}
	
	public float getHRV() {
		return hrv;
	}
	
	public bool getQRS() {
		return qrs;
	}
	
	public float getBPM() {
		return bpm;
	}
	
	public void close() {
		iom_close();
	}

	/// <summary>
	/// To be used as a thread runner method.
	/// In a loop, read new values for hrv and scl from the device,
	/// calculate bpm based on the hrv values, by detecting peaks.
	/// </summary>
	private void readAndUpdateData() {
		this.threadRunning = true; // signal that we are running
		float hrv_sample;
		float prev_hrv_sample = 1.5f; // reasonable default value
		int max_sample_difference = 5;
		float peakAG = 0f;
		float _attack = 0.9875f;
		float _decay = 0.992f;
		float gain;
		float sampleAG;
		float lower_bound = 0.9975f;
		float upper_bound = 0.99f;
		int near_peak = 0;
		int beats = 0;
		int BPM;
		
		long tc;
		long[] tl = new long[50];
		long TICKS_PER_SEC = 10000000; // ticks are in 100-nanosecond intervals
		int index;

		float avgSeg;
		float varSeg;

		while (this.threadRunning) {
			Thread.Sleep(1); // always briefly give up control every loop
			this.hs = iom_get_hrvscl();
			// from the documentation of the lightstone library, the raw values:
			// - HRV - ((AA << 8) | (BB)) * .01 (Returning a value between 1.6-2.5)
			// - SCL - ((CC << 8) | (DD)) * .001 (Returning a value between 3-15)
			// those ranges are not absolute though, lower values are common
			if (hs.hrv < 0) {
				break; // terminates thread loop, invalid data from device
			}
			hrv_sample = hs.hrv;
			this.scl = hs.scl;
			this.hrv = hs.hrv; // TO BE REMOVED WHEN FINISHED WITH BELOW
			if (Math.Abs(hrv_sample - prev_hrv_sample) > max_sample_difference) {
				// disregard hrv samples that are too different from the previous one
				continue;
			}
			prev_hrv_sample = hrv_sample;
			// auto-gain
			if (hrv_sample > peakAG) {
				peakAG = _attack * hrv_sample;
			} else {
				peakAG = _decay * peakAG;
			}
			gain = _attack / peakAG;
			sampleAG = gain * hrv_sample;				
			// peak detect
			if (sampleAG >= lower_bound) {
				near_peak = 1;
			}
			this.qrs = near_peak == 1; // TODO: revisit if this is actually QRS!
			if ((near_peak == 1) && (sampleAG < upper_bound)) {
				near_peak = 0;
				if (beats >= averagingCount) {
					tc = DateTime.Now.Ticks;
					index = beats % averagingCount;
					BPM = (int) (60 * averagingCount * TICKS_PER_SEC / (tc - tl[index]));
					avgSeg = (float) (tc - tl[index]) / averagingCount;
					varSeg = 0;
					for (int p = 0; p < averagingCount; p++) {
						if (p == 0) {
							if (p == index) {
								varSeg += ((tc-tl[averagingCount-1])-avgSeg)*((tc-tl[averagingCount-1])-avgSeg);
							} else {
								varSeg += ((tl[p]-tl[averagingCount-1])-avgSeg)*((tl[p]-tl[averagingCount-1])-avgSeg);
							}
						} else {
							if (p==index) { 
								varSeg += ((tc-tl[p-1])-avgSeg)*((tc-tl[p-1])-avgSeg);
							} else { 
								varSeg += ((tl[p]-tl[p-1])-avgSeg)*((tl[p]-tl[p-1])-avgSeg);
							}
						}
					}
					this.bpm = BPM;
					this.hrv = (float) Math.Sqrt(varSeg / averagingCount) / TICKS_PER_SEC; // TODO: check validity of setting hrv here!
					tl[index] = tc;
				} else {
					tl[beats] = DateTime.Now.Ticks;
				}
				beats++;
			}
		}
	}
}
