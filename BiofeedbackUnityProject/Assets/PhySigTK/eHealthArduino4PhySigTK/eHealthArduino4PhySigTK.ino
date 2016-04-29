/*
 * Reading script for physiological signals from the
 * eHealth sensor platform for Arduino and Raspberry from Cooking-hacks
 * http://www.libelium.com/
 * for use in the PhySigTK in Unity
 * https://strank.info/prof/software/physsigtk
 *
 * Supports EMG, SCL/EDA, and ECG sensor data currently.
 * Update with 10ms delay, roughly a frequency of 100Hz.
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * Version 0.1
 * Author: Stefan Rank
 */
#include <eHealth.h>

// The setup routine runs once when you press reset:
void setup() {
  Serial.begin(115200);  
}

// The loop routine runs over and over again forever:
void loop() {
  float ECGvolt = eHealth.getECG(); // 0-5 Volt
  float SCLms = eHealth.getSkinConductance(); // microSiemens
  int EMGvolt = eHealth.getEMG(); // 0-5 Volt in 0-1023 integer?
  Serial.print("ECG");
  Serial.print(ECGvolt, 4); // 4 decimal places
  Serial.print("|");
  Serial.print("SCL");
  Serial.print(SCLms, 4); // 4 decimal places
  Serial.print("|");
  Serial.print("EMG");
  Serial.print(EMGvolt);
  Serial.println("");
  delay(10);	// wait in milliseconds
}

