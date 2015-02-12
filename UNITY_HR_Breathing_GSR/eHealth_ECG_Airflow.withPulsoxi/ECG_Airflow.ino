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
 *  In this example we read the values in volts of ECG sensor and show
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
#include <PinChangeInt.h>
#include <eHealth.h>
int cont = 0;
// The setup routine runs once when you press reset:
void setup() {
  Serial.begin(115200);  
  eHealth.initPulsioximeter();   
  PCintPort::attachInterrupt(6, readPulsioximeter, RISING);
}

// The loop routine runs over and over again forever:
void loop() {

  float ECG = eHealth.getECG();
  int air = eHealth.getAirFlow();
  float Air = (float)air*5/1023;
  float GSR = eHealth.getSkinConductance();
  int HR = eHealth.getBPM();
  Serial.print("B");
  Serial.print(","); 
  Serial.print(ECG, 2); 
  Serial.print(","); 
  Serial.print(Air, 2); 
  Serial.print(","); 
  Serial.print(GSR, 2); 
  Serial.print(","); 
  Serial.println(HR);
  delay(1);	// wait for a millisecond
}
//Include always this code when using the pulsioximeter sensor
//=========================================================================
void readPulsioximeter(){  

  cont ++;

  if (cont == 50) { //Get only of one 50 measures to reduce the latency
    eHealth.readPulsioximeter();  
    cont = 0;
  }
}
