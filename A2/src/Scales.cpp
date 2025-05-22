#include "HX711.h"

HX711 scale1;
HX711 scale2;

//  adjust pins if needed
uint8_t scale1DataPin = 4;
uint8_t scale1ClockPin = 5;
uint8_t scale2DataPin = 2;
uint8_t scale2ClockPin = 3;

float w1, w2, x1, x2, previous = 0;


void ScaleStart()
{
  scale1.begin(scale1DataPin, scale1ClockPin);
  scale2.begin(scale2DataPin, scale2ClockPin);

  Serial.print("UNITS: ");
  Serial.println(scale1.get_units(10));
  Serial.println(scale2.get_units(10));

  //use calibration file for reference
  scale1.set_scale(202.247451); 
  scale1.set_offset(352258);
  scale1.tare();
  scale2.set_scale(202.865127);
  scale2.set_offset(354034);
  scale2.tare();
  
  //print first measurement, to prove connection
  Serial.print("UNITS: ");
  Serial.print(scale1.get_units(10));
  Serial.println(scale2.get_units(10));
}


void ScaleLoop()
{
  //read until the scale1 has a stable output
  w1 = scale1.get_units(10);
  delay(100);
  w2 = scale1.get_units();
  while (abs(w1 - w2) > 10)
  {
     w1 = w2;
     w2 = scale1.get_units();
     delay(100);
  }

  //read until the scale2 has a stable output
  x1 = scale2.get_units(10);
  delay(100);
  x2 = scale2.get_units();
  while (abs(x1 - x2) > 10)
  {
     x1 = x2;
     x2 = scale2.get_units();
     delay(100);
  }

  //print out the measured weight
  Serial.print("UNITS: ");
  Serial.print(w1);
  Serial.print(" || ");
  Serial.println(x1);
  delay(100);
}
