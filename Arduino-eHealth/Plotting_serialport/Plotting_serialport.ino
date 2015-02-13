/*
 *  eHealth sensor platform for Arduino and Raspberry from Cooking-hacks.
 *
 *  Description: "The e-Health Sensor Shield allows Arduino and Raspberry Pi 
 *  users to perform biometric and medical applications by using 9 different 
 *  sensors: Pulse and Oxygen in Blood Sensor (SPO2), Airflow Sensor (Breathing),
 *  Body Temperature, Electrocardiogram Sensor (ECG), Glucometer, Galvanic Skin
 *  Response Sensor (GSR - Sweating), Blood Pressure (Sphygmomanometer) and 
 *  Patient Position (Accelerometer)."
 *
 *  In this example we read the values in volts of EMG sensor and show
 *  these values in the serial monitor. 
 *
 *  Copyright (C) 2012 Libelium Comunicaciones Distribuidas S.L.
 *  http://www.libelium.com
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  Version 2.0
 *  Author: Luis Martin & Ahmad Saad
 */

#include <eHealth.h>
byte message[] = {0xAA, 0x55, 0x0F, 0x2C, 0x10, 0x01, 0x00, 0xEF, 0xFF, 0x25, 0x00, 0xDB, 0x00, 0xAC, 0x00, 0x00, 0xEF};
byte EMG_low, EMG_high, ECG_low, ECG_high, Air_high, Air_low,GSR_low, GSR_high;
int ECGflag = 1;
// The setup routine runs once when you press reset:
void setup() {
  Serial.begin(115200); 
}

// The loop routine runs over and over again forever:
void loop() {

  int EMG = eHealth.getEMG();
  int ECG = (int)(1023*eHealth.getECG()/5); 
  int Air = eHealth.getAirFlow();
  int GSR = (int)(1023*eHealth.getSkinConductanceVoltage()/5); 
//  Air = 10 * Air;
  if (ECGflag == 1)
  {
  ECG_low = lowByte((int)ECG);
  ECG_high = highByte((int)ECG);
  GSR_high = highByte((int)GSR);
  GSR_low = lowByte((int)GSR);
  Air_high = highByte((int)Air);
  Air_low = lowByte((int)Air);
  message[4] = ECG_low;
  message[5] = ECG_high;  
  message[7] = GSR_low;//7
  message[8] = GSR_high; //8
  message[9] = Air_low;//7
  message[10] = Air_high; //8
  message[11] = highByte(0);
  message[12] = lowByte(0);
  }
  else
  {
  EMG_low = lowByte((int)EMG);
  EMG_high = highByte((int)EMG);
  message[4] = EMG_low;
  message[5] = EMG_high;  
  }
  Serial.write(message, sizeof(message));
  delay(20);	// wait for a millisecond
}

