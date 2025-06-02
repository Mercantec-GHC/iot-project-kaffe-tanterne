#include "HX711.h"

HX711 scale1;
HX711 scale2;

//Adjust pins if needed.
uint8_t scaleWaterDataPin = 4;
uint8_t scaleWaterClockPin = 5;
uint8_t scaleCoffeeDataPin = 2;
uint8_t scaleCoffeeClockPin = 3;

float w1, w2, x1, x2, previous = 0;


void ScaleStart()
{
  scale1.begin(scaleWaterDataPin, scaleWaterClockPin);
  scale2.begin(scaleCoffeeDataPin, scaleCoffeeClockPin);

  Serial.print("UNITS: ");
  Serial.println(scale1.get_units(10));
  Serial.println(scale2.get_units(10));

  //Use calibration file for reference.
  scale1.set_scale(204.895828); 
  scale1.set_offset(510849);
  scale2.set_scale(211.087494);
  scale2.set_offset(584788);
  
  //Print first measurement, to prove connection.
  Serial.print("UNITS: ");
  Serial.print(scale1.get_units(10));
  Serial.print(" || ");
  Serial.println(scale2.get_units(10));
}


void ScaleLoop()
{
  //Read until the scale1 has a stable output.
  w1 = scale1.get_units(10);
  delay(100);
  w2 = scale1.get_units();
  while (abs(w1 - w2) > 10)
  {
     w1 = w2;
     w2 = scale1.get_units();
     delay(100);
  }

  //Read until the scale2 has a stable output.
  x1 = scale2.get_units(10);
  delay(100);
  x2 = scale2.get_units();
  while (abs(x1 - x2) > 10)
  {
     x1 = x2;
     x2 = scale2.get_units();
     delay(100);
  }

  //Print out the measured weight.
  Serial.print("UNITS: ");
  Serial.print(w1);
  Serial.print(" || ");
  Serial.println(x1);
  delay(100);
}

float GetScale1Weight() {
    return w1;
}

float GetScale2Weight() {
    return x1;
}
