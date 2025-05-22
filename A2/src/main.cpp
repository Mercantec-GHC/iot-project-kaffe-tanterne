#include <Arduino.h>
#include "Scales.h"
//#include "ScalesCalibration.h"

// put function declarations here:

void setup() {
  Serial.begin(9600);
  ScaleStart();
  //ScaleCalibrationStart();
}

void loop() {
  ScaleLoop();
  //ScaleCalibrationLoop();
}

// put function definitions here:
