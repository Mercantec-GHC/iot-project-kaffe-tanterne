#include <Arduino.h>
#include "Scales.h"
//#include "ScalesCalibration.h"

// put function declarations here:
int myFunction(int, int);

void setup() {
  Serial.begin(9600);
  int result = myFunction(2, 3);
  ScaleStart();
  //ScaleCalibrationStart();
}

void loop() {
  ScaleLoop();
  //ScaleCalibrationLoop();
}

// put function definitions here:
int myFunction(int x, int y) {
  return x + y;
}