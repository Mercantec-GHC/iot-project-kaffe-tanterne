//    FILE: delta_scale.ino
//  AUTHOR: Rob Tillaart
// PURPOSE: HX711 demo
//     URL: https://github.com/RobTillaart/HX711

#include "HX711.h"

HX711 scale1;
HX711 scale2;

//  adjust pins if needed
uint8_t scale1DataPin = 5;
uint8_t scale1ClockPin = 6;
uint8_t scale2DataPin = 10;
uint8_t scale2ClockPin = 11;

float w1, w2, x1, x2, previous = 0;


void setup()
{
  Serial.begin(9600);

  scale1.begin(scale1DataPin, scale1ClockPin);
  scale2.begin(scale2DataPin, scale2ClockPin);

  Serial.print("UNITS: ");
  Serial.println(scale1.get_units(10));
  Serial.println(scale2.get_units(10));

  //use calibration file for reference
  scale1.set_scale(218.611114); 
  scale1.set_offset(509281);
  scale1.tare();
  scale2.set_scale(202.865127);
  scale2.set_offset(354034);
  scale2.tare();
  
  //print first measurement, to prove connection
  Serial.print("UNITS: ");
  Serial.print(scale1.get_units(10));
  Serial.println(scale2.get_units(10));
}


void loop()
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
  Serial.print("");
  Serial.println(x1);
  delay(100);
}
